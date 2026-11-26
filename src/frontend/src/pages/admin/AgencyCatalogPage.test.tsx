import { render, screen, fireEvent, waitFor, within } from '@testing-library/react';
import { MemoryRouter, Outlet, Route, Routes } from 'react-router-dom';
import { AuthProvider } from '@/auth/AuthContext';
import { AgencyCatalogPage } from '@/pages/admin/AgencyCatalogPage';
import { agencyCatalogApi } from '@/api/agency-catalog';

jest.mock('@/api/agency-catalog', () => ({
  agencyCatalogApi: {
    routes: jest.fn().mockResolvedValue([]), vehicles: jest.fn().mockResolvedValue([]),
    trips: jest.fn().mockResolvedValue([]), fares: jest.fn().mockResolvedValue([]),
    models: jest.fn().mockResolvedValue([
      { vehicleModelId: 'active', name: 'Toyota Avanza', category: 'Familiar', capacity: 6, isActive: true, seats: [{ number: 1, floor: 1, row: 1, column: 3 }] },
    ]),
    createVehicle: jest.fn().mockResolvedValue({}), updateVehicle: jest.fn(), createRoute: jest.fn(), updateRoute: jest.fn(),
    createFare: jest.fn().mockResolvedValue({}), updateFare: jest.fn(), scheduleTrip: jest.fn(),
    setRouteActive: jest.fn(), setVehicleActive: jest.fn(), setFareActive: jest.fn(), cancelTrip: jest.fn(),
  },
}));

function renderCatalog(section: 'routes' | 'vehicles' | 'trips' | 'fares' = 'vehicles') {
  return render(<MemoryRouter future={{ v7_startTransition: true, v7_relativeSplatPath: true }}><AuthProvider initialSession={{ accessToken: 'token', expiresAtUtc: '', user: { id: 'u1', role: 'agency_admin' } }}><Routes><Route element={<Outlet context={{ agencyId: 'agency-1', agencyName: 'Andes Express - Ayacucho', companyName: 'Andes Express' }} />}><Route path="/" element={<AgencyCatalogPage section={section} />} /></Route></Routes></AuthProvider></MemoryRouter>);
}

