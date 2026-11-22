import { render, screen } from '@testing-library/react';
import { MemoryRouter } from 'react-router-dom';
import { AppRoutes } from '@/App';

describe('role routes and layouts', () => {
  it.each([
    ['/pasajero', 'Pasajero'],
    ['/agente', 'Agente'],
    ['/admin-agencia', 'Administrador de agencia'],
    ['/superadmin', 'Superadministrador'],
  ])('renders the empty route for %s', (path, title) => {
    render(
      <MemoryRouter future={{ v7_startTransition: true, v7_relativeSplatPath: true }} initialEntries={[path]}>
        <AppRoutes />
      </MemoryRouter>,
    );

    expect(screen.getByRole('heading', { level: 1, name: title })).toBeInTheDocument();
  });

  it('renders the passenger navigation layout and staff brand shell', () => {
    const { unmount } = render(
      <MemoryRouter future={{ v7_startTransition: true, v7_relativeSplatPath: true }} initialEntries={['/pasajero']}>
        <AppRoutes />
      </MemoryRouter>,
    );
    expect(screen.getByRole('navigation', { name: 'Navegación de pasajero' })).toBeInTheDocument();
    unmount();

    render(
      <MemoryRouter future={{ v7_startTransition: true, v7_relativeSplatPath: true }} initialEntries={['/agente']}>
        <AppRoutes />
      </MemoryRouter>,
    );
    expect(screen.getByRole('navigation', { name: 'Navegación del personal' })).toBeInTheDocument();
    expect(screen.getAllByRole('img', { name: 'RutaAndina' })).toHaveLength(1);
  });
});
