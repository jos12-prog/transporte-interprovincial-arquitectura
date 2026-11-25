import { fireEvent, render, screen, waitFor } from '@testing-library/react';
import { MemoryRouter, Route, Routes } from 'react-router-dom';
import { AuthProvider } from '@/auth/AuthContext';
import { LoginPage } from '@/pages/auth/LoginPage';
import { RegisterPage } from '@/pages/auth/RegisterPage';
import * as identityApi from '@/api/identity';

jest.mock('@/api/identity', () => ({
  ...jest.requireActual('@/api/identity'),
  login: jest.fn(),
  registerPassenger: jest.fn(),
}));

function renderPage(page: React.ReactNode) {
  return render(<MemoryRouter future={{ v7_startTransition: true, v7_relativeSplatPath: true }}><AuthProvider><Routes><Route path="/" element={page} /><Route path="/pasajero" element={<h1>Inicio pasajero</h1>} /><Route path="/pasajero/login" element={<h1>Login pasajero</h1>} /></Routes></AuthProvider></MemoryRouter>);
}

describe('authentication forms', () => {
  beforeEach(() => jest.clearAllMocks());

  it('submits passenger credentials and redirects by technical role', async () => {
    jest.mocked(identityApi.login).mockResolvedValue({
      accessToken: 'token', expiresAtUtc: '2026-11-25T13:00:00Z', user: { id: 'u1', role: 'passenger' },
    });
    renderPage(<LoginPage />);
    fireEvent.change(screen.getByLabelText('Correo o DNI'), { target: { value: '12345678' } });
    fireEvent.change(screen.getByLabelText('Contraseña'), { target: { value: 'password123' } });
    fireEvent.click(screen.getByRole('button', { name: 'Iniciar sesión' }));
    await waitFor(() => expect(screen.getByRole('heading', { name: 'Inicio pasajero' })).toBeInTheDocument());
    expect(identityApi.login).toHaveBeenCalledWith('12345678', 'password123');
  });

  it('shows API errors accessibly', async () => {
    jest.mocked(identityApi.login).mockRejectedValue(new Error('Credenciales incorrectas.'));
    renderPage(<LoginPage staff />);
    fireEvent.change(screen.getByLabelText('Correo corporativo'), { target: { value: 'agent@example.test' } });
    fireEvent.change(screen.getByLabelText('Contraseña'), { target: { value: 'password123' } });
    fireEvent.click(screen.getByRole('button', { name: 'Entrar al portal' }));
    expect(await screen.findByRole('alert')).toHaveTextContent('Credenciales incorrectas.');
  });

  it('registers the supplied passenger fields and redirects to login', async () => {
    jest.mocked(identityApi.registerPassenger).mockResolvedValue({ userId: 'user-1' });
    renderPage(<RegisterPage />);
    fireEvent.change(screen.getByLabelText('Nombre completo'), { target: { value: 'Ana Pérez' } });
    fireEvent.change(screen.getByLabelText('DNI'), { target: { value: '12345678' } });
    fireEvent.change(screen.getByLabelText('Celular'), { target: { value: '999111222' } });
    fireEvent.change(screen.getByLabelText('Correo electrónico'), { target: { value: 'ana@example.test' } });
    fireEvent.change(screen.getByLabelText('Contraseña'), { target: { value: 'password123' } });
    fireEvent.click(screen.getByRole('button', { name: 'Crear cuenta' }));
    await waitFor(() => expect(screen.getByRole('heading', { name: 'Login pasajero' })).toBeInTheDocument());
    expect(identityApi.registerPassenger).toHaveBeenCalledWith({
      fullName: 'Ana Pérez', dni: '12345678', phoneNumber: '999111222', email: 'ana@example.test', password: 'password123',
    });
  });
});
