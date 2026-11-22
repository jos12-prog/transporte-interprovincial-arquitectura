import { ArrowLeft, ArrowRight, BusFront, Clock3, SearchX } from 'lucide-react';
import { FormEvent, useEffect, useMemo, useState } from 'react';
import { Link, useSearchParams } from 'react-router-dom';
import { formatDuration, formatLimaDate, formatLimaTime, formatPrice, searchTrips, TripSearchCriteria, TripSearchResult } from '@/api/trips';

function readCriteria(params: URLSearchParams): TripSearchCriteria {
  return {
    origin: params.get('origin') ?? '',
    destination: params.get('destination') ?? '',
    date: params.get('date') ?? '',
    companyId: params.get('companyId') || undefined,
    agencyId: params.get('agencyId') || undefined,
    departureFrom: params.get('departureFrom') || undefined,
    departureTo: params.get('departureTo') || undefined,
    minPrice: params.get('minPrice') || undefined,
    maxPrice: params.get('maxPrice') || undefined,
  };
}

function TripCard({ trip }: { trip: TripSearchResult }) {
  const initials = trip.companyName.split(/\s+/).map(part => part[0]).join('').slice(0, 2).toUpperCase();

  return (
    <article className="grid gap-4 rounded-card border border-neutral-200 bg-white p-4 shadow-sm sm:p-5 lg:grid-cols-[180px_1fr_180px] lg:items-center">
      <div className="flex items-start gap-3">
        <span aria-hidden="true" className="grid h-11 w-11 shrink-0 place-items-center rounded-button bg-primary-700 font-display font-extrabold text-white">{initials}</span>
        <div className="min-w-0">
          <h2 className="font-semibold text-neutral-900">{trip.companyName}</h2>
          <p className="mt-1 text-sm text-neutral-600">{trip.agencyName}</p>
          <p className="text-xs text-neutral-600">{trip.agencyCity}</p>
        </div>
      </div>

      <div>
        <div className="grid grid-cols-[auto_1fr_auto] items-center gap-3">
          <div>
            <p className="font-display text-xl font-bold tabular-nums text-neutral-900">{formatLimaTime(trip.departureAtUtc)}</p>
            <p className="text-sm text-neutral-600">{trip.origin}</p>
          </div>
          <div className="relative border-t border-dashed border-neutral-200 text-center">
            <span className="absolute -top-3 left-1/2 -translate-x-1/2 bg-white px-2 text-xs text-neutral-600">{formatDuration(trip.departureAtUtc, trip.arrivalAtUtc)}</span>
            <ArrowRight aria-hidden="true" className="absolute -right-1 -top-2.5 bg-white text-primary-600" size={16} />
          </div>
          <div className="text-right">
            <p className="font-display text-xl font-bold tabular-nums text-neutral-900">{formatLimaTime(trip.arrivalAtUtc)}</p>
            <p className="text-sm text-neutral-600">{trip.destination}</p>
          </div>
        </div>
        <div className="mt-4 flex flex-wrap items-center gap-2">
          <span className="inline-flex items-center gap-1.5 rounded-pill bg-neutral-50 px-3 py-1.5 text-sm text-neutral-600">
            <BusFront aria-hidden="true" size={16} className="text-primary-700" /> {trip.vehicleModelName}
          </span>
          <span className="inline-flex items-center gap-1.5 rounded-pill bg-primary-100 px-3 py-1.5 text-sm font-medium text-primary-700">
            <Clock3 aria-hidden="true" size={15} /> {trip.availableSeats} asientos libres
          </span>
        </div>
      </div>

      <div className="flex items-center justify-between border-t border-dashed border-neutral-200 pt-3 sm:justify-end sm:gap-5 lg:h-full lg:flex-col lg:items-end lg:justify-center lg:border-l lg:border-t-0 lg:pl-5 lg:pt-0">
        <div className="lg:text-right">
          <p className="font-display text-2xl font-extrabold tabular-nums text-neutral-900">{formatPrice(trip.price, trip.currency)}</p>
          <p className="text-xs text-neutral-600">por pasajero</p>
        </div>
        <Link to={`/pasajero/viajes/${encodeURIComponent(trip.tripId)}/asientos`} className="inline-flex h-10 items-center justify-center rounded-button bg-primary-600 px-4 text-sm font-semibold text-white hover:bg-primary-700">
          Elegir asientos
        </Link>
      </div>
    </article>
  );
}

function TripSkeleton() {
  return (
    <div role="status" aria-label="Cargando viajes" className="space-y-4">
      {[1, 2, 3].map(item => (
        <div key={item} aria-hidden="true" className="animate-pulse rounded-card border border-neutral-200 bg-white p-5">
          <div className="flex gap-4"><span className="h-11 w-11 rounded-button bg-neutral-200" /><span className="h-4 w-36 rounded bg-neutral-200" /></div>
          <div className="mt-6 h-5 w-3/4 rounded bg-neutral-200" />
          <div className="mt-4 h-4 w-1/2 rounded bg-neutral-100" />
        </div>
      ))}
      <span className="sr-only">Cargando resultados de viajes</span>
    </div>
  );
}

