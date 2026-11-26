import { cleanup, fireEvent, render, screen, waitFor, within } from '@testing-library/react';
import { MemoryRouter } from 'react-router-dom';
import { AuthProvider } from '@/auth/AuthContext';
import { PlatformAdminPage } from '@/pages/admin/PlatformAdminPage';
import * as adminApi from '@/api/platform-admin';

jest.mock('@/api/platform-admin', () => ({
  getPlatformCompanies: jest.fn(),
  getPlatformAgencies: jest.fn(),
  getVehicleModels: jest.fn(),
  savePlatformAgency: jest.fn(),
  savePlatformCompany: jest.fn(),
  saveVehicleModel: jest.fn(),
  setAgencyActive: jest.fn(),
  setCompanyActive: jest.fn(),
  setVehicleModelActive: jest.fn(),
}));

const company = {
  companyId: 'company-1', name: 'Andes Express', taxId: '20601234567', contactName: 'María',
  contactPhone: '999111222', isActive: true,
};
const yarisModel = {
  vehicleModelId: 'yaris', name: 'Toyota Yaris', category: 'Sedán', capacity: 4, isActive: true, isInUse: false,
  seats: [
    { number: 1, floor: 1, row: 1, column: 3 }, { number: 2, floor: 1, row: 3, column: 1 },
    { number: 3, floor: 1, row: 3, column: 2 }, { number: 4, floor: 1, row: 3, column: 3 },
  ],
};
const model = {
  vehicleModelId: 'model-1', name: 'Toyota Avanza', category: 'Familiar', capacity: 6,
  seats: [
    { number: 1, floor: 1, row: 1, column: 3 },
    { number: 2, floor: 1, row: 2, column: 1 }, { number: 3, floor: 1, row: 2, column: 2 }, { number: 4, floor: 1, row: 2, column: 3 },
    { number: 5, floor: 1, row: 3, column: 1 }, { number: 6, floor: 1, row: 3, column: 3 },
  ],
  isActive: true, isInUse: true,
};
const minivanModel = {
  vehicleModelId: 'minivan', name: 'Minivan', category: 'Familiar', capacity: 7, isActive: true, isInUse: false,
  seats: [
    { number: 1, floor: 1, row: 1, column: 3 }, { number: 2, floor: 1, row: 2, column: 1 },
    { number: 3, floor: 1, row: 2, column: 2 }, { number: 4, floor: 1, row: 2, column: 3 },
    { number: 5, floor: 1, row: 3, column: 1 }, { number: 6, floor: 1, row: 3, column: 2 }, { number: 7, floor: 1, row: 3, column: 3 },
  ],
};
const inactiveModel = { ...model, vehicleModelId: 'model-inactive', name: 'Toyota antiguo', isActive: false, isInUse: false };
const vanModel = {
  vehicleModelId: 'van', name: 'Van', category: 'Transporte', capacity: 15, isActive: true, isInUse: false,
  seats: [
    [1, 1, 3], [2, 1, 4], [3, 2, 1], [4, 2, 2], [5, 2, 3], [6, 3, 1], [7, 3, 2], [8, 3, 4],
    [9, 4, 1], [10, 4, 2], [11, 4, 4], [12, 5, 1], [13, 5, 2], [14, 5, 3], [15, 5, 4],
  ].map(([number, row, column]) => ({ number, floor: 1, row, column })),
};
const busSingleFloor = {
  vehicleModelId: 'bus1', name: 'Bus 1 piso', category: 'Bus', capacity: 44, isActive: true, isInUse: false,
  seats: Array.from({ length: 44 }, (_, index) => ({ number: index + 1, floor: 1, row: Math.floor(index / 4) + 1, column: [1, 2, 4, 5][index % 4] })),
};
const busDoubleFloor = {
  vehicleModelId: 'bus2', name: 'Bus 2 pisos', category: 'Bus', capacity: 52, isActive: true, isInUse: false,
  seats: [
    ...Array.from({ length: 12 }, (_, index) => ({ number: index + 1, floor: 1, row: Math.floor(index / 3) + 2, column: [1, 4, 5][index % 3] })),
    ...Array.from({ length: 40 }, (_, index) => ({ number: index + 13, floor: 2, row: Math.floor(index / 4) + 2, column: [1, 2, 4, 5][index % 4] })),
  ],
};

