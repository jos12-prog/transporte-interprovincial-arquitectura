import { fireEvent, render, screen } from '@testing-library/react';
import { MemoryRouter } from 'react-router-dom';
import { AppRoutes } from '@/App';
import { AuthProvider } from '@/auth/AuthContext';
import { getStaffAgencies } from '@/api/identity';

jest.mock('@/api/identity', () => ({
  ...jest.requireActual('@/api/identity'),
  getStaffAgencies: jest.fn().mockResolvedValue([]),
}));

const session = (role: 'passenger' | 'agent' | 'agency_admin' | 'super_admin') => ({
  accessToken: 'test-token', expiresAtUtc: '2026-11-25T13:00:00Z', user: { id: 'user-1', role },
});

function renderRoute(path: string, role?: 'passenger' | 'agent' | 'agency_admin' | 'super_admin') {
  return render(
    <MemoryRouter future={{ v7_startTransition: true, v7_relativeSplatPath: true }} initialEntries={[path]}>
      <AuthProvider initialSession={role ? session(role) : null}><AppRoutes /></AuthProvider>
    </MemoryRouter>,
  );
}

describe('role routes and layouts', () => {
  it.each([
    ['/agente', 'agent', 'Agente/Vendedor'],
    ['/admin-agencia', 'agency_admin', 'Administrador de Agencia'],
    ['/superadmin', 'super_admin', 'Superadministrador'],
  ] as const)('renders the protected route for %s only for its technical role', async (path, role, title) => {
    if (role === 'agency_admin') {
      jest.mocked(getStaffAgencies).mockResolvedValue([{ agencyId: 'agency-1', agencyName: 'Andes Express - Ayacucho', city: 'Ayacucho' }]);
    }
    renderRoute(path, role);
    if (role === 'agency_admin') expect(await screen.findByRole('option', { name: 'Andes Express - Ayacucho' })).toBeInTheDocument();
    expect(screen.getByRole('heading', { level: 1, name: title })).toBeInTheDocument();
    expect(screen.getByRole('button', { name: /cerrar sesión/i })).toBeInTheDocument();
  });

  it('redirects a passenger to the permitted passenger area', () => {
    renderRoute('/agente', 'passenger');
    expect(screen.getByRole('heading', { level: 1, name: '¿A dónde viajamos esta vez?' })).toBeInTheDocument();
    expect(screen.queryByRole('navigation', { name: 'Navegación del personal' })).not.toBeInTheDocument();
  });

  it('redirects a staff role to its own home instead of another staff role route', () => {
    renderRoute('/admin-agencia', 'agent');
    expect(screen.getByRole('heading', { level: 1, name: 'Agente/Vendedor' })).toBeInTheDocument();
  });

  it('sends anonymous staff users to staff login', () => {
    renderRoute('/agente');
    expect(screen.getByRole('heading', { level: 2, name: 'Bienvenido al portal' })).toBeInTheDocument();
  });

  it('renders trip search as the public passenger landing screen', () => {
    renderRoute('/pasajero');
    expect(screen.getByRole('heading', { level: 1, name: '¿A dónde viajamos esta vez?' })).toBeInTheDocument();
    expect(screen.getByRole('form', { name: 'Buscar viajes' })).toBeInTheDocument();
  });

  it('renders passenger navigation and the account menu trigger', () => {
    renderRoute('/pasajero');
    expect(screen.getByRole('navigation', { name: 'Navegación de pasajero' })).toBeInTheDocument();
    expect(screen.getByRole('button', { name: /mi cuenta/i })).toHaveAttribute('id', 'menu-usuario');
  });

  it('opens the passenger account menu and closes the in-memory session', async () => {
    renderRoute('/pasajero', 'passenger');
    fireEvent.click(screen.getByRole('button', { name: 'Pasajero' }));
    expect(await screen.findByRole('menuitem', { name: 'Cerrar sesión' })).toBeInTheDocument();
    fireEvent.click(screen.getByRole('menuitem', { name: 'Cerrar sesión' }));
    expect(await screen.findByRole('heading', { level: 2, name: 'Inicia sesión' })).toBeInTheDocument();
  });

  it('closes the staff session and returns to staff login', async () => {
    renderRoute('/agente', 'agent');
    fireEvent.click(screen.getByRole('button', { name: /cerrar sesión/i }));
    expect(await screen.findByRole('heading', { level: 2, name: 'Bienvenido al portal' })).toBeInTheDocument();
  });
});