export function TripResultsPage() {
  const [searchParams, setSearchParams] = useSearchParams();
  const queryString = searchParams.toString();
  const criteria = useMemo(() => readCriteria(new URLSearchParams(queryString)), [queryString]);
  const [trips, setTrips] = useState<TripSearchResult[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [retry, setRetry] = useState(0);
  const [sortBy, setSortBy] = useState<'departure' | 'price'>('departure');
  const [companyId, setCompanyId] = useState(criteria.companyId ?? '');
  const [agencyId, setAgencyId] = useState(criteria.agencyId ?? '');
  const [departureFrom, setDepartureFrom] = useState(criteria.departureFrom ?? '');
  const [departureTo, setDepartureTo] = useState(criteria.departureTo ?? '');
  const [minPrice, setMinPrice] = useState(criteria.minPrice ?? '');
  const [maxPrice, setMaxPrice] = useState(criteria.maxPrice ?? '');

  useEffect(() => {
    setCompanyId(criteria.companyId ?? '');
    setAgencyId(criteria.agencyId ?? '');
    setDepartureFrom(criteria.departureFrom ?? '');
    setDepartureTo(criteria.departureTo ?? '');
    setMinPrice(criteria.minPrice ?? '');
    setMaxPrice(criteria.maxPrice ?? '');
  }, [criteria]);

  useEffect(() => {
    const controller = new AbortController();
    if (!criteria.origin || !criteria.destination || !criteria.date) {
      setTrips([]);
      setError('Completa origen, destino y fecha para consultar las salidas.');
      setLoading(false);
      return () => controller.abort();
    }

    setLoading(true);
    setError(null);
    searchTrips(criteria, controller.signal)
      .then(setTrips)
      .catch(reason => {
        if (!controller.signal.aborted) {
          setError(reason instanceof Error ? reason.message : 'No pudimos buscar viajes. Inténtalo de nuevo.');
          setTrips([]);
        }
      })
      .finally(() => {
        if (!controller.signal.aborted) setLoading(false);
      });
    return () => controller.abort();
  }, [criteria, retry]);

  function applyFilters(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    const next = new URLSearchParams({ origin: criteria.origin, destination: criteria.destination, date: criteria.date });
    const filters = { companyId, agencyId, departureFrom, departureTo, minPrice, maxPrice };
    Object.entries(filters).forEach(([key, value]) => {
      if (value) next.set(key, value);
    });
    setSearchParams(next);
  }

  const sortedTrips = [...trips].sort((left, right) => sortBy === 'price'
    ? left.price - right.price
    : new Date(left.departureAtUtc).getTime() - new Date(right.departureAtUtc).getTime());
  const companies = [...new Map(trips.map(trip => [trip.companyId, trip.companyName])).entries()];
  const agencies = [...new Map(trips.map(trip => [trip.agencyId, trip.agencyName])).entries()];

  if (!criteria.origin || !criteria.destination || !criteria.date) {
    return (
      <section className="mx-auto max-w-container rounded-card border border-neutral-200 bg-white p-8 text-center shadow-sm">
        <SearchX aria-hidden="true" className="mx-auto text-primary-600" size={36} />
        <h1 className="mt-3 font-display text-2xl font-bold text-neutral-900">Prepara tu búsqueda</h1>
        <p className="mt-2 text-neutral-600">Elige origen, destino y fecha para ver viajes disponibles.</p>
        <Link to="/pasajero" className="mt-5 inline-flex h-12 items-center gap-2 rounded-button bg-primary-600 px-5 font-semibold text-white hover:bg-primary-700">
          <ArrowLeft aria-hidden="true" size={18} /> Buscar viajes
        </Link>
      </section>
    );
  }

  return (
    <div className="mx-auto max-w-container px-page-gutter pb-12 pt-8">
      <div className="mb-6 flex flex-col gap-4 lg:flex-row lg:items-end lg:justify-between">
        <div>
          <p className="text-sm text-neutral-600">Buscar viajes <span aria-hidden="true">/</span> Resultados</p>
          <h1 className="mt-1 font-display text-3xl font-bold text-neutral-900">{criteria.origin} <span aria-hidden="true">→</span> {criteria.destination}</h1>
          <p className="mt-1 text-sm text-neutral-600">{formatLimaDate(criteria.date)} · {trips.length} {trips.length === 1 ? 'salida disponible' : 'salidas disponibles'}</p>
        </div>
        <div className="flex flex-wrap items-center gap-3">
          <Link to="/pasajero" className="inline-flex h-10 items-center gap-2 rounded-button px-3 text-sm font-semibold text-primary-700 hover:bg-primary-100">
            <ArrowLeft aria-hidden="true" size={16} /> Modificar búsqueda
          </Link>
          <label className="flex items-center gap-2 text-sm text-neutral-600">
            Ordenar
            <select aria-label="Ordenar viajes" value={sortBy} onChange={event => setSortBy(event.target.value as 'departure' | 'price')} className="h-10 rounded-button border border-neutral-200 bg-white px-3 text-neutral-900">
              <option value="departure">Más temprano</option>
              <option value="price">Menor precio</option>
            </select>
          </label>
        </div>
      </div>

      <div className="grid gap-5 lg:grid-cols-[260px_1fr] lg:items-start">
        <aside aria-label="Filtros" className="h-fit rounded-card border border-neutral-200 bg-white shadow-sm">
          <form onSubmit={applyFilters}>
            <div className="border-b border-neutral-200 p-5">
              <h2 className="font-display text-base font-bold text-neutral-900">Filtros</h2>
              <label className="mt-4 block text-sm font-semibold text-neutral-600">
                Empresa
                <select aria-label="Empresa" value={companyId} onChange={event => setCompanyId(event.target.value)} className="mt-2 h-11 w-full rounded-button border border-neutral-200 bg-white px-3 font-normal text-neutral-900">
                  <option value="">Todas las empresas</option>
                  {companies.map(([id, name]) => <option key={id} value={id}>{name}</option>)}
                </select>
              </label>
              <label className="mt-4 block text-sm font-semibold text-neutral-600">
                Agencia
                <select aria-label="Agencia" value={agencyId} onChange={event => setAgencyId(event.target.value)} className="mt-2 h-11 w-full rounded-button border border-neutral-200 bg-white px-3 font-normal text-neutral-900">
                  <option value="">Todas las agencias</option>
                  {agencies.map(([id, name]) => <option key={id} value={id}>{name}</option>)}
                </select>
              </label>
            </div>
            <div className="border-b border-neutral-200 p-5">
              <h3 className="text-sm font-semibold text-neutral-900">Horario de salida</h3>
              <div className="mt-3 grid grid-cols-2 gap-2">
                <label className="text-xs text-neutral-600">Desde<input aria-label="Salida desde" type="time" value={departureFrom} onChange={event => setDepartureFrom(event.target.value)} className="mt-1 h-10 w-full rounded-button border border-neutral-200 px-2 text-sm text-neutral-900" /></label>
                <label className="text-xs text-neutral-600">Hasta<input aria-label="Salida hasta" type="time" value={departureTo} onChange={event => setDepartureTo(event.target.value)} className="mt-1 h-10 w-full rounded-button border border-neutral-200 px-2 text-sm text-neutral-900" /></label>
              </div>
            </div>
            <div className="p-5">
              <h3 className="text-sm font-semibold text-neutral-900">Precio por pasajero</h3>
              <div className="mt-3 grid grid-cols-2 gap-2">
                <label className="text-xs text-neutral-600">Mínimo<input aria-label="Precio mínimo" type="number" min="0" step="0.01" value={minPrice} onChange={event => setMinPrice(event.target.value)} placeholder="S/ min" className="mt-1 h-10 w-full rounded-button border border-neutral-200 px-2 text-sm text-neutral-900" /></label>
                <label className="text-xs text-neutral-600">Máximo<input aria-label="Precio máximo" type="number" min="0" step="0.01" value={maxPrice} onChange={event => setMaxPrice(event.target.value)} placeholder="S/ max" className="mt-1 h-10 w-full rounded-button border border-neutral-200 px-2 text-sm text-neutral-900" /></label>
              </div>
            </div>
            <div className="px-5 pb-5">
              <button type="submit" className="h-11 w-full rounded-button bg-primary-600 px-4 font-semibold text-white hover:bg-primary-700">Aplicar filtros</button>
            </div>
          </form>
        </aside>

        <section aria-label="Resultados de viajes" aria-live="polite" className="space-y-4">
          {loading && <TripSkeleton />}
          {!loading && error && (
            <div role="alert" className="rounded-card border border-red-200 bg-red-50 p-5 text-danger-600">
              <p className="font-semibold">No pudimos completar la búsqueda</p>
              <p className="mt-1 text-sm">{error}</p>
              <button type="button" onClick={() => setRetry(value => value + 1)} className="mt-4 h-10 rounded-button bg-white px-4 text-sm font-semibold text-danger-600 ring-1 ring-red-200">Intentar de nuevo</button>
            </div>
          )}
          {!loading && !error && sortedTrips.length === 0 && (
            <div className="rounded-card border border-neutral-200 bg-white px-6 py-12 text-center shadow-sm">
              <SearchX aria-hidden="true" className="mx-auto text-primary-600" size={42} />
              <h2 className="mt-4 font-display text-xl font-bold text-neutral-900">No encontramos salidas para esta ruta</h2>
              <p className="mx-auto mt-2 max-w-md text-sm text-neutral-600">Prueba con otra fecha o modifica el origen y el destino para consultar más viajes.</p>
              <Link to="/pasajero" className="mt-5 inline-flex h-11 items-center gap-2 rounded-button bg-primary-600 px-4 text-sm font-semibold text-white hover:bg-primary-700">Nueva búsqueda</Link>
            </div>
          )}
          {!loading && !error && sortedTrips.map(trip => <TripCard key={trip.tripId} trip={trip} />)}
          {!loading && !error && trips.length > 0 && <p className="px-1 text-xs text-neutral-600">La cantidad de asientos libres es informativa y puede cambiar.</p>}
        </section>
      </div>
    </div>
  );
}
