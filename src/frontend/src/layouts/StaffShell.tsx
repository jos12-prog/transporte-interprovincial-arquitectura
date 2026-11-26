import { useEffect, useRef, useState, type KeyboardEvent, type ReactNode } from 'react';
import {
  Building2,
  BusFront,
  CalendarDays,
  ChevronDown,
  CircleDollarSign,
  LayoutDashboard,
  LogOut,
  Route as RouteIcon,
  Store,
} from 'lucide-react';
import { Link, NavLink, Outlet, useLocation, useNavigate } from 'react-router-dom';
import logoMark from '@/assets/brand/logo-mark.svg';
import { getStaffAgencies, roleLabel } from '@/api/identity';
import { useAuth } from '@/auth/AuthContext';

interface StaffShellProps {
  role: string;
}

interface SideLinkProps {
  to: string;
  label: string;
  icon: ReactNode;
  end?: boolean;
  activeSection?: 'companies' | 'agencies' | 'models';
}

interface StaffAgency { agencyId: string; agencyName: string; companyName: string; city: string }

const activeClass = 'bg-white/[.08] text-white before:bg-accent-500 hover:bg-white/[.08] hover:text-white';
const inactiveClass = 'text-white/70 before:bg-transparent hover:bg-white/[.06] hover:text-white';

function SideLink({ to, label, icon, end = false, activeSection }: SideLinkProps) {
  const location = useLocation();
  const currentSection = new URLSearchParams(location.search).get('section');
  const sectionIsActive = activeSection !== undefined
    && (currentSection === activeSection
      || (activeSection === 'companies' && currentSection !== 'agencies' && currentSection !== 'models'));
  return (
    <NavLink
      aria-label={label}
      aria-current={activeSection ? (sectionIsActive ? 'page' : 'false') : undefined}
      className={({ isActive }) => `group relative flex min-h-10 items-center gap-3 rounded-[10px] px-3 py-2.5 text-sm font-medium transition-colors duration-150 ease-out before:absolute before:-left-3 before:top-2 before:bottom-2 before:w-[3px] before:rounded-r-[3px] before:content-[''] ${isActive && (activeSection === undefined || sectionIsActive) ? activeClass : inactiveClass}`}
      end={end}
      title={label}
      to={to}
    >
      {icon}
      <span className="side-link-label">{label}</span>
    </NavLink>
  );
}

function initials(fullName: string | undefined) {
  const words = fullName?.trim().split(/\s+/).filter(Boolean) ?? [];
  const value = words.length > 1 ? words.slice(0, 2).map(word => word[0]).join('') : (words[0] ?? '').slice(0, 2);
  return value.toLocaleUpperCase('es-PE');
}

function agencyLabel(agency: StaffAgency) {
  return agency.agencyName.toLocaleLowerCase('es-PE').includes(agency.city.toLocaleLowerCase('es-PE'))
    ? agency.agencyName
    : `${agency.agencyName} – ${agency.city}`;
}

