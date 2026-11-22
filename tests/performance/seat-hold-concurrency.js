import http from 'k6/http';
import { check, sleep } from 'k6';
import { Counter } from 'k6/metrics';

const baseUrl = (__ENV.API_BASE_URL || 'http://localhost:8080').replace(/\/$/, '');
const serviceDate = __ENV.SEARCH_DATE || new Date(Date.now() - 5 * 60 * 60 * 1000).toISOString().slice(0, 10);
const origin = __ENV.SEARCH_ORIGIN || 'Ayacucho';
const destination = __ENV.SEARCH_DESTINATION || 'Lima';

const holdSuccess = new Counter('seat_hold_success');
const holdConflict = new Counter('seat_hold_conflict');
const holdUnexpected = new Counter('seat_hold_unexpected');

http.setResponseCallback(http.expectedStatuses(200, 204, 409));

export const options = {
  scenarios: {
    same_seat_contention: {
      executor: 'shared-iterations',
      vus: 50,
      iterations: 50,
      maxDuration: '90s',
    },
  },
  thresholds: {
    seat_hold_success: ['count==1'],
    seat_hold_conflict: ['count==49'],
    seat_hold_unexpected: ['count==0'],
  },
};

export function setup() {
  const query = `origin=${encodeURIComponent(origin)}&destination=${encodeURIComponent(destination)}&date=${encodeURIComponent(serviceDate)}`;
  const searchUrl = `${baseUrl}/api/v1/trips/search?${query}`;
  const search = http.get(searchUrl, { headers: { Accept: 'application/json' } });
  if (search.status !== 200) {
    throw new Error(`Trip search returned HTTP ${search.status}; expected 200.`);
  }
  const trips = search.json();
  if (!Array.isArray(trips) || trips.length === 0) {
    throw new Error(`No seeded trip found for ${origin}-${destination} on ${serviceDate}.`);
  }

  const trip = trips[0];
  const map = http.get(`${baseUrl}/api/v1/trips/${trip.tripId}/seats`, { headers: { Accept: 'application/json' } });
  if (map.status !== 200) {
    throw new Error(`Seat map returned HTTP ${map.status}; expected 200.`);
  }
  const seat = map.json().seats.find((item) => item.status === 'Available');
  if (!seat) {
    throw new Error(`Trip ${trip.tripId} has no available seat for the contention test.`);
  }

  return { tripId: trip.tripId, tripSeatId: seat.tripSeatId };
}

export default function ({ tripId, tripSeatId }) {
  const response = http.post(
    `${baseUrl}/api/v1/reservations/holds`,
    JSON.stringify({ tripId, tripSeatIds: [tripSeatId] }),
    { headers: { 'Content-Type': 'application/json', Accept: 'application/json' } },
  );

  if (response.status === 200) {
    holdSuccess.add(1);
    check(response, { 'hold winner returns HTTP 200': (result) => result.status === 200 });
    const hold = response.json();
    // Let the other 49 requests finish before releasing the winning hold.
    sleep(10);
    const released = http.del(`${baseUrl}/api/v1/reservations/holds/${hold.reservationId}`, null, {
      headers: { 'X-Hold-Token': hold.holdToken },
    });
    check(released, { 'winning hold cleanup returns HTTP 204': (result) => result.status === 204 });
    return;
  }

  if (response.status === 409) {
    holdConflict.add(1);
    check(response, { 'contending hold returns HTTP 409': (result) => result.status === 409 });
    return;
  }

  holdUnexpected.add(1);
  check(response, { 'hold returns either HTTP 200 or HTTP 409': (result) => result.status === 200 || result.status === 409 });
}
