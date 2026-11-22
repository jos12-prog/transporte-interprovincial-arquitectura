import { BusFront, Menu, Ticket } from 'lucide-react';
import { Link, Outlet } from 'react-router-dom';
import logo from '@/assets/brand/logo-horizontal.svg';

export function PassengerLayout() {
  return (
    <div className="min-h-screen bg-neutral-50">
      <header className="border-b border-neutral-200 bg-white">
        <div className="mx-auto flex max-w-container items-center justify-between px-page-gutter py-4">
          <Link to="/pasajero" aria-label="RutaAndina, inicio de pasajero">
            <img className="h-10 w-auto" src={logo} alt="RutaAndina" />
          </Link>
          <nav aria-label="Navegación de pasajero" className="hidden items-center gap-6 sm:flex">
            <Link className="flex items-center gap-2 text-sm text-neutral-600" to="/pasajero">
              <BusFront aria-hidden="true" size={18} /> Viajes
            </Link>
            <Link className="flex items-center gap-2 text-sm text-neutral-600" to="/pasajero/pasajes">
              <Ticket aria-hidden="true" size={18} /> Mis pasajes
            </Link>
          </nav>
          <button className="rounded-button p-2 text-neutral-600 sm:hidden" type="button" aria-label="Abrir menú">
            <Menu aria-hidden="true" size={20} />
          </button>
        </div>
      </header>
      <main className="mx-auto max-w-container px-page-gutter py-10">
        <Outlet />
      </main>
    </div>
  );
}
