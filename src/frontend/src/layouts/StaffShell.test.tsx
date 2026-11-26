import { fireEvent, render, screen, waitFor } from '@testing-library/react';
import { MemoryRouter, Route, Routes, useOutletContext } from 'react-router-dom';
import { getStaffAgencies } from '@/api/identity';
import type { AuthSession } from '@/api/identity';
import { useAuth } from '@/auth/AuthContext';
import { StaffShell } from './StaffShell';

jest.mock('@/api/identity', () => ({
  ...jest.requireActual('@/api/identity'),
  getStaffAgencies: jest.fn(),
}));
jest.mock('@/auth/AuthContext', () => ({ useAuth: jest.fn() }));

const mockGetStaffAgencies = getStaffAgencies as jest.MockedFunction<typeof getStaffAgencies>;
const mockUseAuth = useAuth as jest.Mock;
const signOut = jest.fn();
const session: AuthSession = {
  accessToken: 'staff-token',
  expiresAtUtc: '2026-12-01T00:00:00Z',
  user: { id: 'staff-1', role: 'agency_admin', fullName: 'Rosa Huamán' },
};
const agencyRows = [
  { agencyId: 'agency-1', agencyName: 'Terminal Norte', companyName: 'Andes Express', city: 'Huamanga' },
  { agencyId: 'agency-2', agencyName: 'Terminal Sur – Ayacucho', companyName: 'Wari Bus', city: 'Ayacucho' },
];

function OutletAgency() {
  const context = useOutletContext<{ agencyId: string; companyName?: string; agencyName?: string }>();
  return <p data-testid="outlet-agency">{context.agencyId || 'ninguna'}|{context.companyName}|{context.agencyName}</p>;
}

function renderShell(initialEntry = '/admin-agencia/vehiculos', activeSession: AuthSession = session) {
  mockUseAuth.mockReturnValue({ session: activeSession, setSession: jest.fn(), signOut });
  return render(
    <MemoryRouter initialEntries={[initialEntry]} future={{ v7_startTransition: true, v7_relativeSplatPath: true }}>
      <Routes>
        <Route element={<StaffShell role="Administrador de Agencia" />} path="/admin-agencia">
          <Route element={<OutletAgency />} index />
          <Route element={<OutletAgency />} path="vehiculos" />
        </Route>
        <Route element={<h1>Login personal</h1>} path="/personal/login" />
      </Routes>
    </MemoryRouter>,
  );
}

function renderPlatformShell(initialEntry = '/superadmin') {
  mockUseAuth.mockReturnValue({
    session: { ...session, user: { ...session.user, role: 'super_admin' } },
    setSession: jest.fn(),
    signOut,
  });
  return render(
    <MemoryRouter initialEntries={[initialEntry]} future={{ v7_startTransition: true, v7_relativeSplatPath: true }}>
      <Routes>
        <Route element={<StaffShell role="Superadministrador" />} path="/superadmin">
          <Route element={<p>Contenido de plataforma</p>} index />
        </Route>
      </Routes>
    </MemoryRouter>,
  );
}

