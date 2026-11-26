import { useCallback, useEffect, useRef, useState, type FormEvent, type ReactNode } from 'react';
import { useOutletContext } from 'react-router-dom';
import { SeatLayoutView } from '@/components/seats/SeatLayoutView';
import { ConfirmDialog, RowActions, StatusBadge as RowStatusBadge, Toast, type ConfirmRequest } from '@/components/admin/RowActions';
import { agencyCatalogApi, type AgencyFare, type AgencyRoute, type AgencyTrip, type AgencyVehicle, type AgencyVehicleModel } from '@/api/agency-catalog';
import { useAuth } from '@/auth/AuthContext';
import { formatLimaDate, formatLimaTripDateTime } from '@/lib/format';

type Section = 'routes' | 'vehicles' | 'trips' | 'fares';
type DrawerKind = 'route' | 'vehicle' | 'trip' | 'fare';
const headings: Record<Section, { title: string; subtitle: string; action: string }> = {
  routes: { title: 'Rutas', subtitle: 'Las rutas son de la empresa y las usan todas sus agencias.', action: 'Nueva ruta' },
  vehicles: { title: 'Vehículos', subtitle: 'El plano de asientos de cada vehículo lo define su modelo.', action: 'Registrar vehículo' },
  trips: { title: 'Viajes y horarios', subtitle: 'Programa y consulta las salidas de tu agencia.', action: 'Programar viaje' },
  fares: { title: 'Tarifas', subtitle: 'Un precio único por viaje. Las fechas especiales reemplazan el precio base en su rango.', action: 'Nueva tarifa' },
};
const standardOrder = ['Toyota Yaris', 'Toyota Avanza', 'Minivan', 'Van', 'Bus 1 piso', 'Bus 2 pisos'];
const fieldClass = 'min-h-11 w-full rounded-xl border border-neutral-300 bg-white px-3 text-sm text-neutral-900';

function Drawer({ title, onClose, children, footer, initialFocus }: { title: string; onClose: () => void; children: ReactNode; footer: ReactNode; initialFocus?: string }) {
  const dialog = useRef<HTMLElement>(null);
  const priorFocus = useRef<HTMLElement | null>(null);
  useEffect(() => {
    priorFocus.current = document.activeElement instanceof HTMLElement ? document.activeElement : null;
    const first = initialFocus ? dialog.current?.querySelector<HTMLElement>(initialFocus) : dialog.current?.querySelector<HTMLElement>('input,select,button,textarea');
    first?.focus();
    function handleKeyDown(event: KeyboardEvent) {
      if (event.key === 'Escape') { event.preventDefault(); onClose(); return; }
      if (event.key !== 'Tab' || !dialog.current) return;
      const focusable = [...dialog.current.querySelectorAll<HTMLElement>('a[href],button:not(:disabled),input:not(:disabled),select:not(:disabled),textarea:not(:disabled),[tabindex]:not([tabindex="-1"])')];
      if (!focusable.length) return;
      const firstElement = focusable[0]; const lastElement = focusable[focusable.length - 1];
      if (event.shiftKey && document.activeElement === firstElement) { event.preventDefault(); lastElement.focus(); }
      else if (!event.shiftKey && document.activeElement === lastElement) { event.preventDefault(); firstElement.focus(); }
    }
    document.addEventListener('keydown', handleKeyDown);
    return () => { document.removeEventListener('keydown', handleKeyDown); priorFocus.current?.focus(); };
  }, [initialFocus, onClose]);
  return <div className="fixed inset-0 z-50" data-testid="catalog-drawer">
    <button aria-label="Cerrar" className="fixed inset-0 bg-[rgba(10,24,22,.35)]" onClick={onClose} type="button" />
    <aside aria-labelledby="catalog-drawer-title" aria-modal="true" className="fixed inset-y-0 right-0 z-[51] flex w-full flex-col bg-white shadow-[-24px_0_48px_rgba(17,27,25,.16)] sm:max-w-[480px]" ref={dialog} role="dialog" tabIndex={-1}>
      <div className="flex items-center justify-between border-b border-neutral-200 px-6 py-5"><h2 className="text-lg font-semibold text-neutral-900" id="catalog-drawer-title">{title}</h2><button aria-label="Cerrar" className="grid h-8 w-8 place-items-center rounded-lg text-neutral-500 hover:bg-neutral-100" onClick={onClose} type="button">×</button></div>
      <div className="grid flex-1 content-start gap-[18px] overflow-auto p-6">{children}</div>
      <div className="flex justify-end gap-2.5 border-t border-neutral-200 px-6 py-4">{footer}</div>
    </aside>
  </div>;
}

