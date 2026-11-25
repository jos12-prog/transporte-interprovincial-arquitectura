import { useEffect, useState } from 'react';
import { Building2, LayoutDashboard, LogOut, Settings, Users } from 'lucide-react';
import { Link, Outlet, useNavigate } from 'react-router-dom';
import logo from '@/assets/brand/logo-horizontal.svg';
import { getStaffAgencies } from '@/api/identity';
import { useAuth } from '@/auth/AuthContext';

interface StaffShellProps {
  role: string;
}

export function StaffShell({ role }: StaffShellProps) {
  const { session, signOut } = useAuth();
  const [agencies, setAgencies] = useState<Array<{ agencyId: string; agencyName: string; city: string }>>([]);
  const [agencyId, setAgencyId] = useState('');
  const [agencyError, setAgencyError] = useState('');
  const navigate = useNavigate();

  useEffect(() => {
    if (session?.user.role !== 'agency_admin') return;
    getStaffAgencies(session.accessToken).then(setAgencies).catch((error: unknown) => setAgencyError(error instanceof Error ? error.message : 'No se pudieron cargar las agencias.'));
  }, [session]);

  function logout() {
    signOut();
    navigate('/personal/login', { replace: true });
  }

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
          {session?.user.role === 'super_admin' ? <Link className="flex items-center gap-3 rounded-button px-3 py-3 text-sm text-white/80 hover:bg-white/10" to="/superadmin">
            <Building2 aria-hidden="true" size={20} /> Catálogo de plataforma
          </Link> : <Link className="flex items-center gap-3 rounded-button px-3 py-3 text-sm text-white/80 hover:bg-white/10" to="/superadmin">
            <Users aria-hidden="true" size={20} /> Usuarios
          </Link>}
          <Link className="flex items-center gap-3 rounded-button px-3 py-3 text-sm text-white/80 hover:bg-white/10" to="/admin-agencia/configuracion">
            <Settings aria-hidden="true" size={20} /> Configuración
          </Link>
        </nav>
        <div className="hidden border-t border-white/10 px-6 py-4 text-sm text-white/70 md:block">{role}</div>
      </aside>
      <div className="min-w-0 flex-1">
        <header className="flex h-16 items-center justify-between border-b border-neutral-200 bg-white px-6">
          <span className="text-sm font-medium text-neutral-600">Espacio de trabajo</span>
          <div className="flex items-center gap-3">
            {session?.user.role === 'agency_admin' ? <label className="flex items-center gap-2 text-xs text-neutral-600">Agencia<select aria-label="Agencia activa" className="min-h-9 rounded-button border border-neutral-200 bg-white px-3 text-sm text-neutral-900" value={agencyId} onChange={event => setAgencyId(event.target.value)}><option value="">Seleccionar</option>{agencies.map(agency => <option key={agency.agencyId} value={agency.agencyId}>{agency.agencyName}</option>)}</select></label> : null}
            <span className="rounded-pill bg-primary-100 px-3 py-1 text-xs font-semibold text-primary-700">{role}</span>
            <button onClick={logout} type="button" className="inline-flex min-h-9 items-center gap-2 rounded-button px-3 text-sm text-neutral-700 hover:bg-neutral-50"><LogOut aria-hidden="true" size={16} /><span>Cerrar sesión</span></button>
          </div>
        </header>
        <main className="mx-auto max-w-container px-page-gutter py-8">
          {agencyError ? <p role="alert" className="mb-4 text-sm text-danger-700">{agencyError}</p> : null}
          <Outlet />
        </main>
      </div>
    </div>
  );
}