describe('StaffShell', () => {
  beforeEach(() => {
    jest.clearAllMocks();
    window.sessionStorage.clear();
    mockGetStaffAgencies.mockResolvedValue(agencyRows);
  });

  it('marks only the current module active and keeps Dashboard inactive on a child route', async () => {
    renderShell();

    expect(await screen.findByRole('link', { name: 'Vehículos' })).toHaveAttribute('aria-current', 'page');
    expect(screen.getByRole('link', { name: 'Rutas' })).not.toHaveAttribute('aria-current', 'page');
    expect(screen.getByRole('link', { name: 'Dashboard' })).not.toHaveAttribute('aria-current', 'page');
    expect(await screen.findByRole('button', { name: 'Agencia activa' })).toBeInTheDocument();
    expect(screen.getByRole('button', { name: 'Agencia activa' }).closest('header')).toBeInTheDocument();
    expect(screen.queryByText('Agencia activa', { selector: 'label' })).not.toBeInTheDocument();
  });

  it.each([
    ['/superadmin', 'Empresas'],
    ['/superadmin?section=agencies', 'Agencias'],
    ['/superadmin?section=models', 'Modelos de vehículo'],
  ])('marks only the selected platform section active at %s', (path, activeLabel) => {
    renderPlatformShell(path);

    expect(screen.getByRole('link', { name: activeLabel })).toHaveAttribute('aria-current', 'page');
    for (const label of ['Empresas', 'Agencias', 'Modelos de vehículo']) {
      if (label !== activeLabel) expect(screen.getByRole('link', { name: label })).not.toHaveAttribute('aria-current', 'page');
    }
  });

  it('shows the static platform label in the superadmin top bar without an agency selector', () => {
    renderPlatformShell();
    expect(screen.getByRole('button', { name: 'Plataforma RutaAndina, Superadministración' })).toHaveTextContent('Plataforma RutaAndina');
    expect(screen.getByRole('button', { name: 'Plataforma RutaAndina, Superadministración' })).toHaveTextContent('Superadministración');
    expect(screen.queryByRole('button', { name: 'Agencia activa' })).not.toBeInTheDocument();
    expect(screen.getByRole('button', { name: 'Plataforma RutaAndina, Superadministración' }).closest('header')).toBeInTheDocument();
  });

  it('selects the first assigned agency and passes it to the Outlet', async () => {
    renderShell();

    expect(await screen.findByTestId('outlet-agency')).toHaveTextContent('agency-1|Andes Express|Terminal Norte');
    expect(screen.getByRole('button', { name: 'Agencia activa' })).toHaveTextContent('Terminal Norte – Huamanga');
    expect(screen.getByRole('button', { name: 'Agencia activa' })).toHaveTextContent('Andes Express');
  });

  it('restores a saved agency when it remains assigned', async () => {
    window.sessionStorage.setItem('ra.activeAgencyId', 'agency-2');
    renderShell();

    expect(await screen.findByTestId('outlet-agency')).toHaveTextContent('agency-2|Wari Bus|Terminal Sur – Ayacucho');
    expect(screen.getByRole('button', { name: 'Agencia activa' })).toHaveTextContent('Terminal Sur – Ayacucho');
  });

  it('persists the agency selected by the user', async () => {
    renderShell();

    const selector = await screen.findByRole('button', { name: 'Agencia activa' });
    fireEvent.keyDown(selector, { key: 'ArrowDown' });
    const listbox = await screen.findByRole('listbox', { name: 'Agencias asignadas' });
    fireEvent.keyDown(listbox, { key: 'ArrowDown' });
    fireEvent.keyDown(listbox, { key: 'Enter' });

    await waitFor(() => expect(window.sessionStorage.getItem('ra.activeAgencyId')).toBe('agency-2'));
    expect(screen.getByTestId('outlet-agency')).toHaveTextContent('agency-2|Wari Bus|Terminal Sur – Ayacucho');
  });

  it('shows the empty state when the user has no assigned agencies', async () => {
    mockGetStaffAgencies.mockResolvedValue([]);
    renderShell();

    expect(await screen.findByRole('button', { name: 'Agencia activa' })).toHaveTextContent('Sin agencias asignadas');
    expect(screen.getByTestId('outlet-agency')).toHaveTextContent('ninguna');
  });

  it('shows agency loading errors as an alert', async () => {
    mockGetStaffAgencies.mockRejectedValue(new Error('No se pudo cargar la lista.'));
    renderShell();

    expect(await screen.findByRole('alert')).toHaveTextContent('No se pudo cargar la lista.');
  });

  it('uses the email as the user label and calculates initials when full name is absent', async () => {
    renderShell(undefined, { ...session, user: { ...session.user, fullName: undefined, email: 'rosa@example.com' } });
    await screen.findByRole('button', { name: 'Agencia activa' });
    expect(screen.getByText('rosa@example.com')).toBeInTheDocument();
    expect(screen.getByText('rosa@example.com')).toHaveAttribute('title', 'rosa@example.com');
    expect(screen.getByText('rosa@example.com').closest('aside')).toHaveClass('w-[248px]', 'shrink-0', 'overflow-x-hidden');
    expect(screen.getByText('RO')).toBeInTheDocument();
    expect(screen.queryByText('Cuenta de personal')).not.toBeInTheDocument();
  });

  it('shows the full name before email and never exposes the user id', async () => {
    const withBoth = { ...session, user: { ...session.user, fullName: 'Rosa Huamán', email: 'rosa@example.com' } };
    renderShell(undefined, withBoth);
    await screen.findByRole('button', { name: 'Agencia activa' });
    expect(screen.getByText('Rosa Huamán')).toHaveAttribute('title', 'Rosa Huamán');
    expect(screen.getByText('Rosa Huamán')).toHaveClass('min-w-0', 'truncate');
    expect(screen.queryByText('rosa@example.com')).not.toBeInTheDocument();
    expect(screen.queryByText('staff-1')).not.toBeInTheDocument();
  });

  it('constrains every flex/grid ancestor around the long sidebar user name', async () => {
    const longNameSession = { ...session, user: { ...session.user, fullName: 'Administradora Andes Express' } };
    renderShell(undefined, longNameSession);
    const name = await screen.findByText('Administradora Andes Express');
    const text = name.parentElement!;
    const userRow = text.parentElement!;
    const profile = userRow.parentElement!;
    const aside = screen.getByLabelText('Menú');
    const grid = aside.parentElement!;

    expect(name).toHaveClass('block', 'truncate');
    expect(text).toHaveClass('flex-1', 'min-w-0', 'overflow-hidden');
    expect(userRow).toHaveClass('min-w-0');
    expect(profile).toHaveClass('me', 'grid', 'w-full', 'max-w-full', 'min-w-0');
    expect(aside).toHaveClass('flex', 'min-w-0', 'shrink-0');
    expect(grid).toHaveClass('grid', 'min-w-0');
  });

  it('clears the selected agency and navigates to staff login on sign out', async () => {
    window.sessionStorage.setItem('ra.activeAgencyId', 'agency-2');
    renderShell();

    fireEvent.click(screen.getByRole('button', { name: 'Cerrar sesión' }));

    expect(await screen.findByRole('heading', { name: 'Login personal' })).toBeInTheDocument();
    expect(window.sessionStorage.getItem('ra.activeAgencyId')).toBeNull();
    expect(signOut).toHaveBeenCalledTimes(1);
  });
});
