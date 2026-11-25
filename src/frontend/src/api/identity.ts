export type TechnicalRole = 'passenger' | 'agent' | 'agency_admin' | 'super_admin';

export interface AuthUser {
  id: string;
  role: TechnicalRole;
}

export interface AuthSession {
  accessToken: string;
  expiresAtUtc: string;
  user: AuthUser;
}

const baseUrl = '';

async function request<T>(path: string, body: unknown): Promise<T> {
  const response = await fetch(`${baseUrl}/api/v1${path}`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(body),
  });
  const payload = await response.json().catch(() => ({}));
  if (!response.ok) throw new Error(payload.detail ?? payload.title ?? 'No se pudo completar la solicitud.');
  return payload as T;
}

export function login(identifier: string, password: string): Promise<AuthSession> {
  return request('/auth/login', { identifier, password });
}

export function registerPassenger(input: {
  fullName: string;
  dni: string;
  email: string;
  phoneNumber: string;
  password: string;
}): Promise<{ userId: string }> {
  return request('/auth/register', input);
}

export async function getStaffAgencies(accessToken: string): Promise<Array<{ agencyId: string; agencyName: string; city: string }>> {
  const response = await fetch(`${baseUrl}/api/v1/staff/agencies`, { headers: { Authorization: `Bearer ${accessToken}` } });
  if (!response.ok) throw new Error('No se pudieron cargar las agencias.');
  return response.json();
}

export function roleLabel(role: TechnicalRole): string {
  return {
    passenger: 'Pasajero',
    agent: 'Agente/Vendedor',
    agency_admin: 'Administrador de Agencia',
    super_admin: 'Superadministrador',
  }[role];
}

export function roleHome(role: TechnicalRole): string {
  return {
    passenger: '/pasajero',
    agent: '/agente',
    agency_admin: '/admin-agencia',
    super_admin: '/superadmin',
  }[role];
}
