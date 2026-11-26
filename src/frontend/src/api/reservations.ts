export type SeatState = 'Available' | 'Held' | 'Occupied';

export interface TripSeatMapSeat {
  tripSeatId: string;
  number: number;
  row: number | null;
  column: number | null;
  floor: number;
  status: SeatState;
}

export interface TripSeatMap {
  tripId: string;
  seats: TripSeatMapSeat[];
  vehicleDisplayName?: string;
}

export interface ReservationHold {
  reservationId: string;
  holdToken: string;
  expiresAtUtc: string;
}

interface ProblemResponse {
  title?: string;
  detail?: string;
}

async function ensureSuccess(response: Response): Promise<void> {
  if (response.ok) return;
  const problem = await response.json().catch(() => ({})) as ProblemResponse;
  throw new Error(problem.detail ?? problem.title ?? `La solicitud falló (${response.status}).`);
}

export async function getTripSeatMap(tripId: string, signal?: AbortSignal): Promise<TripSeatMap> {
  const response = await fetch(`/api/v1/trips/${encodeURIComponent(tripId)}/seats`, {
    headers: { Accept: 'application/json' },
    signal,
  });
  await ensureSuccess(response);
  return await response.json() as TripSeatMap;
}

export async function createReservationHold(tripId: string, tripSeatIds: string[]): Promise<ReservationHold> {
  const response = await fetch('/api/v1/reservations/holds', {
    method: 'POST',
    headers: { Accept: 'application/json', 'Content-Type': 'application/json' },
    body: JSON.stringify({ tripId, tripSeatIds }),
  });
  await ensureSuccess(response);
  return await response.json() as ReservationHold;
}

export async function releaseReservationHold(reservationId: string, holdToken: string): Promise<void> {
  const response = await fetch(`/api/v1/reservations/holds/${encodeURIComponent(reservationId)}`, {
    method: 'DELETE',
    headers: { Accept: 'application/json', 'X-Hold-Token': holdToken },
  });
  await ensureSuccess(response);
}