function EmptyState({ children }: { children: ReactNode }) { return <div className="grid justify-items-center gap-2.5 px-4 py-12 text-center text-sm text-neutral-600">{children}</div>; }

export function AgencyCatalogPage({ section }: { section: Section }) {
  const { agencyId, agencyName, companyName } = useOutletContext<{ agencyId: string; agencyName?: string; companyName?: string }>();
  const { session } = useAuth();
  const [routes, setRoutes] = useState<AgencyRoute[]>([]);
  const [vehicles, setVehicles] = useState<AgencyVehicle[]>([]);
  const [trips, setTrips] = useState<AgencyTrip[]>([]);
  const [fares, setFares] = useState<AgencyFare[]>([]);
  const [models, setModels] = useState<AgencyVehicleModel[]>([]);
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);
  const [saving, setSaving] = useState(false);
  const [toast, setToast] = useState<{ message: string; tone: 'success' | 'error' } | null>(null);
  const [confirmation, setConfirmation] = useState<ConfirmRequest & { pending?: boolean; error?: string } | null>(null);
  const [drawer, setDrawer] = useState<DrawerKind | null>(null);
  const [editing, setEditing] = useState<AgencyRoute | AgencyVehicle | AgencyFare | null>(null);
  const [tripView, setTripView] = useState<'week' | 'list'>('list');
  const [selectedModel, setSelectedModel] = useState('');
  const [tripVehicleId, setTripVehicleId] = useState('');
  const [drawerError, setDrawerError] = useState('');

  async function refresh() {
    if (!session || !agencyId) return;
    setLoading(true); setError('');
    try {
      const [routeData, vehicleData, tripData, modelData] = await Promise.all([
        agencyCatalogApi.routes(session, agencyId), agencyCatalogApi.vehicles(session, agencyId),
        agencyCatalogApi.trips(session, agencyId), agencyCatalogApi.models(session, agencyId),
      ]);
      setRoutes(routeData); setVehicles(vehicleData); setTrips(tripData); setModels(modelData);
      const fareLists = await Promise.all(routeData.map(route => agencyCatalogApi.fares(session, agencyId, route.routeId)));
      setFares(fareLists.flat());
      setSelectedModel(current => modelData.some(model => model.vehicleModelId === current && model.capacity > 0) ? current : modelData[0]?.vehicleModelId ?? '');
    } catch (reason) { setError(reason instanceof Error ? reason.message : 'No se pudo cargar el catálogo.'); }
    finally { setLoading(false); }
  }
  useEffect(() => { void refresh(); }, [session, agencyId]);

  function openDrawer(kind: DrawerKind, item: AgencyRoute | AgencyVehicle | AgencyFare | null = null) {
    setDrawerError(''); setError(''); setEditing(item); setDrawer(kind);
    if (item && 'vehicleId' in item) setSelectedModel(item.vehicleModelId);
    if (kind === 'trip') setTripVehicleId('');
  }
  const closeDrawer = useCallback(() => { setDrawer(null); setDrawerError(''); setEditing(null); setTripVehicleId(''); }, []);
  async function changeState(operation: () => Promise<unknown>, successMessage = 'Se guardaron los cambios.') {
    setError(''); setSaving(true);
    try { await operation(); setToast({ message: successMessage, tone: 'success' }); await refresh(); }
    catch (reason) { setToast({ message: reason instanceof Error ? reason.message : 'No se pudo actualizar.', tone: 'error' }); }
    finally { setSaving(false); }
  }
  function askToConfirm(title: string, consequence: string, confirmLabel: string, operation: () => Promise<unknown>) {
    setConfirmation({ title, consequence, confirmLabel, operation: async () => { await operation(); }, pending: false, error: undefined });
  }
  async function confirmChange() {
    if (!confirmation) return;
    setConfirmation(current => current ? { ...current, pending: true, error: undefined } : null);
    try {
      await confirmation.operation();
      setConfirmation(null);
      setToast({ message: 'Se guardaron los cambios.', tone: 'success' });
      await refresh();
    } catch (reason) {
      setConfirmation(current => current ? { ...current, pending: false, error: reason instanceof Error ? reason.message : 'No se pudo completar la operación.' } : null);
    }
  }
  async function submit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault(); if (!session || !agencyId) return;
    const form = event.currentTarget; const data = new FormData(form); setDrawerError(''); setError('');
    try {
      if (drawer === 'route') {
        const values = [String(data.get('originCity')), String(data.get('destinationCity'))] as const;
        if (editing && 'routeId' in editing) await agencyCatalogApi.updateRoute(session, agencyId, editing.routeId, ...values);
        else await agencyCatalogApi.createRoute(session, agencyId, ...values);
      }
      if (drawer === 'vehicle') {
        const body = { plate: String(data.get('plate')), displayName: String(data.get('displayName')), vehicleModelId: String(data.get('vehicleModelId')) };
        if (editing && 'vehicleId' in editing) await agencyCatalogApi.updateVehicle(session, agencyId, editing.vehicleId, body);
        else await agencyCatalogApi.createVehicle(session, agencyId, body);
      }
      if (drawer === 'fare') {
        const routeId = String(data.get('routeId'));
        const body = { type: String(data.get('type')), validFrom: String(data.get('validFrom')), validTo: String(data.get('validTo')) || null, price: Number(data.get('price')), currency: 'PEN' };
        if (editing && 'fareId' in editing) await agencyCatalogApi.updateFare(session, agencyId, routeId, editing.fareId, body);
        else await agencyCatalogApi.createFare(session, agencyId, routeId, body);
      }
      if (drawer === 'trip') await agencyCatalogApi.scheduleTrip(session, agencyId, { routeId: String(data.get('routeId')), vehicleId: String(data.get('vehicleId')), departureAtUtc: new Date(String(data.get('departureAtUtc'))).toISOString(), arrivalAtUtc: new Date(String(data.get('arrivalAtUtc'))).toISOString() });
      closeDrawer(); setToast({ message: 'Se guardó correctamente.', tone: 'success' }); form.reset(); await refresh();
    } catch (reason) { setDrawerError(reason instanceof Error ? reason.message : 'No se pudo guardar.'); }
  }

  const vehicleModels = models.slice().sort((left, right) => {
    const li = standardOrder.indexOf(left.name); const ri = standardOrder.indexOf(right.name);
    return (li < 0 ? standardOrder.length : li) - (ri < 0 ? standardOrder.length : ri) || left.capacity - right.capacity || left.name.localeCompare(right.name);
  });
  const activeModels = vehicleModels.filter(model => model.isActive);
  const modelForPreview = activeModels.find(model => model.vehicleModelId === selectedModel);
  const title = headings[section];
  const routeName = (routeId: string) => { const route = routes.find(item => item.routeId === routeId); return route ? `${route.originCity} → ${route.destinationCity}` : 'Ruta'; };
  const breadcrumb = section === 'trips' ? (agencyName ?? 'Agencia') : section === 'routes' ? `${companyName ?? 'Empresa'} · rutas de la empresa` : section === 'fares' ? `${companyName ?? 'Empresa'} · tarifas de la empresa` : (companyName ?? 'Empresa');

  return <div className="space-y-6">
    <div className="mb-6 flex flex-wrap items-end justify-between gap-4">
      <div><div className="crumbs mb-1.5 text-[13px] text-neutral-500">{breadcrumb}</div><h1 className="text-3xl font-semibold text-neutral-900">{title.title}</h1><p className="mt-2 text-sm text-neutral-600">{title.subtitle}</p></div>
      <button className="min-h-11 rounded-xl bg-primary-700 px-5 text-sm font-semibold text-white hover:bg-primary-800" onClick={() => openDrawer(section === 'routes' ? 'route' : section === 'vehicles' ? 'vehicle' : section === 'trips' ? 'trip' : 'fare')} type="button">＋ {title.action}</button>
    </div>
    {!agencyId ? <p role="status" className="rounded-card bg-warning-50 p-4 text-sm text-warning-800">Selecciona una agencia en la barra lateral para gestionar su catálogo.</p> : null}
    {error ? <p className="text-sm text-danger-600" role="alert">{error}</p> : null}
    {loading ? <div aria-label="Cargando catálogo" className="h-20 animate-pulse rounded-xl bg-neutral-100" role="status" /> : null}
    {agencyId && !loading ? <>
      {section === 'routes' ? <TableState rows={routes.length} empty="Aún no hay rutas registradas."><div className="overflow-auto rounded-2xl border border-neutral-200 bg-white"><table className="w-full border-collapse text-sm"><thead className="bg-neutral-50 text-left text-xs uppercase tracking-wide text-neutral-500"><tr><th className="px-4 py-3">Ruta</th><th className="px-4 py-3">Activa</th><th className="px-4 py-3 text-right">Acción</th></tr></thead><tbody>{routes.map(route => { const label = `${route.originCity} → ${route.destinationCity}`; return <tr className="border-t border-neutral-100 even:bg-[#FCFDFE] hover:bg-primary-50" key={route.routeId}><td className="h-14 px-4 font-semibold">{label}</td><td className="h-14 px-4"><div className="flex items-center gap-2"><button aria-checked={route.isActive} aria-label={`${route.isActive ? 'Desactivar' : 'Activar'} ruta ${label}`} className={`relative h-[22px] w-[38px] shrink-0 rounded-full transition-colors disabled:cursor-not-allowed disabled:opacity-50 ${route.isActive ? 'bg-primary-700' : 'bg-neutral-300'}`} disabled={saving || !!confirmation?.pending} onClick={() => route.isActive ? askToConfirm(`¿Desactivar ruta ${label}?`, 'No se ofrecerá para viajes nuevos. Los viajes ya programados se mantienen.', 'Desactivar', () => agencyCatalogApi.setRouteActive(session!, agencyId, route.routeId, false)) : void changeState(() => agencyCatalogApi.setRouteActive(session!, agencyId, route.routeId, true), 'Ruta activada.')} role="switch" type="button"><span className={`absolute top-[3px] h-4 w-4 rounded-full bg-white ${route.isActive ? 'left-[19px]' : 'left-[3px]'}`} /></button><RowStatusBadge status={route.isActive ? 'active' : 'inactive'} /></div></td><td className="h-14 px-4 text-right"><RowActions name={label} disabled={saving || !!confirmation?.pending} actions={[{ id: 'edit', label: 'Editar', onSelect: () => openDrawer('route', route) }]} /></td></tr>; })}</tbody></table></div></TableState> : null}
        {section === 'vehicles' ? <TableState rows={vehicles.length} empty="Aún no hay vehículos registrados."><div className="overflow-auto rounded-2xl border border-neutral-200 bg-white"><table className="w-full border-collapse text-sm"><thead className="bg-neutral-50 text-left text-xs uppercase tracking-wide text-neutral-500"><tr><th className="px-4 py-3">Placa</th><th className="px-4 py-3">Nombre visible / Modelo</th><th className="px-4 py-3">Categoría</th><th className="px-4 py-3">Asientos</th><th className="px-4 py-3">Plano</th><th className="px-4 py-3">Estado</th><th className="px-4 py-3 text-right">Acción</th></tr></thead><tbody>{vehicles.map(vehicle => <tr className="border-t border-neutral-100 even:bg-[#FCFDFE] hover:bg-primary-50" key={vehicle.vehicleId}><td className="h-20 px-4 font-semibold tabular-nums">{vehicle.plate}</td><td className="h-20 px-4"><b>{vehicle.displayName}</b><span className="block text-xs text-neutral-500">{vehicle.vehicleModelName}</span></td><td className="h-20 px-4">{models.find(model => model.vehicleModelId === vehicle.vehicleModelId)?.category ?? '—'}</td><td className="h-20 px-4 tabular-nums">{vehicle.seatLayout?.length ?? '—'}</td><td className="h-20 px-4">{vehicle.seatLayout?.length ? <SeatLayoutView compact seats={vehicle.seatLayout} modelName={vehicle.vehicleModelName} ariaLabel={`Plano ${vehicle.displayName}`} /> : '—'}</td><td className="h-20 px-4"><RowStatusBadge status={vehicle.isActive ? 'active' : 'inactive'} /></td><td className="h-20 px-4 text-right"><RowActions name={`vehículo ${vehicle.plate}`} disabled={saving || !!confirmation?.pending} actions={[{ id: 'edit', label: 'Editar', onSelect: () => openDrawer('vehicle', vehicle) }, ...(vehicle.isActive ? [{ id: 'deactivate' as const, label: 'Desactivar', onSelect: () => askToConfirm(`¿Desactivar vehículo ${vehicle.plate}?`, 'No podrá asignarse a nuevos viajes. Los viajes ya programados se mantienen.', 'Desactivar', () => agencyCatalogApi.setVehicleActive(session!, agencyId, vehicle.vehicleId, false)) }] : [{ id: 'activate' as const, label: 'Activar', onSelect: () => void changeState(() => agencyCatalogApi.setVehicleActive(session!, agencyId, vehicle.vehicleId, true), 'Vehículo activado.') }])]} /></td></tr>)}</tbody></table></div></TableState> : null}
      {section === 'trips' ? <>
        <div aria-label="Vista de viajes" className="inline-flex gap-1 rounded-xl bg-neutral-100 p-1"><button aria-pressed={tripView === 'week'} className={`rounded-[9px] px-3.5 py-2 text-[13.5px] ${tripView === 'week' ? 'bg-white text-neutral-900 shadow-sm' : 'text-neutral-600'}`} onClick={() => setTripView('week')} type="button">Semana</button><button aria-pressed={tripView === 'list'} className={`rounded-[9px] px-3.5 py-2 text-[13.5px] ${tripView === 'list' ? 'bg-white text-neutral-900 shadow-sm' : 'text-neutral-600'}`} onClick={() => setTripView('list')} type="button">Lista</button></div>
        {/* La API no entrega ocupación; Semana muestra horarios, sin porcentajes inventados. Lista es la vista inicial. */}
        {tripView === 'week' ? <WeekGrid trips={trips} /> : <TableState rows={trips.length} empty="Aún no hay viajes programados."><div className="overflow-auto rounded-2xl border border-neutral-200 bg-white"><table className="w-full border-collapse text-sm"><thead className="bg-neutral-50 text-left text-xs uppercase tracking-wide text-neutral-500"><tr><th className="px-4 py-3">Ruta</th><th className="px-4 py-3">Vehículo</th><th className="px-4 py-3">Salida</th><th className="px-4 py-3">Llegada</th><th className="px-4 py-3">Precio</th><th className="px-4 py-3">Estado</th><th className="px-4 py-3 text-right">Acción</th></tr></thead><tbody>{trips.map(trip => <tr className="border-t border-neutral-100 even:bg-[#FCFDFE] hover:bg-primary-50" key={trip.tripId}><td className="h-14 px-4">{trip.origin} → {trip.destination}</td><td className="h-14 px-4">{trip.vehicleDisplayName}</td><td className="h-14 px-4">{formatLimaTripDateTime(trip.departureAtUtc)}</td><td className="h-14 px-4">{formatLimaTripDateTime(trip.arrivalAtUtc)}</td><td className="h-14 px-4">S/ {trip.price.toFixed(2)}</td><td className="h-14 px-4"><RowStatusBadge status={trip.status === 'Scheduled' ? 'scheduled' : 'cancelled'} /></td><td className="h-14 px-4 text-right">{trip.status === 'Scheduled' ? <RowActions name={`${trip.origin} a ${trip.destination}`} disabled={saving || !!confirmation?.pending} actions={[{ id: 'cancel', label: 'Cancelar viaje', onSelect: () => askToConfirm(`¿Cancelar viaje ${trip.origin} → ${trip.destination}?`, 'Se liberarán los asientos retenidos. Si hay ventas confirmadas no se podrá cancelar.', 'Cancelar viaje', () => agencyCatalogApi.cancelTrip(session!, agencyId, trip.tripId)) }]} /> : null}</td></tr>)}</tbody></table></div></TableState>}
      </> : null}
      {section === 'fares' ? <TableState rows={routes.length} empty="Aún no hay rutas para configurar tarifas."><div className="overflow-auto rounded-2xl border border-neutral-200 bg-white"><table className="w-full border-collapse text-sm"><thead className="bg-neutral-50 text-left text-xs uppercase tracking-wide text-neutral-500"><tr><th className="px-4 py-3">Ruta</th><th className="px-4 py-3">Precio base</th><th className="px-4 py-3">Vigente desde</th><th className="px-4 py-3">Hasta</th><th className="px-4 py-3">Fechas especiales</th><th className="px-4 py-3">Estado</th><th className="px-4 py-3 text-right">Acción</th></tr></thead><tbody>{routes.map(route => { const routeFares = fares.filter(fare => fare.routeId === route.routeId); const base = routeFares.find(fare => fare.type === 'Base' && fare.isActive) ?? routeFares.find(fare => fare.type === 'Base'); const specials = routeFares.filter(fare => fare.type === 'Special'); const actionFor = (fare: AgencyFare) => [{ id: 'edit' as const, label: 'Editar', onSelect: () => openDrawer('fare', fare) }, ...(fare.isActive ? [{ id: 'deactivate' as const, label: 'Desactivar', onSelect: () => askToConfirm(`¿Desactivar tarifa de ${routeName(route.routeId)}?`, 'La tarifa dejará de aplicarse a viajes nuevos. Los viajes ya programados conservan su precio.', 'Desactivar', () => agencyCatalogApi.setFareActive(session!, agencyId, route.routeId, fare.fareId, false)) }] : [{ id: 'activate' as const, label: 'Activar', onSelect: () => void changeState(() => agencyCatalogApi.setFareActive(session!, agencyId, route.routeId, fare.fareId, true), 'Tarifa activada.') }])]; return <tr className="border-t border-neutral-100 even:bg-[#FCFDFE] hover:bg-primary-50" key={route.routeId}><td className="h-14 px-4 font-semibold">{routeName(route.routeId)}</td><td className="h-14 px-4">{base ? `S/ ${base.price.toFixed(2)}` : '—'}</td><td className="h-14 px-4">{base ? formatLimaDate(base.validFrom) : '—'}</td><td className="h-14 px-4">{base?.validTo ? formatLimaDate(base.validTo) : '—'}</td><td className="px-4 py-3">{specials.length ? <div className="flex flex-wrap items-center gap-1.5">{specials.map(fare => <span className={`rounded-full px-2.5 py-1 text-xs ${fare.isActive ? 'bg-accent-100 text-neutral-800' : 'bg-neutral-100 text-neutral-500'}`} key={fare.fareId}>{fare.isActive ? 'Especial' : 'Especial · inactiva'} · S/ {fare.price.toFixed(2)} <RowActions name={`tarifa especial de ${routeName(route.routeId)}`} disabled={saving || !!confirmation?.pending} actions={actionFor(fare)} /></span>)}</div> : '—'}</td><td className="h-14 px-4"><div className="flex flex-col items-start gap-1.5">{routeFares.map(fare => <span key={fare.fareId}><RowStatusBadge status={fare.isActive ? 'active' : 'inactive'} /></span>)}</div></td><td className="h-14 px-4 text-right">{base ? <RowActions name={`tarifa de ${routeName(route.routeId)}`} disabled={saving || !!confirmation?.pending} actions={actionFor(base)} /> : <button className="text-sm font-semibold text-primary-700" onClick={() => openDrawer('fare')} type="button">Nueva tarifa</button>}</td></tr>; })}</tbody></table></div></TableState> : null}
    </> : null}

    {drawer ? <Drawer onClose={closeDrawer} title={drawer === 'route' ? editing && 'routeId' in editing ? 'Editar ruta' : 'Nueva ruta' : drawer === 'vehicle' ? editing && 'vehicleId' in editing ? 'Editar vehículo' : 'Registrar vehículo' : drawer === 'trip' ? 'Programar viaje' : editing && 'fareId' in editing ? 'Editar tarifa' : 'Nueva tarifa'} footer={<><button className="min-h-10 rounded-xl px-4 text-sm text-neutral-700 hover:bg-neutral-100" onClick={closeDrawer} type="button">Cancelar</button><button className="min-h-10 rounded-xl bg-primary-700 px-5 text-sm font-semibold text-white" form="catalog-form" type="submit">Guardar</button></>}>
      <form className="grid gap-[18px]" id="catalog-form" onSubmit={submit}>
        {drawerError ? <p className="text-sm text-danger-600" role="alert">{drawerError}</p> : null}
        {drawer === 'route' ? <><label className="grid gap-2 text-sm">Origen<input className={fieldClass} defaultValue={editing && 'originCity' in editing ? editing.originCity : ''} name="originCity" required /></label><label className="grid gap-2 text-sm">Destino<input className={fieldClass} defaultValue={editing && 'originCity' in editing ? editing.destinationCity : ''} name="destinationCity" required /></label></> : null}
        {drawer === 'vehicle' ? <><label className="grid gap-2 text-sm">Placa<input className={fieldClass} defaultValue={editing && 'vehicleId' in editing ? editing.plate : ''} name="plate" required /></label><label className="grid gap-2 text-sm">Nombre visible<input className={fieldClass} defaultValue={editing && 'vehicleId' in editing ? editing.displayName : ''} name="displayName" placeholder="Toyota Avanza" required /></label><fieldset><legend className="mb-2 text-sm font-medium">Modelo de vehículo</legend><div className="grid grid-cols-2 gap-2">{activeModels.map(model => <label className={`flex h-full cursor-pointer flex-col items-center rounded-xl border p-3 text-center ${selectedModel === model.vehicleModelId ? 'border-primary-700 bg-primary-50' : 'border-neutral-200'}`} key={model.vehicleModelId}><input checked={selectedModel === model.vehicleModelId} className="sr-only" name="vehicleModelId" onChange={() => setSelectedModel(model.vehicleModelId)} required type="radio" value={model.vehicleModelId} /><div className="flex w-full items-center justify-center py-1"><SeatLayoutView ariaLabel={`Plano ${model.name}`} compact modelName={model.name} seats={model.seats} /></div><b className="mt-2 text-xs">{model.name}</b><span className="mt-2 text-xs text-neutral-500">{model.capacity} asientos</span></label>)}</div></fieldset>{modelForPreview ? <div className="grid justify-items-center gap-3 rounded-xl border border-neutral-200 p-4"><b className="text-sm">Vista previa · {modelForPreview.name}</b><SeatLayoutView ariaLabel={`Vista previa ${modelForPreview.name}`} modelName={modelForPreview.name} seats={modelForPreview.seats} /><span className="text-xs text-neutral-500">El plano de asientos se hereda del modelo. Para otro plano, el superadministrador debe crear un modelo nuevo.</span></div> : null}</> : null}
        {drawer === 'trip' ? <><label className="grid gap-2 text-sm">Ruta<select className={fieldClass} name="routeId" required defaultValue=""><option value="" disabled>Selecciona ruta</option>{routes.filter(route => route.isActive).map(route => <option key={route.routeId} value={route.routeId}>{route.originCity} → {route.destinationCity}</option>)}</select></label><label className="grid gap-2 text-sm">Vehículo<select className={fieldClass} name="vehicleId" required defaultValue="" onChange={event => setTripVehicleId(event.target.value)}><option value="" disabled>Selecciona vehículo</option>{vehicles.filter(vehicle => vehicle.isActive).map(vehicle => <option key={vehicle.vehicleId} value={vehicle.vehicleId}>{vehicle.displayName} · {vehicle.plate}</option>)}</select></label>{vehicles.find(vehicle => vehicle.vehicleId === tripVehicleId)?.seatLayout?.length ? <SeatLayoutView ariaLabel={`Plano ${vehicles.find(vehicle => vehicle.vehicleId === tripVehicleId)?.displayName}`} compact modelName={vehicles.find(vehicle => vehicle.vehicleId === tripVehicleId)?.vehicleModelName} seats={vehicles.find(vehicle => vehicle.vehicleId === tripVehicleId)!.seatLayout!} /> : null}<label className="grid gap-2 text-sm">Salida<input className={fieldClass} name="departureAtUtc" type="datetime-local" required /></label><label className="grid gap-2 text-sm">Llegada<input className={fieldClass} name="arrivalAtUtc" type="datetime-local" required /></label></> : null}
        {drawer === 'fare' ? <><label className="grid gap-2 text-sm">Ruta<select className={fieldClass} name="routeId" required defaultValue={editing && 'fareId' in editing ? editing.routeId : ''}><option value="" disabled>Selecciona ruta</option>{routes.map(route => <option key={route.routeId} value={route.routeId}>{route.originCity} → {route.destinationCity}</option>)}</select></label><label className="grid gap-2 text-sm">Tipo<select className={fieldClass} name="type" defaultValue={editing && 'fareId' in editing ? editing.type : 'Base'}><option value="Base">Base</option><option value="Special">Especial</option></select></label><label className="grid gap-2 text-sm">Vigente desde<input className={fieldClass} defaultValue={editing && 'fareId' in editing ? editing.validFrom : ''} name="validFrom" type="date" required /></label><label className="grid gap-2 text-sm">Hasta<input className={fieldClass} defaultValue={editing && 'fareId' in editing ? editing.validTo ?? '' : ''} name="validTo" type="date" /></label><label className="grid gap-2 text-sm">Precio (PEN)<input className={fieldClass} defaultValue={editing && 'fareId' in editing ? editing.price : ''} min="0.01" name="price" step="0.01" type="number" required /></label><p className="text-xs text-neutral-500">El cambio no afecta reservas ya pagadas.</p></> : null}
      </form>
    </Drawer> : null}
    {confirmation ? <ConfirmDialog open title={confirmation.title} consequence={confirmation.consequence} confirmLabel={confirmation.confirmLabel} pending={confirmation.pending} error={confirmation.error} onConfirm={() => void confirmChange()} onCancel={() => setConfirmation(null)} /> : null}
    {toast ? <Toast message={toast.message} tone={toast.tone} onClose={() => setToast(null)} /> : null}
  </div>;
}

