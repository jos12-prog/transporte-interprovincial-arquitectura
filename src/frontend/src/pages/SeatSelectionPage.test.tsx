import { fireEvent, render, screen, waitFor } from '@testing-library/react';
import { MemoryRouter, Route, Routes } from 'react-router-dom';
import { SeatSelectionPage } from '@/pages/SeatSelectionPage';
import type { TripSeatMap } from '@/api/reservations';

const seatMap: TripSeatMap = {
  tripId: 'trip-1',
  seats: [
    { tripSeatId: 'seat-1', number: 1, row: 1, column: 1, floor: 1, status: 'Available' },
    { tripSeatId: 'seat-2', number: 2, row: 1, column: 2, floor: 1, status: 'Available' },
    { tripSeatId: 'seat-3', number: 3, row: 2, column: 1, floor: 1, status: 'Held' },
    { tripSeatId: 'seat-4', number: 4, row: 2, column: 2, floor: 2, status: 'Occupied' },
  ],
};

function response(body: unknown, ok = true, status = 200) {
  return { ok, status, json: async () => body };
}

function renderSeatPage() {
  return render(
    <MemoryRouter future={{ v7_startTransition: true, v7_relativeSplatPath: true }} initialEntries={['/pasajero/viajes/trip-1/asientos']}>
      <Routes>
        <Route path="/pasajero/viajes/:tripId/asientos" element={<SeatSelectionPage />} />
      </Routes>
    </MemoryRouter>,
  );
}

describe('seat selection page', () => {
  beforeEach(() => {
    Object.defineProperty(globalThis, 'fetch', { configurable: true, writable: true, value: jest.fn() });
  });

  afterEach(() => jest.restoreAllMocks());

  it('renders the seat map by floor and communicates available, held, and occupied states accessibly', async () => {
    (global.fetch as jest.Mock).mockResolvedValue(response(seatMap));

    renderSeatPage();

    expect(await screen.findByRole('button', { name: 'Asiento 1, Libre' })).toBeInTheDocument();
    expect(screen.getByRole('button', { name: 'Asiento 3, Pendiente de pago' })).toBeDisabled();
    expect(screen.getByRole('button', { name: 'Asiento 4, Ocupado' })).toBeDisabled();
    expect(screen.getByRole('region', { name: 'Piso 1' })).toBeInTheDocument();
    expect(screen.getByRole('region', { name: 'Piso 2' })).toBeInTheDocument();
  });

  it('selects multiple seats and creates a hold with trip seat IDs', async () => {
    jest.spyOn(Date, 'now').mockReturnValue(Date.parse('2026-11-23T12:00:00Z'));
    const fetchMock = global.fetch as jest.Mock;
    fetchMock.mockResolvedValueOnce(response(seatMap))
      .mockResolvedValueOnce(response({ reservationId: 'hold-1', holdToken: 'opaque-token', expiresAtUtc: '2026-11-23T12:10:00Z' }))
      .mockResolvedValueOnce(response(seatMap));
    renderSeatPage();

    fireEvent.click(await screen.findByRole('button', { name: 'Asiento 1, Libre' }));
    fireEvent.click(screen.getByRole('button', { name: 'Asiento 2, Libre' }));
    expect(screen.getByText('2 asientos seleccionados')).toBeInTheDocument();
    fireEvent.click(screen.getByRole('button', { name: 'Retener asientos · 10 min' }));

    expect(await screen.findByText('Asientos retenidos · Pendiente de pago')).toBeInTheDocument();
    const holdCall = fetchMock.mock.calls[1];
    expect(holdCall[0]).toBe('/api/v1/reservations/holds');
    expect(JSON.parse(holdCall[1].body as string)).toEqual({ tripId: 'trip-1', tripSeatIds: ['seat-1', 'seat-2'] });
    expect(screen.getByRole('status')).toHaveTextContent('10:00');
  });

  it('limits selection to ten seats', async () => {
    const tenSeats: TripSeatMap = {
      tripId: 'trip-1',
      seats: Array.from({ length: 11 }, (_, index) => ({
        tripSeatId: `seat-${index + 1}`, number: index + 1, row: index + 1, column: 1, floor: 1, status: 'Available' as const,
      })),
    };
    (global.fetch as jest.Mock).mockResolvedValue(response(tenSeats));
    renderSeatPage();

    for (let number = 1; number <= 11; number += 1) {
      const seat = await screen.findByRole('button', { name: `Asiento ${number}, Libre` });
      fireEvent.click(seat);
    }

    expect(screen.getByText('10 asientos seleccionados')).toBeInTheDocument();
    expect(screen.getByText('Alcanzaste el máximo de 10 asientos por retención.')).toBeInTheDocument();
    expect(screen.getByRole('button', { name: 'Asiento 11, Libre' })).toHaveAttribute('aria-pressed', 'false');
  });

  it('refreshes after a 409 conflict and displays the API problem', async () => {
    const fetchMock = global.fetch as jest.Mock;
    fetchMock.mockResolvedValueOnce(response(seatMap))
      .mockResolvedValueOnce(response({ title: 'Seat conflict', detail: 'Uno de los asientos ya no está libre.' }, false, 409))
      .mockResolvedValueOnce(response(seatMap));
    renderSeatPage();
    fireEvent.click(await screen.findByRole('button', { name: 'Asiento 1, Libre' }));
    fireEvent.click(screen.getByRole('button', { name: 'Retener asientos · 10 min' }));

    expect(await screen.findByRole('alert')).toHaveTextContent('Uno de los asientos ya no está libre.');
    expect(fetchMock).toHaveBeenCalledTimes(3);
  });

  it('releases a hold using X-Hold-Token and accepts the empty 204 response', async () => {
    const fetchMock = global.fetch as jest.Mock;
    fetchMock.mockResolvedValueOnce(response(seatMap))
      .mockResolvedValueOnce(response({ reservationId: 'hold-1', holdToken: 'opaque-token', expiresAtUtc: '2099-11-23T12:10:00Z' }))
      .mockResolvedValueOnce(response(seatMap))
      .mockResolvedValueOnce({ ok: true, status: 204, json: async () => { throw new Error('204 has no body'); } })
      .mockResolvedValueOnce(response(seatMap));
    renderSeatPage();
    fireEvent.click(await screen.findByRole('button', { name: 'Asiento 1, Libre' }));
    fireEvent.click(screen.getByRole('button', { name: 'Retener asientos · 10 min' }));
    fireEvent.click(await screen.findByRole('button', { name: 'Liberar asientos' }));

    await waitFor(() => expect(fetchMock).toHaveBeenCalledTimes(5));
    const releaseCall = fetchMock.mock.calls[3];
    expect(releaseCall[0]).toBe('/api/v1/reservations/holds/hold-1');
    expect(releaseCall[1].headers).toEqual({ Accept: 'application/json', 'X-Hold-Token': 'opaque-token' });
    expect(await screen.findByRole('button', { name: 'Retener asientos · 10 min' })).toBeInTheDocument();
  });
});
