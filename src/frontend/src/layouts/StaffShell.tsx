import { Building2, LayoutDashboard, Settings, Users } from 'lucide-react';
import { Link, Outlet } from 'react-router-dom';
import logo from '@/assets/brand/logo-horizontal.svg';

interface StaffShellProps {
  role: string;
}

export function StaffShell({ role }: StaffShellProps) {
  return (
    <div className="min-h-screen md:flex">
      <aside className="flex w-full flex-col bg-primary-900 text-white md:min-h-screen md:w-64">
        <div className="border-b border-white/10 px-6 py-5">
          <Link to="/agente" aria-label="RutaAndina, inicio del personal">
            <img className="h-9 w-auto rounded bg-white px-2 py-1" src={logo} alt="RutaAndina" />
          </Link>
          <p className="mt-3 text-xs font-semibold uppercase tracking-wider text-white/70">Portal de agencias</p>
        </div>
        <nav aria-label="Navegación del personal" className="flex flex-wrap gap-2 p-4 md:flex-1 md:flex-col">
          <Link className="flex items-center gap-3 rounded-button px-3 py-3 text-sm text-white/80 hover:bg-white/10" to="/agente">
            <LayoutDashboard aria-hidden="true" size={20} /> Panel
          </Link>
          <Link className="flex items-center gap-3 rounded-button px-3 py-3 text-sm text-white/80 hover:bg-white/10" to="/admin-agencia">
            <Building2 aria-hidden="true" size={20} /> Agencias
          </Link>
          <Link className="flex items-center gap-3 rounded-button px-3 py-3 text-sm text-white/80 hover:bg-white/10" to="/superadmin">
            <Users aria-hidden="true" size={20} /> Usuarios
          </Link>
          <Link className="flex items-center gap-3 rounded-button px-3 py-3 text-sm text-white/80 hover:bg-white/10" to="/admin-agencia/configuracion">
            <Settings aria-hidden="true" size={20} /> Configuración
          </Link>
        </nav>
        <div className="hidden border-t border-white/10 px-6 py-4 text-sm text-white/70 md:block">{role}</div>
      </aside>
      <div className="min-w-0 flex-1">
        <header className="flex h-16 items-center justify-between border-b border-neutral-200 bg-white px-6">
          <span className="text-sm font-medium text-neutral-600">Espacio de trabajo</span>
          <span className="rounded-pill bg-primary-100 px-3 py-1 text-xs font-semibold text-primary-700">{role}</span>
        </header>
        <main className="mx-auto max-w-container px-page-gutter py-8">
          <Outlet />
        </main>
      </div>
    </div>
  );
}