function TableState({ rows, empty, children }: { rows: number; empty: string; children: ReactNode }) {
  return rows ? <>{children}</> : <div className="rounded-2xl border border-neutral-200 bg-white"><EmptyState>{empty}</EmptyState></div>;
}

function WeekGrid({ trips }: { trips: AgencyTrip[] }) {
  const start = new Date(); start.setHours(0, 0, 0, 0); start.setDate(start.getDate() - ((start.getDay() + 6) % 7));
  const days = Array.from({ length: 7 }, (_, index) => { const date = new Date(start); date.setDate(start.getDate() + index); return date; });
  return <div className="grid min-w-[700px] grid-cols-7 overflow-auto rounded-2xl border border-neutral-200 bg-white">{days.map(day => {
    const entries = trips.filter(trip => new Date(trip.departureAtUtc).toDateString() === day.toDateString()).sort((a, b) => a.departureAtUtc.localeCompare(b.departureAtUtc));
    return <section aria-label={day.toLocaleDateString('es-PE', { weekday: 'long', day: 'numeric' })} className="min-h-56 border-r border-neutral-100 last:border-0" key={day.toISOString()}><h2 className="border-b border-neutral-200 bg-neutral-50 p-3 text-xs font-semibold">{day.toLocaleDateString('es-PE', { weekday: 'short', day: 'numeric' })}</h2><div className="grid gap-2 p-2">{entries.map(trip => <article className="rounded-lg border-l-[3px] border-primary-600 bg-primary-50 p-2 text-xs" key={trip.tripId}><b>{new Date(trip.departureAtUtc).toLocaleTimeString('es-PE', { hour: '2-digit', minute: '2-digit' })}</b><span className="mt-1 block">{trip.destination}</span><span className="block truncate text-neutral-500">{trip.vehicleDisplayName}</span></article>)}</div></section>;
  })}</div>;
}
