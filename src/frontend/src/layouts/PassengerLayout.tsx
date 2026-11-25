import { useEffect, useRef, useState } from 'react';
import { BusFront, CircleHelp, Menu, Search, Ticket, UserRound } from 'lucide-react';
import { Link, Outlet, useNavigate } from 'react-router-dom';
import logo from '@/assets/brand/logo-horizontal.svg';
import { useAuth } from '@/auth/AuthContext';
import { roleLabel } from '@/api/identity';

export function PassengerLayout() {
  const { session, signOut } = useAuth();
  const [menuOpen, setMenuOpen] = useState(false);
  const menuRef = useRef<HTMLDivElement>(null);
  const navigate = useNavigate();

  useEffect(() => {
    if (!menuOpen) return;
    const onPointerDown = (event: PointerEvent) => {
      if (!menuRef.current?.contains(event.target as Node)) setMenuOpen(false);
    };
    const onKeyDown = (event: KeyboardEvent) => { if (event.key === 'Escape') setMenuOpen(false); };
    document.addEventListener('pointerdown', onPointerDown);
    document.addEventListener('keydown', onKeyDown);
    return () => { document.removeEventListener('pointerdown', onPointerDown); document.removeEventListener('keydown', onKeyDown); };
  }, [menuOpen]);

  function logout() {
    signOut();
    setMenuOpen(false);
    navigate('/pasajero/login');
  }

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
          <div className="relative" ref={menuRef}>
            <button id="menu-usuario" className="flex min-h-10 items-center gap-2 rounded-pill border border-neutral-200 px-3 text-sm text-neutral-700 transition-colors duration-150 hover:bg-neutral-50 focus-visible:outline focus-visible:outline-2 focus-visible:outline-primary-700" type="button" aria-expanded={menuOpen} aria-haspopup="menu" onClick={() => setMenuOpen(open => !open)}>
              <UserRound aria-hidden="true" size={18} /><span className="hidden sm:inline">{session ? roleLabel(session.user.role) : 'Mi cuenta'}</span>
            </button>
            {menuOpen ? <div role="menu" aria-labelledby="menu-usuario" className="absolute right-0 top-[calc(100%+8px)] z-30 min-w-52 origin-top-right animate-menu-in rounded-card border border-neutral-200 bg-white p-2 shadow-layered">
              {session ? <><p className="px-3 py-2 text-xs text-neutral-600">{roleLabel(session.user.role)}</p><button role="menuitem" className="w-full rounded-button px-3 py-2 text-left text-sm text-neutral-900 hover:bg-neutral-50" onClick={logout}>Cerrar sesión</button></> : <><Link role="menuitem" className="block rounded-button px-3 py-2 text-sm text-neutral-900 hover:bg-neutral-50" to="/pasajero/login" onClick={() => setMenuOpen(false)}>Iniciar sesión</Link><Link role="menuitem" className="block rounded-button px-3 py-2 text-sm text-neutral-900 hover:bg-neutral-50" to="/pasajero/registro" onClick={() => setMenuOpen(false)}>Crear cuenta</Link></>}
            </div> : null}
          </div>
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
        <button type="button" onClick={() => setMenuOpen(open => !open)} className="flex flex-col items-center gap-1 py-1 text-xs text-neutral-600" aria-controls="menu-usuario" aria-expanded={menuOpen}><Menu aria-hidden="true" size={19} />Cuenta</button>
      </nav>
    </div>
  );
}
