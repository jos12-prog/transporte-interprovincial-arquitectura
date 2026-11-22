export interface TripSearchCriteria {
  origin: string;
  destination: string;
  date: string;
  companyId?: string;
  agencyId?: string;
  departureFrom?: string;
  departureTo?: string;
  minPrice?: string;
  maxPrice?: string;
}

export interface TripSearchResult {
  tripId: string;
  companyId: string;
  companyName: string;
  agencyId: string;
  agencyName: string;
  agencyCity: string;
  origin: string;
  destination: string;
  departureAtUtc: string;
  arrivalAtUtc: string;
  vehicleModelName: string;
  price: number;
  currency: string;
  availableSeats: number;
}

interface ProblemResponse {
  title?: string;
  detail?: string;
}

export async function searchTrips(criteria: TripSearchCriteria, signal?: AbortSignal): Promise<TripSearchResult[]> {
  const query = new URLSearchParams();
  Object.entries(criteria).forEach(([key, value]) => {
    if (value !== undefined && value !== '') query.set(key, value);
  });

  const response = await fetch(`/api/v1/trips/search?${query.toString()}`, {
    headers: { Accept: 'application/json' },
    signal,
  });

  if (!response.ok) {
    const problem = await response.json().catch(() => ({})) as ProblemResponse;
    throw new Error(problem.detail ?? problem.title ?? 'No pudimos buscar viajes. Inténtalo de nuevo.');
  }

  return await response.json() as TripSearchResult[];
}

export function limaToday(): string {
  const parts = new Intl.DateTimeFormat('en-US', {
    timeZone: 'America/Lima',
    year: 'numeric',
    month: '2-digit',
    day: '2-digit',
  }).formatToParts(new Date());
  const part = (type: Intl.DateTimeFormatPartTypes) => parts.find(item => item.type === type)?.value ?? '';
  return `${part('year')}-${part('month')}-${part('day')}`;
}

export function formatLimaTime(utcDate: string): string {
  return new Intl.DateTimeFormat('es-PE', {
    timeZone: 'America/Lima',
    hour: '2-digit',
    minute: '2-digit',
    hourCycle: 'h23',
  }).format(new Date(utcDate));
}

export function formatLimaDate(date: string): string {
  return new Intl.DateTimeFormat('es-PE', {
    timeZone: 'America/Lima',
    weekday: 'short',
    day: 'numeric',
    month: 'short',
    year: 'numeric',
  }).format(new Date(`${date}T12:00:00-05:00`));
}

export function formatPrice(price: number, currency = 'PEN'): string {
  return new Intl.NumberFormat('es-PE', { style: 'currency', currency }).format(price);
}

export function formatDuration(departure: string, arrival: string): string {
  const minutes = Math.max(0, Math.round((new Date(arrival).getTime() - new Date(departure).getTime()) / 60_000));
  const hours = Math.floor(minutes / 60);
  const remainingMinutes = minutes % 60;
  return remainingMinutes ? `${hours} h ${remainingMinutes} min` : `${hours} h`;
}
