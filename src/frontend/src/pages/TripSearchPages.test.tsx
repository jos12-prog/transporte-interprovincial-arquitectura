import { fireEvent, render, screen, within } from '@testing-library/react';
import { MemoryRouter } from 'react-router-dom';
import { AppRoutes } from '@/App';
import { AuthProvider } from '@/auth/AuthContext';
import type { TripSearchResult } from '@/api/trips';

const sampleTrip: TripSearchResult = {
  tripId: 'trip-1',
  companyId: 'company-1',
  companyName: 'Andes Express',
  agencyId: 'agency-1',
  agencyName: 'Agencia Ayacucho',
  agencyCity: 'Ayacucho',
  origin: 'Ayacucho',
  destination: 'Lima',
  departureAtUtc: '2026-11-23T13:00:00Z',
  arrivalAtUtc: '2026-11-23T22:00:00Z',
  vehicleModelName: 'Bus 1 piso',
  price: 75,
  currency: 'PEN',
  availableSeats: 21,
};

function renderRoutes(path: string) {
  return render(
    <MemoryRouter future={{ v7_startTransition: true, v7_relativeSplatPath: true }} initialEntries={[path]}>
      <AuthProvider><AppRoutes /></AuthProvider>
    </MemoryRouter>,
  );
}

function mockApiResponse(body: unknown, ok = true, status = 200) {
  (global.fetch as jest.Mock).mockResolvedValue({
    ok,
    status,
    json: async () => body,
  });
}

function apiResponse(body: unknown) {
  return { ok: true, status: 200, json: async () => body };
}

describe('passenger trip search screens', () => {
  beforeEach(() => {
    Object.defineProperty(globalThis, 'fetch', {
      configurable: true,
      writable: true,
      value: jest.fn(),
    });
  });

  it('shows a loading skeleton while the real search request is pending', () => {
    (global.fetch as jest.Mock).mockReturnValue(new Promise(() => undefined));

    renderRoutes('/pasajero/resultados?origin=Ayacucho&destination=Lima&date=2026-11-23');

    expect(screen.getByRole('status', { name: 'Cargando viajes' })).toBeInTheDocument();
  });

  it('renders the company, agency, route, vehicle, fare, and free-seat count from the API', async () => {
    mockApiResponse([sampleTrip]);

    renderRoutes('/pasajero/resultados?origin=Ayacucho&destination=Lima&date=2026-11-23');

    expect(await screen.findByRole('heading', { name: 'Andes Express' })).toBeInTheDocument();
    const tripCard = screen.getByRole('article');
    expect(within(tripCard).getByText('Agencia Ayacucho')).toBeInTheDocument();
    expect(within(tripCard).getByText('Bus 1 piso')).toBeInTheDocument();
    expect(within(tripCard).getByText('21 asientos libres')).toBeInTheDocument();
    expect(within(tripCard).getByRole('link', { name: 'Elegir asientos' })).toHaveAttribute(
      'href',
      '/pasajero/viajes/trip-1/asientos',
    );
    expect(screen.getAllByText('Ayacucho').length).toBeGreaterThan(0);
    expect(screen.getByText('Lima')).toBeInTheDocument();
  });

  it('shows an empty state when the API returns no trips', async () => {
    mockApiResponse([]);

    renderRoutes('/pasajero/resultados?origin=Ayacucho&destination=Huanta&date=2026-11-23');

    expect(await screen.findByRole('heading', { name: 'No encontramos salidas para esta ruta' })).toBeInTheDocument();
    expect(screen.getByRole('link', { name: 'Nueva búsqueda' })).toHaveAttribute('href', '/pasajero');
  });

  it('shows a retryable error when the API request fails', async () => {
    (global.fetch as jest.Mock).mockRejectedValue(new Error('API no disponible'));

    renderRoutes('/pasajero/resultados?origin=Ayacucho&destination=Lima&date=2026-11-23');

    expect(await screen.findByRole('alert')).toHaveTextContent('API no disponible');
    expect(screen.getByRole('button', { name: 'Intentar de nuevo' })).toBeInTheDocument();
  });

  it('submits agency, departure-time, and fare filters to the search API', async () => {
    const fetchMock = global.fetch as jest.Mock;
    fetchMock.mockResolvedValueOnce(apiResponse([sampleTrip])).mockResolvedValueOnce(apiResponse([]));
    renderRoutes('/pasajero/resultados?origin=Ayacucho&destination=Lima&date=2026-11-23');
    await screen.findByRole('heading', { name: 'Andes Express' });

    fireEvent.change(screen.getByRole('combobox', { name: 'Agencia' }), { target: { value: 'agency-1' } });
    fireEvent.change(screen.getByLabelText('Salida desde'), { target: { value: '08:00' } });
    fireEvent.change(screen.getByRole('spinbutton', { name: 'Precio mínimo' }), { target: { value: '50' } });
    fireEvent.click(screen.getByRole('button', { name: 'Aplicar filtros' }));

    expect(await screen.findByRole('heading', { name: 'No encontramos salidas para esta ruta' })).toBeInTheDocument();
    const url = new URL(fetchMock.mock.calls[1][0] as string, 'http://localhost');
    expect(url.searchParams.get('agencyId')).toBe('agency-1');
    expect(url.searchParams.get('departureFrom')).toBe('08:00');
    expect(url.searchParams.get('minPrice')).toBe('50');
  });

  it('submits the search form and opens results for the selected route and date', async () => {
    mockApiResponse([sampleTrip]);

    renderRoutes('/pasajero');
    fireEvent.change(screen.getByRole('combobox', { name: 'Origen' }), { target: { value: 'Ayacucho' } });
    fireEvent.change(screen.getByRole('combobox', { name: 'Destino' }), { target: { value: 'Lima' } });
    fireEvent.click(screen.getByRole('button', { name: /Buscar viajes/ }));

    expect(await screen.findByRole('heading', { name: 'Andes Express' })).toBeInTheDocument();
  });
});
