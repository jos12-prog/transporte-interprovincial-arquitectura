export {};

interface ScheduledTripSummary {
  tripId: string;
  companyId: string;
  agencyId: string;
}

interface SeatMapResponse {
  tripId: string;
  seats: Array<{ tripSeatId: string; number: number; status: 'Available' | 'Held' | 'Occupied' }>;
}

interface HoldResponse {
  reservationId: string;
  holdToken: string;
  expiresAtUtc: string;
}

function limaServiceDate(): string {
  return new Date(Date.now() - 5 * 60 * 60 * 1000).toISOString().slice(0, 10);
}

describe('seat map and hold API contracts', () => {
  const baseUrl = (process.env.API_BASE_URL ?? 'http://127.0.0.1:8080').replace(/\/$/, '');

  it('returns seat layout, validates the 10-seat maximum, reports conflicts, and releases with 204', async () => {
    const search = new URLSearchParams({ origin: 'Ayacucho', destination: 'Lima', date: limaServiceDate() });
    const tripsResponse = await fetch(`${baseUrl}/api/v1/trips/search?${search}`);
    expect(tripsResponse.status).toBe(200);
    const trips = await tripsResponse.json() as ScheduledTripSummary[];
    expect(trips.length).toBeGreaterThan(0);
    const tripId = trips[0].tripId;

    const mapResponse = await fetch(`${baseUrl}/api/v1/trips/${tripId}/seats`);
    expect(mapResponse.status).toBe(200);
    const map = await mapResponse.json() as SeatMapResponse;
    expect(map.tripId).toBe(tripId);
    expect(map.seats.length).toBeGreaterThanOrEqual(44);
    expect(map.seats[0]).toEqual(expect.objectContaining({ tripSeatId: expect.any(String), number: expect.any(Number), status: 'Available' }));
    expect(JSON.stringify(map)).not.toMatch(/passenger|email|phone/i);

    const available = map.seats.filter(seat => seat.status === 'Available');
    expect(available.length).toBeGreaterThanOrEqual(11);
    const tenSeatResponse = await fetch(`${baseUrl}/api/v1/reservations/holds`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json', Accept: 'application/json' },
      body: JSON.stringify({ tripId, tripSeatIds: available.slice(0, 10).map(seat => seat.tripSeatId) }),
    });
    expect(tenSeatResponse.status).toBe(200);
    const hold = await tenSeatResponse.json() as HoldResponse;
    expect(hold.reservationId).toEqual(expect.any(String));
    expect(hold.holdToken).toEqual(expect.any(String));
    expect(new Date(hold.expiresAtUtc).getTime()).toBeGreaterThan(Date.now());

    const elevenSeatResponse = await fetch(`${baseUrl}/api/v1/reservations/holds`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json', Accept: 'application/json' },
      body: JSON.stringify({ tripId, tripSeatIds: available.slice(10, 11).map(seat => seat.tripSeatId).concat(available.slice(0, 10).map(seat => seat.tripSeatId)) }),
    });
    expect(elevenSeatResponse.status).toBe(400);
    expect(elevenSeatResponse.headers.get('content-type')).toContain('application/problem+json');
    expect((await elevenSeatResponse.json()).status).toBe(400);

    const conflictResponse = await fetch(`${baseUrl}/api/v1/reservations/holds`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json', Accept: 'application/json' },
      body: JSON.stringify({ tripId, tripSeatIds: [available[0].tripSeatId] }),
    });
    expect(conflictResponse.status).toBe(409);
    expect(conflictResponse.headers.get('content-type')).toContain('application/problem+json');

    const wrongTokenResponse = await fetch(`${baseUrl}/api/v1/reservations/holds/${hold.reservationId}`, {
      method: 'DELETE', headers: { 'X-Hold-Token': 'not-the-token' },
    });
    expect(wrongTokenResponse.status).toBe(404);

    const releaseResponse = await fetch(`${baseUrl}/api/v1/reservations/holds/${hold.reservationId}`, {
      method: 'DELETE', headers: { 'X-Hold-Token': hold.holdToken },
    });
    expect(releaseResponse.status).toBe(204);
    expect(await releaseResponse.text()).toBe('');

    const missingMapResponse = await fetch(`${baseUrl}/api/v1/trips/00000000-0000-0000-0000-000000000001/seats`);
    expect(missingMapResponse.status).toBe(404);
    expect(missingMapResponse.headers.get('content-type')).toContain('application/problem+json');
  });

  it('documents the seat and hold endpoints in Swagger including 204', async () => {
    const response = await fetch(`${baseUrl}/swagger/v1/swagger.json`);
    expect(response.status).toBe(200);
    const swagger = await response.json() as { paths: Record<string, Record<string, { responses: Record<string, unknown> }>> };
    expect(swagger.paths['/api/v1/trips/{tripId}/seats']?.get.responses).toHaveProperty('200');
    expect(swagger.paths['/api/v1/reservations/holds']?.post.responses).toHaveProperty('400');
    expect(swagger.paths['/api/v1/reservations/holds/{id}']?.delete.responses).toHaveProperty('204');
  });
});
