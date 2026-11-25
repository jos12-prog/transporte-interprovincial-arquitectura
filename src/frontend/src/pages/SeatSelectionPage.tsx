import { ArrowLeft, Armchair, Clock3, LoaderCircle, RotateCcw } from 'lucide-react';
import { useCallback, useEffect, useState } from 'react';
import { Link, useParams } from 'react-router-dom';
import { createReservationHold, getTripSeatMap, releaseReservationHold, TripSeatMap } from '@/api/reservations';
import { SeatLayoutView } from '@/components/seats/SeatLayoutView';

function formatCountdown(expiresAtUtc: string, now: number): string {
  const seconds = Math.max(0, Math.ceil((new Date(expiresAtUtc).getTime() - now) / 1000));
  return `${Math.floor(seconds / 60).toString().padStart(2, '0')}:${(seconds % 60).toString().padStart(2, '0')}`;
}

export function SeatSelectionPage() {
  const { tripId = '' } = useParams();
  const [seatMap, setSeatMap] = useState<TripSeatMap | null>(null);
  const [selected, setSelected] = useState<number[]>([]);
  const [hold, setHold] = useState<{ reservationId: string; holdToken: string; expiresAtUtc: string } | null>(null);
  const [now, setNow] = useState(Date.now());
  const [loading, setLoading] = useState(true);
  const [busy, setBusy] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const refreshMap = useCallback(async (signal?: AbortSignal) => {
    if (!tripId) return;
    const freshMap = await getTripSeatMap(tripId, signal);
    setSeatMap(freshMap);
  }, [tripId]);

  useEffect(() => {
    const controller = new AbortController();
    setLoading(true);
    refreshMap(controller.signal)
      .catch(reason => {
        if (!controller.signal.aborted) setError(reason instanceof Error ? reason.message : 'No pudimos cargar el mapa de asientos.');
      })
      .finally(() => {
        if (!controller.signal.aborted) setLoading(false);
      });
    return () => controller.abort();
  }, [refreshMap]);

  useEffect(() => {
    if (!hold) return undefined;
    const timer = window.setInterval(() => setNow(Date.now()), 1000);
    return () => window.clearInterval(timer);
  }, [hold]);

  function toggleSeat(number: number) {
    setSelected(current => current.includes(number) ? current.filter(item => item !== number) :
      current.length < 10 ? [...current, number].sort((a, b) => a - b) : current);
  }

  async function submitHold() {
    if (!seatMap || selected.length === 0) return;
    setBusy(true);
    setError(null);
    try {
      const selectedIds = seatMap.seats.filter(seat => selected.includes(seat.number)).map(seat => seat.tripSeatId);
      const result = await createReservationHold(tripId, selectedIds);
      setHold(result);
      setSelected([]);
      await refreshMap();
    } catch (reason) {
      setError(reason instanceof Error ? reason.message : 'No pudimos retener esos asientos.');
      await refreshMap().catch(() => undefined);
    } finally {
      setBusy(false);
    }
  }

  async function releaseHold() {
    if (!hold) return;
    setBusy(true);
    try {
      await releaseReservationHold(hold.reservationId, hold.holdToken);
      setHold(null);
      await refreshMap();
    } catch (reason) {
      setError(reason instanceof Error ? reason.message : 'No pudimos liberar la retención.');
    } finally {
      setBusy(false);
    }
  }

  const remaining = hold ? formatCountdown(hold.expiresAtUtc, now) : null;

  return (
    <section className="mx-auto max-w-container px-page-gutter py-6 sm:py-10">
      <Link to="/pasajero/resultados" className="inline-flex items-center gap-2 text-sm font-semibold text-primary-700 hover:underline">
        <ArrowLeft aria-hidden="true" size={18} /> Volver a resultados
      </Link>
      <div className="mt-5 flex flex-wrap items-end justify-between gap-3">
        <div>
          <p className="text-sm font-semibold uppercase tracking-wide text-primary-700">Paso 1 de 3 · Selección</p>
          <h1 className="mt-1 font-display text-3xl font-extrabold text-neutral-900">Elige tus asientos</h1>
          <p className="mt-2 text-sm text-neutral-600">Selecciona hasta 10 asientos libres. La retención dura 10 minutos.</p>
        </div>
        <button type="button" onClick={() => refreshMap().catch(reason => setError(String(reason)))} className="inline-flex h-10 items-center gap-2 rounded-button border border-neutral-200 bg-white px-3 text-sm font-semibold text-primary-700">
          <RotateCcw aria-hidden="true" size={16} /> Actualizar mapa
        </button>
      </div>

      {error && <div role="alert" className="mt-5 rounded-card border border-red-200 bg-red-50 p-4 text-sm text-danger-600">{error}</div>}

      {hold && remaining && (
        <div role="status" className="mt-5 flex flex-wrap items-center justify-between gap-4 rounded-card border border-accent-500 bg-accent-100 p-4">
          <div className="flex items-center gap-3 text-neutral-900">
            <Clock3 aria-hidden="true" className="text-accent-500" />
            <div><p className="font-semibold">Asientos retenidos · Pendiente de pago</p><p className="text-sm">Se liberan en <strong className="tabular-nums">{remaining}</strong></p></div>
          </div>
          <button disabled={busy} onClick={releaseHold} className="rounded-button bg-white px-4 py-2 text-sm font-semibold text-primary-700 disabled:opacity-60">Liberar asientos</button>
        </div>
      )}

      <div className="mt-6 grid gap-6 lg:grid-cols-[minmax(0,1fr)_320px]">
        <div className="rounded-card border border-neutral-200 bg-white p-4 shadow-sm sm:p-6">
          {loading ? <div role="status" aria-label="Cargando mapa" className="flex justify-center py-16"><LoaderCircle className="animate-spin text-primary-600" /></div> :
            !seatMap?.seats.length ? <p className="py-12 text-center text-neutral-600">No hay un mapa disponible para este viaje.</p> :
              <SeatLayoutView seats={seatMap.seats} mode="select" selectedNumbers={selected} onSeatToggle={toggleSeat} ariaLabel="Mapa de asientos" />}
        </div>

        <aside className="space-y-4 lg:sticky lg:top-6 lg:self-start">
          <div className="rounded-card border border-neutral-200 bg-white p-5 shadow-sm">
            <h2 className="font-display text-lg font-bold text-neutral-900">Estado de los asientos</h2>
            <ul className="mt-4 space-y-3 text-sm">
              <li className="flex items-center gap-3"><span className="h-6 w-6 rounded-md border-2 border-primary-600 bg-white" /> Libre · Tócalo para elegirlo</li>
              <li className="flex items-center gap-3"><span className="h-6 w-6 rounded-md bg-primary-700" /> Tu selección</li>
              <li className="flex items-center gap-3"><span className="h-6 w-6 rounded-md border-2 border-dashed border-accent-500 bg-accent-100" /> Pendiente de pago · 10 min</li>
              <li className="flex items-center gap-3"><span className="h-6 w-6 rounded-md bg-neutral-200" /> Ocupado · Ya pagado</li>
            </ul>
          </div>
          <div className="rounded-card border border-neutral-200 bg-white p-5 shadow-sm">
            <h2 className="flex items-center gap-2 font-display text-lg font-bold text-neutral-900"><Armchair aria-hidden="true" className="text-primary-600" /> Tu selección</h2>
            <p className="mt-3 text-sm text-neutral-600">{selected.length} {selected.length === 1 ? 'asiento' : 'asientos'} seleccionados</p>
            <div className="mt-2 flex min-h-8 flex-wrap gap-2">{selected.map(number => <span key={number} className="rounded-pill bg-primary-100 px-3 py-1 text-sm font-semibold text-primary-700">{number.toString().padStart(2, '0')}</span>)}</div>
            <button type="button" onClick={submitHold} disabled={busy || loading || selected.length === 0 || Boolean(hold)} className="mt-5 h-12 w-full rounded-button bg-primary-600 px-4 font-semibold text-white hover:bg-primary-700 disabled:cursor-not-allowed disabled:opacity-50">
              {busy ? 'Procesando…' : hold ? 'Asientos retenidos' : 'Retener asientos · 10 min'}
            </button>
            {selected.length === 10 && <p className="mt-2 text-xs text-neutral-600">Alcanzaste el máximo de 10 asientos por retención.</p>}
          </div>
        </aside>
      </div>
    </section>
  );
}