function renderPage(initialEntry = '/') {
  return render(<MemoryRouter future={{ v7_startTransition: true, v7_relativeSplatPath: true }} initialEntries={[initialEntry]}><AuthProvider initialSession={{
    accessToken: 'token', expiresAtUtc: '2026-11-25T13:00:00Z', user: { id: 'admin', role: 'super_admin' },
  }}><PlatformAdminPage /></AuthProvider></MemoryRouter>);
}

function expectSeatPosition(card: HTMLElement, seatNumber: number, rowIndex: number, columnIndex: number) {
  const seat = within(card).getByRole('img', { name: `Asiento ${seatNumber}` });
  const row = seat.parentElement!;
  const rows = row.parentElement!;
  expect(Array.from(rows.children).indexOf(row)).toBe(rowIndex);
  expect(Array.from(row.children).indexOf(seat)).toBe(columnIndex);
}

describe('platform administration screens', () => {
  beforeEach(() => {
    jest.mocked(adminApi.getPlatformCompanies).mockResolvedValue([company]);
    jest.mocked(adminApi.getPlatformAgencies).mockResolvedValue([]);
    jest.mocked(adminApi.getVehicleModels).mockResolvedValue([model, inactiveModel]);
  });

  it('loads companies and shows the empty agency state', async () => {
    renderPage();
    expect(await screen.findByText('Andes Express')).toBeInTheDocument();
    expect(screen.getByRole('table')).toBeInTheDocument();
    expect(screen.getByRole('columnheader', { name: 'RUC' })).toBeInTheDocument();
    expect(screen.getByText('20601234567')).toBeInTheDocument();
    const page = screen.getByRole('region', { name: 'Empresas de transporte' });
    expect(page).toBeInTheDocument();
    cleanup();
    renderPage('/superadmin?section=agencies');
    expect(await screen.findByText('Aún no hay agencias registradas.')).toBeInTheDocument();
    fireEvent.click(screen.getByRole('button', { name: 'Nueva agencia' }));
    expect(screen.getByLabelText('Empresa')).toHaveValue('company-1');
  });

  it('opens the platform section from its section query parameter', async () => {
    renderPage('/superadmin?section=models');

    expect(await screen.findByRole('heading', { name: 'Modelos de vehículo' })).toBeInTheDocument();
  });

  it('keeps the seat geometry locked for an in-use model while allowing its name to be edited', async () => {
    renderPage('/superadmin?section=models');
    await screen.findByRole('heading', { name: 'Toyota Avanza' });
    const modelCard = screen.getByRole('heading', { name: 'Toyota Avanza' }).closest('article')!;
    expect(modelCard).toHaveClass('flex', 'flex-col');
    expect(within(modelCard).getByRole('button', { name: 'Editar modelo Toyota Avanza' }).parentElement?.parentElement).toHaveClass('mt-auto');
    fireEvent.click(within(modelCard).getByRole('button', { name: 'Editar modelo Toyota Avanza' }));
    expect(screen.getByRole('note')).toHaveTextContent('plano y la capacidad están bloqueados');
    expect(screen.getByRole('button', { name: 'Asiento 1, Libre' })).toBeDisabled();
    expect(screen.getByLabelText('Nombre visible')).toHaveValue('Toyota Avanza');
    expect(screen.getByRole('button', { name: 'Guardar cambios' })).toBeEnabled();
  });

  it('shows visible model actions with their icons and danger color', async () => {
    renderPage('/superadmin?section=models');
    await screen.findByRole('heading', { name: 'Toyota Avanza' });
    const card = screen.getByRole('heading', { name: 'Toyota Avanza' }).closest('article')!;
    const edit = within(card).getByRole('button', { name: 'Editar modelo Toyota Avanza' });
    const deactivate = within(card).getByRole('button', { name: 'Desactivar modelo Toyota Avanza' });
    expect(edit.querySelector('svg')).toBeInTheDocument();
    expect(deactivate.querySelector('svg')).toBeInTheDocument();
    expect(deactivate).toHaveClass('border-danger-600/30', 'text-danger-700');
  });

  it('renders the complete 15-seat van plan and keeps its empty lane distinct from an aisle', async () => {
    jest.mocked(adminApi.getVehicleModels).mockResolvedValueOnce([vanModel]);
    renderPage('/superadmin?section=models');
    await screen.findByRole('heading', { name: 'Van' });
    const card = screen.getByRole('heading', { name: 'Van' }).closest('article')!;
    expect(within(card).getAllByRole('img', { name: /^Asiento/ })).toHaveLength(15);
    expect(within(card).getByRole('img', { name: 'Conductor, piso 1' })).toBeInTheDocument();
    expect(within(card).queryByRole('img', { name: 'Pasillo, piso 1' })).not.toBeInTheDocument();
  });

  it('draws Yaris, Avanza, and Minivan in the approved three-column driver layout and sorts by capacity', async () => {
    jest.mocked(adminApi.getVehicleModels).mockResolvedValueOnce([busDoubleFloor, vanModel, minivanModel, busSingleFloor, model, yarisModel]);
    renderPage('/superadmin?section=models');
    await screen.findByRole('heading', { name: 'Toyota Yaris' });
    const cards = ['Toyota Yaris', 'Toyota Avanza', 'Minivan'].map(name =>
      screen.getByRole('heading', { name }).closest('article')!,
    );

    for (const card of cards) {
      expect(within(card).getByRole('img', { name: 'Conductor, piso 1' })).toBeInTheDocument();
      const firstSeat = within(card).getByRole('img', { name: 'Asiento 1' });
      const row = firstSeat.parentElement!;
      expect(row.style.gridTemplateColumns).toContain('repeat(3,');
      expect(Array.from(row.children).indexOf(firstSeat)).toBe(2);
      expect(firstSeat.querySelector('span')).toHaveClass('absolute', 'inset-x-0', 'h-[66%]');
    }
    expectSeatPosition(cards[0], 2, 2, 0);
    expectSeatPosition(cards[0], 3, 2, 1);
    expectSeatPosition(cards[0], 4, 2, 2);
    expectSeatPosition(cards[1], 2, 1, 0);
    expectSeatPosition(cards[1], 3, 1, 1);
    expectSeatPosition(cards[1], 4, 1, 2);
    expectSeatPosition(cards[1], 5, 2, 0);
    expectSeatPosition(cards[1], 6, 2, 2);
    expectSeatPosition(cards[2], 2, 1, 0);
    expectSeatPosition(cards[2], 3, 1, 1);
    expectSeatPosition(cards[2], 4, 1, 2);
    expectSeatPosition(cards[2], 5, 2, 0);
    expectSeatPosition(cards[2], 7, 2, 2);
    const yarisSeatTwo = within(cards[0]).getByRole('img', { name: 'Asiento 2' });
    const yarisRows = yarisSeatTwo.parentElement!.parentElement!;
    expect(Array.from(yarisRows.children).indexOf(yarisSeatTwo.parentElement!)).toBe(2);
    expect(within(cards[0]).getByText('Sedán')).toBeInTheDocument();
    expect(within(cards[1]).getByText('Familiar')).toBeInTheDocument();
    expect(within(cards[2]).getByText('Familiar')).toBeInTheDocument();

    const visibleModelOrder = screen.getAllByRole('heading').filter(heading =>
      ['Toyota Yaris', 'Toyota Avanza', 'Minivan', 'Van', 'Bus 1 piso', 'Bus 2 pisos'].includes(heading.textContent ?? ''),
    ).map(heading => heading.textContent);
    expect(visibleModelOrder).toEqual(['Toyota Yaris', 'Toyota Avanza', 'Minivan', 'Van', 'Bus 1 piso', 'Bus 2 pisos']);
  });

  it('starts a new model with a 4 by 5 editor and can resize its rows and columns', async () => {
    renderPage('/superadmin?section=models');
    await screen.findByRole('heading', { name: 'Toyota Avanza' });
    fireEvent.click(screen.getByRole('button', { name: 'Nuevo modelo' }));
    const editor = screen.getByRole('region', { name: 'Editor visual del plano de asientos' });
    const grid = editor.querySelector('[data-row-count]')!;
    expect(grid).toHaveAttribute('data-row-count', '5');
    expect(grid).toHaveAttribute('data-column-count', '4');
    fireEvent.click(screen.getByRole('button', { name: 'Agregar fila' }));
    fireEvent.click(screen.getByRole('button', { name: 'Quitar columna' }));
    expect(grid).toHaveAttribute('data-row-count', '6');
    expect(grid).toHaveAttribute('data-column-count', '3');
  });

  it('renders the two bus floors with 12 lower and 40 upper seats plus the aisle and stairs', async () => {
    jest.mocked(adminApi.getVehicleModels).mockResolvedValueOnce([busSingleFloor, busDoubleFloor]);
    renderPage('/superadmin?section=models');
    await screen.findByRole('heading', { name: 'Bus 2 pisos' });
    const doubleCard = screen.getByRole('heading', { name: 'Bus 2 pisos' }).closest('article')!;
    const lower = within(doubleCard).getByRole('region', { name: 'Piso 1' });
    const upper = within(doubleCard).getByRole('region', { name: 'Piso 2' });
    expect(within(lower).getAllByRole('img', { name: /^Asiento/ })).toHaveLength(12);
    expect(within(upper).getAllByRole('img', { name: /^Asiento/ })).toHaveLength(40);
    expect(within(lower).getByRole('img', { name: 'Conductor, piso 1' })).toBeInTheDocument();
    expect(within(lower).getByRole('img', { name: 'Escalera, piso 1' })).toBeInTheDocument();
    expect(within(upper).getByRole('img', { name: 'Escalera, piso 2' })).toBeInTheDocument();
    const singleCard = screen.getByRole('heading', { name: 'Bus 1 piso' }).closest('article')!;
    expect(within(singleCard).getAllByRole('img', { name: /^Asiento/ })).toHaveLength(44);
    expect(within(singleCard).getAllByRole('img', { name: 'Pasillo, piso 1' })).toHaveLength(11);
  });

  it('uses the editor palette to remove and restore seats with automatic front-to-back numbering', async () => {
    jest.mocked(adminApi.getVehicleModels).mockResolvedValueOnce([{ ...model, isInUse: false }]);
    renderPage('/superadmin?section=models');
    await screen.findByRole('heading', { name: 'Toyota Avanza' });
    const card = screen.getByRole('heading', { name: 'Toyota Avanza' }).closest('article')!;
    fireEvent.click(within(card).getByRole('button', { name: 'Editar modelo Toyota Avanza' }));
    fireEvent.click(screen.getByRole('button', { name: 'Vacío' }));
    fireEvent.click(screen.getByRole('button', { name: 'Asiento 1, Libre' }));
    expect(screen.getByText('Asientos: 5')).toBeInTheDocument();
    fireEvent.click(screen.getByRole('button', { name: 'Asiento' }));
    fireEvent.click(screen.getByRole('button', { name: 'Vacío, piso 1, fila 1, columna 3' }));
    expect(screen.getByText('Asientos: 6')).toBeInTheDocument();
    expect(screen.getByRole('button', { name: 'Asiento 1, Libre' })).toBeInTheDocument();
  });

  it('allows editing an unused layout and reports a 409 if the server detects it became used', async () => {
    jest.mocked(adminApi.getVehicleModels).mockResolvedValueOnce([{ ...model, isInUse: false }]);
    renderPage('/superadmin?section=models');
    await screen.findByRole('heading', { name: 'Toyota Avanza' });
    const modelCard = screen.getByRole('heading', { name: 'Toyota Avanza' }).closest('article')!;
    fireEvent.click(within(modelCard).getByRole('button', { name: 'Editar modelo Toyota Avanza' }));
    fireEvent.click(screen.getByRole('button', { name: 'Vacío' }));
    const firstSeat = screen.getByRole('button', { name: 'Asiento 1, Libre' });
    fireEvent.click(firstSeat);
    expect(screen.getByRole('button', { name: 'Vacío, piso 1, fila 1, columna 3' })).toBeInTheDocument();
    jest.mocked(adminApi.saveVehicleModel).mockRejectedValueOnce(new Error('El plano está en uso y no puede modificarse (409).'));
    fireEvent.click(screen.getByRole('button', { name: 'Guardar cambios' }));
    expect(await screen.findByRole('status')).toHaveTextContent('no puede modificarse (409)');
    expect(adminApi.saveVehicleModel).toHaveBeenCalledWith('token', expect.objectContaining({ seats: expect.any(Array) }), 'model-1');
  });

  it('shows backend errors without losing the administration screen', async () => {
    jest.mocked(adminApi.getPlatformCompanies).mockRejectedValueOnce(new Error('Servicio no disponible'));
    renderPage();
    expect(await screen.findByRole('alert')).toHaveTextContent('Servicio no disponible');
    await waitFor(() => expect(adminApi.getVehicleModels).toHaveBeenCalledWith('token'));
  });
});
