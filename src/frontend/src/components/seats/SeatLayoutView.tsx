import type { SeatState } from '@/api/reservations';

export type SeatCellTool = 'seat' | 'aisle' | 'driver' | 'empty';
export type SeatLayoutMode = 'preview' | 'select' | 'edit';

export interface SeatLayoutPosition {
  number: number;
  floor: number;
  row: number | null;
  column: number | null;
  status?: SeatState;
}

export interface SeatLayoutCell {
  kind: SeatCellTool | 'stairs';
  floor: number;
  row: number;
  column: number;
  seat?: SeatLayoutPosition;
}

interface SeatLayoutViewProps {
  seats: SeatLayoutPosition[];
  modelName?: string;
  mode?: SeatLayoutMode;
  selectedNumbers?: number[];
  onSeatToggle?: (seatNumber: number) => void;
  onCellAction?: (cell: SeatLayoutCell) => void;
  disabled?: boolean;
  compact?: boolean;
  floor?: number;
  floorCount?: number;
  rows?: number;
  columns?: number;
  ariaLabel?: string;
}

type LayoutKind = 'compact-car' | 'van' | 'bus1' | 'bus2' | 'generic';

function getLayoutKind(seats: SeatLayoutPosition[], modelName?: string): LayoutKind {
  const normalized = modelName?.toLocaleLowerCase('es-PE') ?? '';
  if (normalized.includes('bus 2')) return 'bus2';
  if (normalized.includes('bus')) return 'bus1';
  if (normalized.includes('van') && !normalized.includes('minivan')) return 'van';
  if (normalized.includes('yaris') || normalized.includes('avanza') || normalized.includes('minivan')) return 'compact-car';

  const floors = new Set(seats.map(seat => seat.floor));
  if (floors.size > 1 && seats.length === 52) return 'bus2';
  if (seats.length === 44) return 'bus1';
  if (seats.length === 15) return 'van';
  if ([4, 6, 7].includes(seats.length) && seats.some(seat => seat.number === 1 && seat.row === 1 && seat.column === 3)) return 'compact-car';
  return 'generic';
}

function getRows(seats: SeatLayoutPosition[], floor: number): number {
  const positioned = seats.filter(seat => seat.floor === floor && seat.row !== null);
  if (positioned.length) return Math.max(...positioned.map(seat => seat.row!));
  return Math.ceil(seats.filter(seat => seat.floor === floor).length / 4);
}

function getColumns(kind: LayoutKind, seats: SeatLayoutPosition[], floor: number): number {
  const columns = seats.filter(seat => seat.floor === floor).map(seat => seat.column).filter((column): column is number => column !== null);
  if (columns.length) return Math.max(...columns);
  if (kind === 'compact-car') return 3;
  if (kind === 'van') return 4;
  if (kind === 'bus1' || kind === 'bus2') return 5;
  return 4;
}

function isDriverCell(kind: LayoutKind, floor: number, row: number, column: number): boolean {
  return row === 1 && column === 1 && (kind === 'compact-car' || kind === 'van' || (kind === 'bus2' && floor === 1));
}

function isStairsCell(kind: LayoutKind, floor: number, row: number, column: number): boolean {
  return kind === 'bus2' && column === 1 && ((floor === 1 && row === 6) || (floor === 2 && row === 1));
}

function isAisleCell(kind: LayoutKind, floor: number, column: number, seats: SeatLayoutPosition[]): boolean {
  if (column === 3 && (kind === 'bus1' || kind === 'bus2')) return true;
  if (kind !== 'generic') return false;
  const columns = seats.filter(seat => seat.floor === floor && seat.column !== null).map(seat => seat.column!);
  const min = Math.min(...columns);
  const max = Math.max(...columns);
  return column > min && column < max && !columns.includes(column);
}

function cellKind(kind: LayoutKind, seats: SeatLayoutPosition[], floor: number, row: number, column: number): SeatLayoutCell['kind'] {
  if (isDriverCell(kind, floor, row, column)) return 'driver';
  if (isStairsCell(kind, floor, row, column)) return 'stairs';
  if (isAisleCell(kind, floor, column, seats)) return 'aisle';
  return 'empty';
}

function statusLabel(seat: SeatLayoutPosition, selected: boolean): string {
  if (selected) return 'Seleccionado';
  if (seat.status === 'Held') return 'Pendiente de pago';
  if (seat.status === 'Occupied') return 'Ocupado';
  return 'Libre';
}

