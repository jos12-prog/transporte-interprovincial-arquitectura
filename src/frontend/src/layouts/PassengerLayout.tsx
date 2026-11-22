import { BusFront, CircleHelp, Menu, Search, Ticket } from 'lucide-react';
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
      <main className="min-h-[calc(100vh-72px)] pb-20 sm:pb-0">
        <Outlet />
      </main>
      <nav aria-label="Navegación inferior pasajero" className="fixed inset-x-0 bottom-0 z-20 grid grid-cols-4 border-t border-neutral-200 bg-white px-2 py-2 shadow-md sm:hidden">
        <Link to="/pasajero" className="flex flex-col items-center gap-1 py-1 text-xs font-medium text-primary-700"><Search aria-hidden="true" size={19} />Buscar</Link>
        <Link to="/pasajero/pasajes" className="flex flex-col items-center gap-1 py-1 text-xs text-neutral-600"><Ticket aria-hidden="true" size={19} />Mis pasajes</Link>
        <Link to="#ayuda" className="flex flex-col items-center gap-1 py-1 text-xs text-neutral-600"><CircleHelp aria-hidden="true" size={19} />Ayuda</Link>
        <Link to="#cuenta" className="flex flex-col items-center gap-1 py-1 text-xs text-neutral-600"><Menu aria-hidden="true" size={19} />Cuenta</Link>
      </nav>
    </div>
  );
}
