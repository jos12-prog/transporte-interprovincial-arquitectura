import type { AuthSession } from '@/api/identity';
import type { SeatLayoutPosition } from '@/components/seats/SeatLayoutView';

export interface AgencyRoute { routeId: string; originCity: string; destinationCity: string; isActive: boolean }
export interface AgencyVehicle { vehicleId: string; vehicleModelId: string; plate: string; displayName: string; vehicleModelName: string; isActive: boolean; seatLayout?: SeatLayoutPosition[] }
export interface AgencyFare { fareId: string; routeId: string; type: 'Base' | 'Special'; validFrom: string; validTo: string | null; price: number; currency: string; isActive: boolean }
export interface AgencyTrip { tripId: string; routeId: string; vehicleId: string; origin: string; destination: string; vehicleDisplayName: string; vehicleModelName: string; departureAtUtc: string; arrivalAtUtc: string; price: number; currency: string; status: 'Scheduled' | 'Cancelled' }
export interface AgencyVehicleModel { vehicleModelId: string; name: string; category: string | null; capacity: number; isActive: boolean; seats: Array<{ number: number; floor: number; row: number | null; column: number | null }> }

async function request<T>(session: AuthSession, agencyId: string, path: string, init: RequestInit = {}): Promise<T> {
  const response = await fetch(`/api/v1/admin/agencies/${agencyId}${path}`, {
    ...init,
    headers: { Authorization: `Bearer ${session.accessToken}`, 'Content-Type': 'application/json', ...init.headers },
  });
  if (response.status === 204) return undefined as T;
  const data = await response.json().catch(() => ({}));
  if (!response.ok) throw new Error(data.detail ?? data.title ?? `Error HTTP ${response.status}`);
  return data as T;
}

export const agencyCatalogApi = {
  routes: (s: AuthSession, a: string) => request<AgencyRoute[]>(s, a, '/routes'),
  vehicles: (s: AuthSession, a: string) => request<AgencyVehicle[]>(s, a, '/vehicles'),
  trips: (s: AuthSession, a: string) => request<AgencyTrip[]>(s, a, '/trips'),
  fares: (s: AuthSession, a: string, routeId: string) => request<AgencyFare[]>(s, a, `/routes/${routeId}/fares`),
  models: (s: AuthSession, a: string) => request<AgencyVehicleModel[]>(s, a, '/vehicle-models'),
  createRoute: (s: AuthSession, a: string, originCity: string, destinationCity: string) => request<AgencyRoute>(s, a, '/routes', { method: 'POST', body: JSON.stringify({ originCity, destinationCity }) }),
  updateRoute: (s: AuthSession, a: string, id: string, originCity: string, destinationCity: string) => request<AgencyRoute>(s, a, `/routes/${id}`, { method: 'PUT', body: JSON.stringify({ originCity, destinationCity }) }),
  setRouteActive: (s: AuthSession, a: string, id: string, isActive: boolean) => request<AgencyRoute>(s, a, `/routes/${id}/active`, { method: 'PATCH', body: JSON.stringify({ isActive }) }),
  createVehicle: (s: AuthSession, a: string, body: { plate: string; displayName: string; vehicleModelId: string }) => request<AgencyVehicle>(s, a, '/vehicles', { method: 'POST', body: JSON.stringify(body) }),
  updateVehicle: (s: AuthSession, a: string, id: string, body: { plate: string; displayName: string; vehicleModelId: string }) => request<AgencyVehicle>(s, a, `/vehicles/${id}`, { method: 'PUT', body: JSON.stringify(body) }),
  setVehicleActive: (s: AuthSession, a: string, id: string, isActive: boolean) => request<AgencyVehicle>(s, a, `/vehicles/${id}/active`, { method: 'PATCH', body: JSON.stringify({ isActive }) }),
  scheduleTrip: (s: AuthSession, a: string, body: { routeId: string; vehicleId: string; departureAtUtc: string; arrivalAtUtc: string }) => request<AgencyTrip>(s, a, '/trips', { method: 'POST', body: JSON.stringify(body) }),
  createFare: (s: AuthSession, a: string, routeId: string, body: { type: string; validFrom: string; validTo: string | null; price: number; currency: string }) => request<AgencyFare>(s, a, `/routes/${routeId}/fares`, { method: 'POST', body: JSON.stringify(body) }),
  updateFare: (s: AuthSession, a: string, routeId: string, fareId: string, body: { type: string; validFrom: string; validTo: string | null; price: number; currency: string }) => request<AgencyFare>(s, a, `/routes/${routeId}/fares/${fareId}`, { method: 'PUT', body: JSON.stringify(body) }),
  setFareActive: (s: AuthSession, a: string, routeId: string, fareId: string, isActive: boolean) => request<AgencyFare>(s, a, `/routes/${routeId}/fares/${fareId}/active`, { method: 'PATCH', body: JSON.stringify({ isActive }) }),
  cancelTrip: (s: AuthSession, a: string, id: string) => request<AgencyTrip>(s, a, `/trips/${id}/cancel`, { method: 'POST' }),
};