function SeatGlyph({ compact = false }: { compact?: boolean }) {
  return <svg aria-hidden="true" viewBox="0 0 48 50" className={compact ? 'absolute inset-0 h-full w-full' : 'absolute inset-1 h-[calc(100%-0.5rem)] w-[calc(100%-0.5rem)]'}>
    <rect x="1.5" y="18" width="9" height="25" rx="4.5" fill="currentColor" opacity=".24" />
    <rect x="37.5" y="18" width="9" height="25" rx="4.5" fill="currentColor" opacity=".24" />
    <rect x="8" y="1.5" width="32" height="35" rx="9" fill="currentColor" opacity=".13" stroke="currentColor" strokeWidth="2" />
    <rect x="6.5" y="31" width="35" height="15" rx="6" fill="currentColor" stroke="currentColor" strokeWidth="2" />
  </svg>;
}

function SeatCell({ seat, mode, selected, compact, disabled, onSeatToggle, onCellAction }: {
  seat: SeatLayoutPosition; mode: SeatLayoutMode; selected: boolean; compact: boolean; disabled: boolean;
  onSeatToggle?: (number: number) => void; onCellAction?: (cell: SeatLayoutCell) => void;
}) {
  const isEditor = mode === 'edit';
  const unavailable = !isEditor && seat.status !== undefined && seat.status !== 'Available';
  const seatClass = selected ? 'border-primary-700 bg-primary-700 text-white' :
    seat.status === 'Held' ? 'border-dashed border-accent-500 bg-accent-100 text-neutral-600' :
      seat.status === 'Occupied' ? 'border-neutral-200 bg-neutral-200 text-neutral-600 line-through' :
        'border-primary-600 bg-white text-primary-700 hover:bg-primary-100';
  const size = compact ? 'h-7 w-7 rounded-md text-[9px]' : 'h-14 w-14 rounded-t-xl rounded-b-md text-sm';
  const content = <><SeatGlyph compact={compact} /><span className={`absolute inset-x-0 top-0 z-10 grid h-[66%] place-items-center font-bold tabular-nums ${compact ? 'text-[8px]' : 'text-sm'}`}>{String(seat.number).padStart(2, '0')}</span></>;
  if (mode === 'preview') {
    return <div role="img" aria-label={`Asiento ${seat.number}`} className={`relative grid ${size} place-items-center border-2 font-bold ${seatClass}`}>{content}</div>;
  }
  return <button type="button" aria-label={`Asiento ${seat.number}, ${statusLabel(seat, selected)}`} aria-pressed={selected}
    disabled={disabled || unavailable} onClick={() => isEditor ? onCellAction?.({ kind: 'seat', floor: seat.floor, row: seat.row ?? 1, column: seat.column ?? 1, seat }) : onSeatToggle?.(seat.number)}
    className={`relative grid ${size} place-items-center border-2 font-bold transition focus-visible:outline focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-primary-700 disabled:cursor-not-allowed ${seatClass}`}>{content}</button>;
}

function SpecialCell({ cell, mode, compact, disabled, onCellAction }: {
  cell: SeatLayoutCell; mode: SeatLayoutMode; compact: boolean; disabled: boolean; onCellAction?: (cell: SeatLayoutCell) => void;
}) {
  const size = compact ? 'h-7 w-7 text-[8px]' : 'h-14 w-14 text-[10px]';
  const className = cell.kind === 'aisle'
    ? `grid ${size} place-items-center bg-[repeating-linear-gradient(135deg,#e7edeb_0,#e7edeb_2px,#f7faf9_2px,#f7faf9_6px)] text-neutral-400`
    : cell.kind === 'driver'
      ? `grid ${size} place-items-center rounded-md bg-neutral-200 font-bold text-neutral-600`
      : cell.kind === 'stairs'
        ? `grid ${size} place-items-center rounded-md bg-neutral-200 font-medium text-neutral-600`
        : `grid ${size} place-items-center rounded-md border border-dashed border-neutral-200 bg-white/80 text-neutral-300`;
  const label = cell.kind === 'driver' ? 'Conductor' : cell.kind === 'stairs' ? 'Escalera' : cell.kind === 'aisle' ? 'Pasillo' : 'Vacío';
  const content = cell.kind === 'driver' ? compact ? 'C' : <><span>C</span><small className="text-[8px]">Conductor</small></> : cell.kind === 'stairs' ? 'E' : null;
  if (mode !== 'edit') return <div role="img" aria-label={`${label}, piso ${cell.floor}`} className={className}>{content}</div>;
  return <button type="button" aria-label={`${label}, piso ${cell.floor}, fila ${cell.row}, columna ${cell.column}`} disabled={disabled}
    onClick={() => onCellAction?.(cell)} className={`${className} disabled:cursor-not-allowed`}>{content}</button>;
}

