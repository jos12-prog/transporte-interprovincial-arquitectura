import { ArrowLeftRight, ArrowRight, CalendarDays, MapPin, Search } from 'lucide-react';
import { FormEvent, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import ayacuchoImage from '@/assets/search/fondo.jpg';
import limaImage from '@/assets/search/fondo-2.jpg';
import huantaImage from '@/assets/search/fondo-3.jpg';
import cuscoImage from '@/assets/search/fondo-4.jpg';
import { limaToday } from '@/api/trips';

const cities = ['Ayacucho', 'Lima', 'Huanta', 'Andahuaylas', 'Cusco'];
const popularRoutes = [
  { origin: 'Ayacucho', destination: 'Lima', image: limaImage, detail: 'Salidas diarias' },
  { origin: 'Lima', destination: 'Huanta', image: ayacuchoImage, detail: 'Conecta con el valle' },
  { origin: 'Huanta', destination: 'Andahuaylas', image: huantaImage, detail: 'Viaja por los Andes' },
  { origin: 'Cusco', destination: 'Ayacucho', image: cuscoImage, detail: 'Descubre el sur andino' },
];

export function TripSearchPage() {
  const navigate = useNavigate();
  const [origin, setOrigin] = useState('Ayacucho');
  const [destination, setDestination] = useState('Lima');
  const [date, setDate] = useState(limaToday());
  const [error, setError] = useState<string | null>(null);

  function goToResults(routeOrigin: string, routeDestination: string, routeDate: string) {
    const query = new URLSearchParams({ origin: routeOrigin, destination: routeDestination, date: routeDate });
    navigate(`/pasajero/resultados?${query.toString()}`);
  }

  function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    if (origin === destination) {
      setError('El origen y el destino deben ser diferentes.');
      return;
    }
    setError(null);
    goToResults(origin, destination, date);
  }

  function swapRoute() {
    setOrigin(destination);
    setDestination(origin);
    setError(null);
  }

  return (
    <div>
      <section
        className="relative overflow-hidden bg-primary-900 bg-cover bg-center text-white"
        style={{ backgroundImage: `linear-gradient(90deg, rgba(6,63,54,.96), rgba(6,63,54,.66) 58%, rgba(6,63,54,.2)), url(${ayacuchoImage})` }}
      >
        <div className="mx-auto max-w-container px-page-gutter pb-28 pt-12 sm:pb-32 sm:pt-16">
          <p className="text-sm font-semibold text-accent-500">Viaja seguro por el Perú</p>
          <h1 className="mt-2 max-w-2xl font-display text-3xl font-extrabold leading-tight sm:text-5xl">
            ¿A dónde viajamos esta vez?
          </h1>
          <p className="mt-3 max-w-xl text-base leading-relaxed text-white/85">
            Compara horarios de varias empresas y encuentra el viaje que mejor se acomoda a ti.
          </p>
        </div>
      </section>

      <div className="relative mx-auto -mt-16 max-w-container px-page-gutter sm:-mt-20">
        <form onSubmit={handleSubmit} aria-label="Buscar viajes" className="grid gap-3 rounded-card border border-neutral-200 bg-white p-4 shadow-md sm:grid-cols-[1fr_auto_1fr_1fr_auto] sm:items-end sm:p-5">
          <label className="block text-sm font-semibold text-neutral-600">
            Origen
            <span className="relative mt-2 block">
              <MapPin aria-hidden="true" className="pointer-events-none absolute left-3 top-1/2 -translate-y-1/2 text-primary-600" size={18} />
              <select aria-label="Origen" className="h-12 w-full rounded-button border border-neutral-200 bg-white pl-10 pr-3 text-base font-normal text-neutral-900 outline-none focus:border-primary-600 focus:ring-4 focus:ring-primary-100" value={origin} onChange={event => setOrigin(event.target.value)}>
                {cities.map(city => <option key={city}>{city}</option>)}
              </select>
            </span>
          </label>
          <button type="button" onClick={swapRoute} aria-label="Intercambiar origen y destino" className="flex h-10 w-10 items-center justify-center justify-self-end rounded-button border border-neutral-200 text-primary-700 hover:bg-primary-100 sm:mb-1 sm:justify-self-center">
            <ArrowLeftRight aria-hidden="true" size={18} />
          </button>
          <label className="block text-sm font-semibold text-neutral-600">
            Destino
            <span className="relative mt-2 block">
              <MapPin aria-hidden="true" className="pointer-events-none absolute left-3 top-1/2 -translate-y-1/2 text-primary-600" size={18} />
              <select aria-label="Destino" className="h-12 w-full rounded-button border border-neutral-200 bg-white pl-10 pr-3 text-base font-normal text-neutral-900 outline-none focus:border-primary-600 focus:ring-4 focus:ring-primary-100" value={destination} onChange={event => setDestination(event.target.value)}>
                {cities.map(city => <option key={city}>{city}</option>)}
              </select>
            </span>
          </label>
          <label className="block text-sm font-semibold text-neutral-600">
            Fecha de viaje
            <span className="relative mt-2 block">
              <CalendarDays aria-hidden="true" className="pointer-events-none absolute left-3 top-1/2 -translate-y-1/2 text-primary-600" size={18} />
              <input aria-label="Fecha de viaje" type="date" min={limaToday()} required value={date} onChange={event => setDate(event.target.value)} className="h-12 w-full rounded-button border border-neutral-200 bg-white pl-10 pr-3 text-base font-normal text-neutral-900 outline-none focus:border-primary-600 focus:ring-4 focus:ring-primary-100" />
            </span>
          </label>
          <button type="submit" className="flex h-12 items-center justify-center gap-2 rounded-button bg-primary-600 px-5 font-semibold text-white transition hover:bg-primary-700 focus:outline-none focus:ring-4 focus:ring-primary-100">
            <Search aria-hidden="true" size={18} /> Buscar viajes <ArrowRight aria-hidden="true" size={18} />
          </button>
        </form>
        {error && <p className="mt-3 rounded-button bg-red-50 px-4 py-3 text-sm font-medium text-danger-600" role="alert">{error}</p>}
      </div>

      <section className="mx-auto mt-12 max-w-container px-page-gutter pb-12">
        <div className="mb-5 flex items-end justify-between gap-4">
          <div>
            <h2 className="font-display text-2xl font-bold text-neutral-900">Rutas para descubrir</h2>
            <p className="mt-1 text-sm text-neutral-600">Elige una ruta y consulta las salidas disponibles.</p>
          </div>
        </div>
        <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-4">
          {popularRoutes.map(route => (
            <button
              key={`${route.origin}-${route.destination}`}
              type="button"
              onClick={() => goToResults(route.origin, route.destination, limaToday())}
              className="group relative flex aspect-[4/3] items-end overflow-hidden rounded-card bg-primary-700 bg-cover bg-center p-4 text-left text-white shadow-sm transition hover:-translate-y-0.5 hover:shadow-md"
              style={{ backgroundImage: `linear-gradient(0deg, rgba(4,20,18,.88), transparent 72%), url(${route.image})` }}
            >
              <span>
                <span className="block font-display text-lg font-bold">{route.origin} <span aria-hidden="true">→</span> {route.destination}</span>
                <span className="mt-1 block text-sm text-white/85">{route.detail}</span>
              </span>
            </button>
          ))}
        </div>
      </section>
    </div>
  );
}
