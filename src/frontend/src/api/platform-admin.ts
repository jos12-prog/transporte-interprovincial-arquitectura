export interface CompanyAdmin {
  companyId: string;
  name: string;
  taxId: string | null;
  contactName: string | null;
  contactPhone: string | null;
  isActive: boolean;
}

export interface AgencyAdmin {
  agencyId: string;
  companyId: string;
  name: string;
  city: string;
  address: string | null;
  phone: string | null;
  isActive: boolean;
}

export interface SeatPositionAdmin {
  number: number;
  floor: number;
  row: number | null;
  column: number | null;
}

export interface VehicleModelAdmin {
  vehicleModelId: string;
  name: string;
  category: string | null;
  capacity: number;
  seats: SeatPositionAdmin[];
  isActive: boolean;
  isInUse: boolean;
}

export interface SaveCompanyInput {
  name: string;
  taxId: string | null;
  contactName: string | null;
  contactPhone: string | null;
}

export interface SaveAgencyInput {
  name: string;
  city: string;
  address: string | null;
  phone: string | null;
}

export interface SaveVehicleModelInput {
  name: string;
  category: string | null;
  seats: SeatPositionAdmin[];
}

async function request<T>(path: string, token: string, method = 'GET', body?: unknown): Promise<T> {
  const response = await fetch(path, {
    method,
    headers: {
      Accept: 'application/json',
      Authorization: `Bearer ${token}`,
      ...(body ? { 'Content-Type': 'application/json' } : {}),
    },
    ...(body ? { body: JSON.stringify(body) } : {}),
  });
  const payload = await response.json().catch(() => ({})) as { title?: string; detail?: string };
  if (!response.ok) throw new Error(payload.detail ?? payload.title ?? 'No se pudo completar la operación.');
  return payload as T;
}

export const getPlatformCompanies = (token: string) => request<CompanyAdmin[]>('/api/v1/admin/companies', token);
export const savePlatformCompany = (token: string, input: SaveCompanyInput, id?: string) =>
  request<CompanyAdmin>(id ? `/api/v1/admin/companies/${id}` : '/api/v1/admin/companies', token, id ? 'PUT' : 'POST', input);
export const setCompanyActive = (token: string, id: string, isActive: boolean) =>
  request<CompanyAdmin>(`/api/v1/admin/companies/${id}/status`, token, 'PATCH', { isActive });

export const getPlatformAgencies = (token: string, companyId: string) =>
  request<AgencyAdmin[]>(`/api/v1/admin/companies/${companyId}/agencies`, token);
export const savePlatformAgency = (token: string, companyId: string, input: SaveAgencyInput, id?: string) =>
  request<AgencyAdmin>(id ? `/api/v1/admin/agencies/${id}` : `/api/v1/admin/companies/${companyId}/agencies`, token, id ? 'PUT' : 'POST', input);
export const setAgencyActive = (token: string, id: string, isActive: boolean) =>
  request<AgencyAdmin>(`/api/v1/admin/agencies/${id}/status`, token, 'PATCH', { isActive });

export const getVehicleModels = (token: string, activeOnly = false) =>
  request<VehicleModelAdmin[]>(`/api/v1/admin/vehicle-models${activeOnly ? '?activeOnly=true' : ''}`, token);
export const saveVehicleModel = (token: string, input: SaveVehicleModelInput, id?: string) =>
  request<VehicleModelAdmin>(id ? `/api/v1/admin/vehicle-models/${id}` : '/api/v1/admin/vehicle-models', token, id ? 'PUT' : 'POST', input);
export const setVehicleModelActive = (token: string, id: string, isActive: boolean) =>
  request<VehicleModelAdmin>(`/api/v1/admin/vehicle-models/${id}/status`, token, 'PATCH', { isActive });