export function SeatLayoutView({ seats, modelName, mode = 'preview', selectedNumbers = [], onSeatToggle, onCellAction, disabled = false, compact = false, floor, floorCount, rows, columns, ariaLabel = 'Plano de asientos' }: SeatLayoutViewProps) {
  if (!seats.length && mode !== 'edit') return <p className="py-4 text-center text-sm text-neutral-500">No hay asientos en este plano.</p>;
  const kind = getLayoutKind(seats, modelName);
  const seatFloors = [...new Set(seats.map(seat => seat.floor))].sort((left, right) => left - right);
  const floors = (mode === 'edit' && floorCount ? Array.from({ length: floorCount }, (_, index) => index + 1) : seatFloors)
    .filter(item => floor === undefined || item === floor);
  return <div className={compact ? 'space-y-3' : 'space-y-8'} aria-label={ariaLabel}>
    {floors.map(currentFloor => {
      const floorSeats = seats.filter(seat => seat.floor === currentFloor);
      const rowCount = Math.max(getRows(seats, currentFloor), rows ?? 0, kind === 'bus2' && currentFloor === 1 ? 6 : 1, mode === 'edit' && !floorSeats.length ? kind === 'bus2' ? currentFloor === 1 ? 6 : 11 : 1 : 1);
      const columnCount = mode === 'edit' && columns ? columns : getColumns(kind, seats, currentFloor);
      const seatAt = new Map(floorSeats.filter(seat => seat.row !== null && seat.column !== null).map(seat => [`${seat.row}:${seat.column}`, seat]));
      const hasTwoFloors = (floorCount ?? seatFloors.length) > 1;
      return <section key={currentFloor} aria-label={hasTwoFloors ? `Piso ${currentFloor}` : ariaLabel} className={`mx-auto w-fit rounded-2xl border-2 border-neutral-200 bg-neutral-50 ${compact ? 'p-2' : 'p-4 sm:p-5'}`}>
        {hasTwoFloors ? <h3 className="mb-2 text-center text-xs font-semibold text-neutral-600">Piso {currentFloor}</h3> : null}
        {mode === 'select' && currentFloor === floors[0] ? <div className="mb-3 text-center text-[10px] font-bold tracking-[0.15em] text-neutral-500">FRENTE DEL VEHÍCULO</div> : null}
        <div className="flex flex-col items-center gap-1" data-row-count={rowCount} data-column-count={columnCount}>
          {Array.from({ length: rowCount }, (_, rowIndex) => {
            const row = rowIndex + 1;
            const spacer = kind === 'compact-car' && seats.length === 4 && row === 2;
            if (spacer) return <div aria-hidden="true" key={row} className={compact ? 'h-3' : 'h-6'} />;
            return <div key={row} className="grid gap-1" style={{ gridTemplateColumns: `repeat(${columnCount}, minmax(0, 1fr))` }}>
              {Array.from({ length: columnCount }, (_, columnIndex) => {
                const column = columnIndex + 1;
                const cell: SeatLayoutCell = { kind: cellKind(kind, seats, currentFloor, row, column), floor: currentFloor, row, column };
                if (isDriverCell(kind, currentFloor, row, column) || isStairsCell(kind, currentFloor, row, column) || isAisleCell(kind, currentFloor, column, seats)) {
                  return <SpecialCell key={column} cell={cell} mode={mode} compact={compact} disabled={disabled} onCellAction={onCellAction} />;
                }
                const seat = seatAt.get(`${row}:${column}`);
                if (seat) return <SeatCell key={column} seat={seat} mode={mode} selected={selectedNumbers.includes(seat.number)} compact={compact} disabled={disabled} onSeatToggle={onSeatToggle} onCellAction={onCellAction} />;
                return <SpecialCell key={column} cell={cell} mode={mode} compact={compact} disabled={disabled} onCellAction={onCellAction} />;
              })}
            </div>;
          })}
        </div>
      </section>;
    })}
    <span className="sr-only">{seats.length} asientos</span>
  </div>;
}

export function sortAndRenumberSeats(seats: SeatLayoutPosition[]): SeatLayoutPosition[] {
  return seats.slice().sort((left, right) => left.floor - right.floor || (left.row ?? Number.MAX_SAFE_INTEGER) - (right.row ?? Number.MAX_SAFE_INTEGER) || (left.column ?? Number.MAX_SAFE_INTEGER) - (right.column ?? Number.MAX_SAFE_INTEGER) || left.number - right.number)
    .map((seat, index) => ({ ...seat, number: index + 1 }));
}