describe('AgencyCatalogPage', () => {
  beforeEach(() => jest.clearAllMocks());

  it('renders a vehicle table with the plan thumbnail', async () => {
    jest.mocked(agencyCatalogApi.vehicles).mockResolvedValue([{ vehicleId: 'v1', vehicleModelId: 'active', plate: 'ABC-123', displayName: 'Toyota Avanza', vehicleModelName: 'Toyota Avanza', isActive: true, seatLayout: [{ number: 1, floor: 1, row: 1, column: 3 }] }]);
    renderCatalog();
    expect(await screen.findByText('ABC-123')).toBeInTheDocument();
    expect(screen.getByRole('table')).toBeInTheDocument();
    expect(screen.getAllByLabelText('Plano Toyota Avanza').length).toBeGreaterThan(0);
    expect(screen.getByRole('columnheader', { name: 'Placa' })).toBeInTheDocument();
    expect(screen.getByText('Familiar')).toBeInTheDocument();
    expect(screen.getByText('Activo').closest('span')?.querySelector('svg')).toBeInTheDocument();
    expect(screen.queryByText('Cambiar')).not.toBeInTheDocument();
    fireEvent.click(screen.getByRole('button', { name: 'Desactivar vehículo ABC-123' }));
    expect(agencyCatalogApi.setVehicleActive).not.toHaveBeenCalled();
    expect(screen.getByRole('alertdialog')).toHaveTextContent('No podrá asignarse a nuevos viajes.');
    fireEvent.click(within(screen.getByRole('alertdialog')).getByRole('button', { name: 'Desactivar' }));
    await waitFor(() => expect(agencyCatalogApi.setVehicleActive).toHaveBeenCalledWith(expect.anything(), 'agency-1', 'v1', false));
  });

  it('shows only the state badge and an activate action for inactive vehicles', async () => {
    jest.mocked(agencyCatalogApi.vehicles).mockResolvedValue([{ vehicleId: 'v2', vehicleModelId: 'active', plate: 'XYZ-456', displayName: 'Toyota Avanza', vehicleModelName: 'Toyota Avanza', isActive: false, seatLayout: [] }]);
    renderCatalog();
    expect(await screen.findByText('Inactivo')).toHaveClass('rounded-full');
    const activate = screen.getByRole('button', { name: 'Activar vehículo XYZ-456' });
    expect(activate.querySelector('svg')).toBeInTheDocument();
    expect(activate).toHaveClass('border-success-600/30');
    fireEvent.click(activate);
    await waitFor(() => expect(agencyCatalogApi.setVehicleActive).toHaveBeenCalledWith(expect.anything(), 'agency-1', 'v2', true));
    expect(screen.queryByRole('switch')).not.toBeInTheDocument();
  });

  it('shows a trip cancellation conflict inside the confirmation dialog', async () => {
    jest.mocked(agencyCatalogApi.trips).mockResolvedValue([{ tripId: 'trip-1', routeId: 'r1', vehicleId: 'v1', origin: 'Ayacucho', destination: 'Lima', vehicleDisplayName: 'Toyota Yaris', vehicleModelName: 'Toyota Yaris', departureAtUtc: '2026-11-25T13:00:00Z', arrivalAtUtc: '2026-11-25T22:00:00Z', price: 80, currency: 'PEN', status: 'Scheduled' }]);
    jest.mocked(agencyCatalogApi.cancelTrip).mockRejectedValueOnce(new Error('Hay ventas confirmadas (409).'));
    renderCatalog('trips');
    fireEvent.click(await screen.findByRole('button', { name: 'Cancelar viaje Ayacucho a Lima' }));
    expect(agencyCatalogApi.cancelTrip).not.toHaveBeenCalled();
    fireEvent.click(within(screen.getByRole('alertdialog')).getByRole('button', { name: 'Cancelar viaje' }));
    expect(await screen.findByRole('alert')).toHaveTextContent('Hay ventas confirmadas (409).');
    expect(screen.getByRole('alertdialog')).toBeInTheDocument();
  });

  it('opens the new vehicle drawer and closes it on Escape', async () => {
    renderCatalog();
    fireEvent.click(await screen.findByRole('button', { name: /Registrar vehículo/ }));
    expect(await screen.findByRole('dialog', { name: 'Registrar vehículo' })).toHaveAttribute('aria-modal', 'true');
    expect(screen.getByRole('radio', { name: /Toyota Avanza/ })).toBeInTheDocument();
    const previewSeat = screen.getAllByRole('img', { name: 'Asiento 1' }).find(element => element.className.includes('h-7'));
    expect(previewSeat).toHaveClass('h-7', 'w-7');
    expect(screen.getAllByText('Toyota Avanza').find(element => element.classList.contains('mt-2'))).toHaveClass('mt-2');
    expect(agencyCatalogApi.models).toHaveBeenCalledWith(expect.anything(), 'agency-1');
    fireEvent.keyDown(document, { key: 'Escape' });
    await waitFor(() => expect(screen.queryByRole('dialog')).not.toBeInTheDocument());
  });

  it('registers a vehicle from the drawer and shows the inherited seat plan preview', async () => {
    renderCatalog();
    fireEvent.click(await screen.findByRole('button', { name: /Registrar vehículo/ }));
    expect(await screen.findByText('Vista previa · Toyota Avanza')).toBeInTheDocument();
    fireEvent.change(screen.getByLabelText('Placa'), { target: { value: 'AEX-77' } });
    fireEvent.change(screen.getByLabelText('Nombre visible'), { target: { value: 'Toyota Avanza' } });
    fireEvent.click(screen.getByRole('button', { name: 'Guardar' }));
    await waitFor(() => expect(agencyCatalogApi.createVehicle).toHaveBeenCalledWith(expect.anything(), 'agency-1', { plate: 'AEX-77', displayName: 'Toyota Avanza', vehicleModelId: 'active' }));
  });

  it('allows switching between week and list views', async () => {
    renderCatalog('trips');
    expect(await screen.findByRole('button', { name: 'Lista' })).toHaveAttribute('aria-pressed', 'true');
    fireEvent.click(screen.getByRole('button', { name: 'Semana' }));
    expect(screen.getByRole('button', { name: 'Semana' })).toHaveAttribute('aria-pressed', 'true');
    expect(screen.getAllByRole('region')).toHaveLength(7);
    fireEvent.click(screen.getByRole('button', { name: 'Lista' }));
    expect(screen.getByRole('button', { name: 'Lista' })).toHaveAttribute('aria-pressed', 'true');
  });

  it('loads fares for every route without requiring prior route selection', async () => {
    jest.mocked(agencyCatalogApi.routes).mockResolvedValue([
      { routeId: 'r1', originCity: 'Ayacucho', destinationCity: 'Lima', isActive: true },
      { routeId: 'r2', originCity: 'Huanta', destinationCity: 'Ayacucho', isActive: true },
    ]);
    jest.mocked(agencyCatalogApi.fares).mockImplementation(async (_session, _agency, routeId) => routeId === 'r1' ? [{ fareId: 'f1', routeId: 'r1', type: 'Base', validFrom: '2026-11-01', validTo: null, price: 75, currency: 'PEN', isActive: true }] : []);
    renderCatalog('fares');
    expect(await screen.findByText('Ayacucho → Lima')).toBeInTheDocument();
    expect(screen.getByText('Huanta → Ayacucho')).toBeInTheDocument();
    expect(screen.getByText('S/ 75.00')).toBeInTheDocument();
    expect(screen.getByRole('columnheader', { name: 'Estado' })).toBeInTheDocument();
    const priceCell = screen.getByText('S/ 75.00').closest('td');
    expect(priceCell?.querySelector('svg')).not.toBeInTheDocument();
    expect(screen.getByText('Activo').closest('td')?.querySelector('svg')).toBeInTheDocument();
    expect(agencyCatalogApi.fares).toHaveBeenCalledWith(expect.anything(), 'agency-1', 'r1');
    expect(agencyCatalogApi.fares).toHaveBeenCalledWith(expect.anything(), 'agency-1', 'r2');
    expect(screen.getByText('Andes Express · tarifas de la empresa')).toBeInTheDocument();
  });

  it('shows company breadcrumbs and uses only an accessible switch for route status', async () => {
    jest.mocked(agencyCatalogApi.routes).mockResolvedValue([
      { routeId: 'r1', originCity: 'Ayacucho', destinationCity: 'Lima', isActive: true },
      { routeId: 'r2', originCity: 'Huanta', destinationCity: 'Ayacucho', isActive: false },
    ]);
    renderCatalog('routes');
    expect(await screen.findByText('Ayacucho → Lima')).toBeInTheDocument();
    expect(screen.getByText('Andes Express · rutas de la empresa')).toBeInTheDocument();
    expect(screen.getByRole('switch', { name: 'Desactivar ruta Ayacucho → Lima' })).toHaveAttribute('aria-checked', 'true');
    expect(screen.getByRole('switch', { name: 'Activar ruta Huanta → Ayacucho' })).toHaveAttribute('aria-checked', 'false');
    expect(screen.getByText('Activo').closest('span')?.querySelector('svg')).toBeInTheDocument();
    expect(screen.getByText('Inactivo').closest('span')?.querySelector('svg')).toBeInTheDocument();
  });

  it('formats trip timestamps in Lima and shows a status badge', async () => {
    jest.mocked(agencyCatalogApi.trips).mockResolvedValue([{ tripId: 't1', routeId: 'r1', vehicleId: 'v1', origin: 'Ayacucho', destination: 'Lima', vehicleDisplayName: 'Toyota Yaris', vehicleModelName: 'Toyota Yaris', departureAtUtc: '2026-11-25T13:00:00Z', arrivalAtUtc: '2026-11-25T22:00:00Z', price: 75, currency: 'PEN', status: 'Scheduled' }]);
    renderCatalog('trips');
    expect(await screen.findByText('Programado')).toBeInTheDocument();
    expect(screen.getByText('Andes Express - Ayacucho')).toBeInTheDocument();
    expect(screen.getByText('mié 25 nov · 08:00')).toBeInTheDocument();
    expect(screen.getByText('mié 25 nov · 17:00')).toBeInTheDocument();
    expect(screen.getByText('Programado')).toHaveClass('rounded-full');
    expect(screen.getByText('Programado').closest('span')?.querySelector('svg')).toBeInTheDocument();
  });

  it('keeps the trip conflict detail inside the scheduling drawer', async () => {
    jest.mocked(agencyCatalogApi.scheduleTrip).mockRejectedValue(new Error('La ruta no tiene tarifa vigente para esa fecha'));
    jest.mocked(agencyCatalogApi.routes).mockResolvedValue([{ routeId: 'r1', originCity: 'Ayacucho', destinationCity: 'Lima', isActive: true }]);
    jest.mocked(agencyCatalogApi.vehicles).mockResolvedValue([{ vehicleId: 'v1', vehicleModelId: 'active', plate: 'ABC-123', displayName: 'Toyota Avanza', vehicleModelName: 'Toyota Avanza', isActive: true, seatLayout: [{ number: 1, floor: 1, row: 1, column: 3 }] }]);
    renderCatalog('trips');
    fireEvent.click(await screen.findByRole('button', { name: /Programar viaje/ }));
    fireEvent.change(screen.getByLabelText('Ruta'), { target: { value: 'r1' } });
    fireEvent.change(screen.getByLabelText('Vehículo'), { target: { value: 'v1' } });
    fireEvent.change(screen.getByLabelText('Salida'), { target: { value: '2026-11-26T08:00' } });
    fireEvent.change(screen.getByLabelText('Llegada'), { target: { value: '2026-11-26T17:00' } });
    fireEvent.click(screen.getByRole('button', { name: 'Guardar' }));
    expect(await screen.findByRole('alert')).toHaveTextContent('La ruta no tiene tarifa vigente para esa fecha');
    expect(screen.getByRole('dialog')).toBeInTheDocument();
  });
});