export function StaffShell({ role }: StaffShellProps) {
  const { session, signOut } = useAuth();
  const [agencies, setAgencies] = useState<StaffAgency[]>([]);
  const [agencyId, setAgencyId] = useState('');
  const [agencyListOpen, setAgencyListOpen] = useState(false);
  const [highlightedAgencyIndex, setHighlightedAgencyIndex] = useState(0);
  const [agencyStatus, setAgencyStatus] = useState<'idle' | 'loading' | 'ready' | 'error'>('idle');
  const [agencyError, setAgencyError] = useState('');
  const navigate = useNavigate();
  const agencyButtonRef = useRef<HTMLButtonElement>(null);
  const agencyListRef = useRef<HTMLUListElement>(null);
  const isAgencyAdmin = session?.user.role === 'agency_admin';
  const selectedAgency = agencies.find(agency => agency.agencyId === agencyId);
  const userLabel = session?.user.fullName?.trim() || session?.user.email?.trim() || '';

  useEffect(() => {
    if (agencyListOpen) agencyListRef.current?.focus();
  }, [agencyListOpen]);

  useEffect(() => {
    if (!isAgencyAdmin || !session) {
      setAgencies([]);
      setAgencyId('');
      setAgencyStatus('idle');
      setAgencyError('');
      return;
    }

    let cancelled = false;
    setAgencyStatus('loading');
    setAgencyError('');
    getStaffAgencies(session.accessToken)
      .then(rows => {
        if (cancelled) return;
        setAgencies(rows);
        setAgencyStatus('ready');
        if (rows.length === 0) {
          setAgencyId('');
          return;
        }

        let storedAgencyId = '';
        try {
          storedAgencyId = window.sessionStorage.getItem('ra.activeAgencyId') ?? '';
        } catch {
          // Storage can be unavailable in privacy-restricted browser contexts.
        }
        const selected = rows.some(row => row.agencyId === storedAgencyId) ? storedAgencyId : rows[0].agencyId;
        setAgencyId(selected);
        try {
          window.sessionStorage.setItem('ra.activeAgencyId', selected);
        } catch {
          // Agency selection remains usable for this page even when it cannot persist.
        }
      })
      .catch((error: unknown) => {
        if (cancelled) return;
        setAgencyStatus('error');
        setAgencyError(error instanceof Error ? error.message : 'No se pudieron cargar las agencias.');
      });

    return () => { cancelled = true; };
  }, [isAgencyAdmin, session?.accessToken, session?.user.role]);

  function selectAgency(nextAgencyId: string) {
    setAgencyId(nextAgencyId);
    setAgencyListOpen(false);
    try {
      window.sessionStorage.setItem('ra.activeAgencyId', nextAgencyId);
    } catch {
      // The current selection still applies even if session storage is unavailable.
    }
  }

  function handleAgencyButtonKeyDown(event: KeyboardEvent<HTMLButtonElement>) {
    if (!agencies.length) return;
    if (event.key === 'ArrowDown' || event.key === 'Enter' || event.key === ' ') {
      event.preventDefault();
      setHighlightedAgencyIndex(Math.max(0, agencies.findIndex(agency => agency.agencyId === agencyId)));
      setAgencyListOpen(true);
    }
  }

  function handleAgencyListKeyDown(event: KeyboardEvent<HTMLUListElement>) {
    if (event.key === 'Escape') {
      event.preventDefault(); setAgencyListOpen(false); agencyButtonRef.current?.focus();
    } else if (event.key === 'ArrowDown') {
      event.preventDefault(); setHighlightedAgencyIndex(index => (index + 1) % agencies.length);
    } else if (event.key === 'ArrowUp') {
      event.preventDefault(); setHighlightedAgencyIndex(index => (index - 1 + agencies.length) % agencies.length);
    } else if (event.key === 'Home') {
      event.preventDefault(); setHighlightedAgencyIndex(0);
    } else if (event.key === 'End') {
      event.preventDefault(); setHighlightedAgencyIndex(agencies.length - 1);
    } else if (event.key === 'Enter' && agencies[highlightedAgencyIndex]) {
      event.preventDefault(); selectAgency(agencies[highlightedAgencyIndex].agencyId); agencyButtonRef.current?.focus();
    }
  }

  function logout() {
    signOut();
    try {
      window.sessionStorage.removeItem('ra.activeAgencyId');
    } catch {
      // Signing out and navigation must still work if storage is unavailable.
    }
    navigate('/personal/login', { replace: true });
  }

  const agencyLinks = [
    { to: '/admin-agencia', label: 'Dashboard', icon: <LayoutDashboard aria-hidden="true" size={20} />, end: true },
    { to: '/admin-agencia/viajes', label: 'Viajes y horarios', icon: <CalendarDays aria-hidden="true" size={20} /> },
    { to: '/admin-agencia/rutas', label: 'Rutas', icon: <RouteIcon aria-hidden="true" size={20} /> },
    { to: '/admin-agencia/vehiculos', label: 'Vehículos', icon: <BusFront aria-hidden="true" size={20} /> },
    { to: '/admin-agencia/tarifas', label: 'Tarifas', icon: <CircleDollarSign aria-hidden="true" size={20} /> },
  ];

  return (
    <div className="grid min-h-screen min-w-0 grid-cols-[248px_minmax(0,1fr)] max-[1024px]:grid-cols-[72px_minmax(0,1fr)]" data-role={role}>
      <aside aria-label="Menú" className="sticky top-0 flex h-screen h-[100dvh] w-[248px] min-w-0 shrink-0 flex-col overflow-x-hidden overflow-y-auto bg-primary-950 px-3 py-[18px] text-white/70 max-[1024px]:w-[72px]">
        <Link aria-label="RutaAndina, inicio del personal" className="mb-5 flex items-center gap-3 px-[10px] py-1 text-white max-[1024px]:justify-center max-[1024px]:px-0" to={session?.user.role === 'agency_admin' ? '/admin-agencia' : session?.user.role === 'super_admin' ? '/superadmin' : '/agente'}>
          <img alt="" className="h-8 w-8 shrink-0" src={logoMark} />
          <span className="side-link-label font-display text-lg font-bold">Ruta<span className="text-accent-500">Andina</span></span>
        </Link>

        <nav aria-label="Navegación del personal" className="flex flex-1 flex-col">
          {session?.user.role === 'agency_admin' ? <>
            <p className="px-3 pb-1.5 pt-4 text-[11px] font-semibold uppercase tracking-[.1em] text-white/40 max-[1024px]:hidden">AGENCIA</p>
            {agencyLinks.map(link => <SideLink key={link.to} {...link} />)}
          </> : null}

          {session?.user.role === 'agent' ? <>
            <p className="px-3 pb-1.5 pt-4 text-[11px] font-semibold uppercase tracking-[.1em] text-white/40 max-[1024px]:hidden">AGENCIA</p>
            <SideLink end icon={<LayoutDashboard aria-hidden="true" size={20} />} label="Panel" to="/agente" />
            <SideLink icon={<Building2 aria-hidden="true" size={20} />} label="Mi agencia" to="/agente/mi-agencia" />
          </> : null}

          {session?.user.role === 'super_admin' ? <>
            <p className="px-3 pb-1.5 pt-4 text-[11px] font-semibold uppercase tracking-[.1em] text-white/40 max-[1024px]:hidden">PLATAFORMA</p>
            <SideLink activeSection="companies" icon={<Building2 aria-hidden="true" size={20} />} label="Empresas" to="/superadmin?section=companies" />
            <SideLink activeSection="agencies" icon={<Store aria-hidden="true" size={20} />} label="Agencias" to="/superadmin?section=agencies" />
            <SideLink activeSection="models" icon={<BusFront aria-hidden="true" size={20} />} label="Modelos de vehículo" to="/superadmin?section=models" />
          </> : null}
        </nav>

        <div className="me mt-auto grid w-full max-w-full min-w-0 gap-1.5 overflow-hidden rounded-xl border border-white/[.06] bg-white/[.05] px-3 pb-1.5 pt-3 text-[13px] text-white max-[1024px]:justify-items-center max-[1024px]:px-1">
          <div className="flex min-w-0 items-center gap-2.5 border-b border-white/[.08] pb-2 max-[1024px]:border-0 max-[1024px]:pb-0">
            <span aria-hidden="true" className="grid h-9 w-9 shrink-0 place-items-center rounded-full bg-primary-600 text-xs font-semibold text-white">{initials(userLabel)}</span>
            <div className="min-w-0 flex-1 overflow-hidden max-[1024px]:hidden">
              <p className="block min-w-0 truncate font-medium" title={userLabel}>{userLabel}</p>
              <small className="block text-xs text-white/55">{roleLabel(session?.user.role ?? 'agent')}</small>
            </div>
          </div>
          <button aria-label="Cerrar sesión" className="-mx-1.5 flex w-full items-center gap-2 rounded-[10px] px-1.5 py-2 text-[13px] text-[#FF9C8C] transition-colors duration-150 hover:bg-[rgba(255,156,140,0.12)] hover:text-[#FFB8AC] max-[1024px]:mx-0 max-[1024px]:w-10 max-[1024px]:justify-center" onClick={logout} title="Cerrar sesión" type="button">
            <LogOut aria-hidden="true" size={16} />
            <span className="max-[1024px]:hidden">Cerrar sesión</span>
          </button>
        </div>
      </aside>

      <div className="min-w-0">
        <header className="sticky top-0 z-10 flex h-16 items-center gap-4 border-b border-neutral-200 bg-white px-7">
          {isAgencyAdmin ? <div className="relative">
            <button aria-controls="assigned-agencies-list" aria-expanded={agencyListOpen} aria-haspopup="listbox" aria-label="Agencia activa" className="inline-flex h-10 items-center gap-2.5 rounded-xl border border-neutral-200 bg-white px-3 text-left text-sm font-medium text-neutral-900 hover:border-neutral-300 disabled:cursor-wait" disabled={agencyStatus !== 'ready' || agencies.length === 0} onClick={() => { setHighlightedAgencyIndex(Math.max(0, agencies.findIndex(agency => agency.agencyId === agencyId))); setAgencyListOpen(open => !open); }} onKeyDown={handleAgencyButtonKeyDown} ref={agencyButtonRef} type="button">
              <span className="grid h-7 w-7 shrink-0 place-items-center rounded-lg bg-primary-100 text-primary-700"><Store aria-hidden="true" size={16} /></span>
              <span className="min-w-0"><span className="block max-w-60 truncate">{agencyStatus === 'loading' ? 'Cargando agencias…' : selectedAgency ? agencyLabel(selectedAgency) : 'Sin agencias asignadas'}</span><small className="block text-[11.5px] leading-[1.1] text-neutral-500">{selectedAgency?.companyName ?? ''}</small></span>
              <ChevronDown aria-hidden="true" className="shrink-0 text-neutral-500" size={16} />
            </button>
            {agencyListOpen ? <ul aria-activedescendant={`agency-option-${agencies[highlightedAgencyIndex]?.agencyId ?? ''}`} aria-label="Agencias asignadas" className="absolute left-0 top-[calc(100%+6px)] z-20 min-w-full rounded-xl border border-neutral-200 bg-white p-1 shadow-lg outline-none" id="assigned-agencies-list" onKeyDown={handleAgencyListKeyDown} ref={agencyListRef} role="listbox" tabIndex={0}>
              {agencies.map((agency, index) => <li aria-selected={agency.agencyId === agencyId} className={`cursor-pointer rounded-lg px-3 py-2 text-sm ${highlightedAgencyIndex === index ? 'bg-primary-50 text-primary-800' : 'text-neutral-900'}`} id={`agency-option-${agency.agencyId}`} key={agency.agencyId} onClick={() => { selectAgency(agency.agencyId); agencyButtonRef.current?.focus(); }} onMouseEnter={() => setHighlightedAgencyIndex(index)} role="option">
                <span className="block font-medium">{agencyLabel(agency)}</span><small className="block text-[11.5px] text-neutral-500">{agency.companyName}</small>
              </li>)}
            </ul> : null}
            {agencyStatus === 'error' ? <p className="absolute left-0 top-full mt-1 text-xs text-danger-600" role="alert">{agencyError}</p> : null}
          </div> : session?.user.role === 'super_admin' ? <button aria-label="Plataforma RutaAndina, Superadministración" className="inline-flex h-10 items-center gap-2.5 rounded-xl border border-neutral-200 bg-white px-3 text-left text-sm font-medium text-neutral-900" type="button">
            <span className="grid h-7 w-7 shrink-0 place-items-center rounded-lg bg-primary-100 text-primary-700"><Store aria-hidden="true" size={16} /></span>
            <span><span className="block">Plataforma RutaAndina</span><small className="block text-[11.5px] leading-[1.1] text-neutral-500">Superadministración</small></span>
          </button> : null}
        </header>
        <main className="mx-auto max-w-container px-page-gutter py-8">
          <Outlet context={{ agencyId, agencyName: selectedAgency?.agencyName, companyName: selectedAgency?.companyName }} />
        </main>
      </div>
    </div>
  );
}
