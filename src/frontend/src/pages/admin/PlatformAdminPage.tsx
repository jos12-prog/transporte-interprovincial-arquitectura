import { Fragment, useCallback, useEffect, useMemo, useState, type FormEvent } from 'react';
import { Plus } from 'lucide-react';
import { useSearchParams } from 'react-router-dom';
import { AdminDrawer, ConfirmDialog, RowActions, StatusBadge as RowStatusBadge, Toast, type ConfirmRequest } from '@/components/admin/RowActions';
import { SeatLayoutView, sortAndRenumberSeats, type SeatCellTool, type SeatLayoutCell } from '@/components/seats/SeatLayoutView';
import { useAuth } from '@/auth/AuthContext';
import {
  getPlatformAgencies,
  getPlatformCompanies,
  getVehicleModels,
  savePlatformAgency,
  savePlatformCompany,
  saveVehicleModel,
  setAgencyActive,
  setCompanyActive,
  setVehicleModelActive,
  type AgencyAdmin,
  type CompanyAdmin,
  type SeatPositionAdmin,
  type VehicleModelAdmin,
} from '@/api/platform-admin';

type Section = 'companies' | 'agencies' | 'models';

function parseSection(value: string | null): Section {
  return value === 'agencies' || value === 'models' ? value : 'companies';
}

const inputClass = 'mt-1 min-h-11 w-full rounded-button border border-neutral-200 bg-white px-3 text-sm text-neutral-900 outline-none focus-visible:border-primary-600 focus-visible:ring-2 focus-visible:ring-primary-100';
const buttonClass = 'inline-flex min-h-10 items-center justify-center gap-2 rounded-button px-4 text-sm font-semibold transition-colors duration-150 focus-visible:outline focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-primary-700';

export function PlatformAdminPage() {
  const { session } = useAuth();
  const [searchParams, setSearchParams] = useSearchParams();
  const querySection = searchParams.get('section');
  const [section, setCurrentSection] = useState<Section>(() => parseSection(querySection));
  const [companies, setCompanies] = useState<CompanyAdmin[]>([]);
  const [agencies, setAgencies] = useState<AgencyAdmin[]>([]);
  const [models, setModels] = useState<VehicleModelAdmin[]>([]);
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState('');
  const [toast, setToast] = useState<{ message: string; tone: 'success' | 'error' } | null>(null);
  const [confirmation, setConfirmation] = useState<ConfirmRequest & { pending?: boolean; error?: string } | null>(null);
  const [drawerKind, setDrawerKind] = useState<Section | null>(null);
  const [companyEdit, setCompanyEdit] = useState<CompanyAdmin | null>(null);
  const [agencyEdit, setAgencyEdit] = useState<AgencyAdmin | null>(null);
  const [modelEdit, setModelEdit] = useState<VehicleModelAdmin | null>(null);
  const [selectedCompanyId, setSelectedCompanyId] = useState('');
  const [seatLayout, setSeatLayout] = useState<SeatPositionAdmin[]>([]);
  const [seatTool, setSeatTool] = useState<SeatCellTool>('seat');
  const [selectedFloor, setSelectedFloor] = useState(1);
  const [editorRows, setEditorRows] = useState(5);
  const [editorColumns, setEditorColumns] = useState(4);
  const token = session?.accessToken ?? '';
  const setSection = useCallback((next: Section) => {
    setCurrentSection(next);
    setSearchParams({ section: next }, { replace: true });
  }, [setSearchParams]);

  useEffect(() => { setCurrentSection(parseSection(querySection)); }, [querySection]);

  const load = useCallback(async () => {
    if (!token) return;
    setLoading(true);
    setError('');
    try {
      const [companyRows, modelRows] = await Promise.all([getPlatformCompanies(token), getVehicleModels(token)]);
      const agencyGroups = await Promise.all(companyRows.map(company => getPlatformAgencies(token, company.companyId)));
      setCompanies(companyRows);
      setAgencies(agencyGroups.flat());
      setModels(modelRows);
      setSelectedCompanyId(current => current || companyRows[0]?.companyId || '');
    } catch (cause) {
      setError(cause instanceof Error ? cause.message : 'No se pudo cargar el catálogo.');
    } finally {
      setLoading(false);
    }
  }, [token]);

  useEffect(() => { void load(); }, [load]);

  const sectionTitle = useMemo(() => ({
    companies: 'Empresas', agencies: 'Agencias', models: 'Modelos de vehículo',
  }[section]), [section]);

  function startCompanyEdit(company: CompanyAdmin) {
    setCompanyEdit(company);
    setDrawerKind('companies');
    setSection('companies');
  }

  function startAgencyEdit(agency: AgencyAdmin) {
    setAgencyEdit(agency);
    setDrawerKind('agencies');
    setSelectedCompanyId(agency.companyId);
    setSection('agencies');
  }

  function startModelEdit(model: VehicleModelAdmin) {
    setModelEdit(model);
    setDrawerKind('models');
    setSeatLayout(model.seats);
    setSelectedFloor(1);
    setSeatTool('seat');
    setSection('models');
  }

  function applySeatTool(cell: SeatLayoutCell) {
    if (modelEdit?.isInUse) return;
    if (cell.kind === 'driver' || cell.kind === 'stairs' || cell.kind === 'aisle') return;
    if (seatTool === 'driver') return;
    setSeatLayout(current => {
      let next = current;
      if (seatTool === 'seat') {
        if (cell.seat) return current;
        next = [...current, { number: 0, floor: cell.floor, row: cell.row, column: cell.column }];
      } else if (seatTool === 'empty') {
        if (!cell.seat) return current;
        next = current.filter(seat => seat.number !== cell.seat?.number);
      } else if (seatTool === 'aisle') {
        if (cell.seat) return current;
        next = current.map(seat => seat.floor === cell.floor && seat.column !== null && seat.column >= cell.column
          ? { ...seat, column: seat.column + 1 }
          : seat);
      }
      return sortAndRenumberSeats(next).map(({ number, floor, row, column }) => ({ number, floor, row, column }));
    });
  }

  function removeEditorRow() {
    const nextRows = Math.max(1, editorRows - 1);
    setEditorRows(nextRows);
    setSeatLayout(current => sortAndRenumberSeats(current.filter(seat => seat.row !== null && seat.row <= nextRows))
      .map(({ number, floor, row, column }) => ({ number, floor, row, column })));
  }

  function removeEditorColumn() {
    const nextColumns = Math.max(1, editorColumns - 1);
    setEditorColumns(nextColumns);
    setSeatLayout(current => sortAndRenumberSeats(current.filter(seat => seat.column !== null && seat.column <= nextColumns))
      .map(({ number, floor, row, column }) => ({ number, floor, row, column })));
  }

  function toggleCompany(company: CompanyAdmin) {
    if (company.isActive) {
      setConfirmation({ title: `¿Desactivar empresa ${company.name}?`, consequence: 'Sus viajes dejarán de aparecer en la búsqueda. Las reservas pagadas se mantienen.', confirmLabel: 'Desactivar', operation: async () => { await setCompanyActive(token, company.companyId, false); await load(); }, pending: false });
    } else void perform(async () => { await setCompanyActive(token, company.companyId, true); await load(); }, 'Empresa activada.');
  }

  function toggleAgency(agency: AgencyAdmin) {
    const action = async () => { await setAgencyActive(token, agency.agencyId, !agency.isActive); await load(); };
    if (agency.isActive) setConfirmation({ title: `¿Desactivar agencia ${agency.name}?`, consequence: 'Sus viajes dejarán de aparecer en la búsqueda. Los viajes ya programados se conservan.', confirmLabel: 'Desactivar', operation: action, pending: false });
    else void perform(action, 'Agencia activada.');
  }

  function toggleModel(model: VehicleModelAdmin) {
    const action = async () => { await setVehicleModelActive(token, model.vehicleModelId, !model.isActive); await load(); };
    if (model.isActive) setConfirmation({ title: `¿Desactivar modelo ${model.name}?`, consequence: 'No se ofrecerá para vehículos nuevos. Los vehículos y viajes existentes mantienen su plano.', confirmLabel: 'Desactivar', operation: action, pending: false });
    else void perform(action, 'Modelo activado.');
  }

  async function perform(action: () => Promise<void>, successMessage = 'Cambios guardados.') {
    setSaving(true);
    setError('');
    try { await action(); setToast({ message: successMessage, tone: 'success' }); }
    catch (cause) { setToast({ message: cause instanceof Error ? cause.message : 'No se pudo guardar el cambio.', tone: 'error' }); }
    finally { setSaving(false); }
  }

  async function confirmAction() {
    if (!confirmation) return;
    setConfirmation(current => current ? { ...current, pending: true, error: undefined } : null);
    try { await confirmation.operation(); setConfirmation(null); setToast({ message: 'Cambios guardados.', tone: 'success' }); }
    catch (cause) { setConfirmation(current => current ? { ...current, pending: false, error: cause instanceof Error ? cause.message : 'No se pudo completar la operación.' } : null); }
  }

  async function submitCompany(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    const data = new FormData(event.currentTarget);
    await perform(async () => {
      await savePlatformCompany(token, {
        name: String(data.get('name') ?? '').trim(),
        taxId: String(data.get('taxId') ?? '').trim() || null,
        contactName: String(data.get('contactName') ?? '').trim() || null,
        contactPhone: String(data.get('contactPhone') ?? '').trim() || null,
      }, companyEdit?.companyId);
      setCompanyEdit(null); setDrawerKind(null);
      await load();
    }, companyEdit ? 'Empresa actualizada.' : 'Empresa creada.');
  }

  async function submitAgency(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    const data = new FormData(event.currentTarget);
    const companyId = agencyEdit?.companyId ?? String(data.get('companyId') ?? '');
    await perform(async () => {
      await savePlatformAgency(token, companyId, {
        name: String(data.get('name') ?? '').trim(),
        city: String(data.get('city') ?? '').trim(),
        address: String(data.get('address') ?? '').trim() || null,
        phone: String(data.get('phone') ?? '').trim() || null,
      }, agencyEdit?.agencyId);
      setAgencyEdit(null); setDrawerKind(null);
      setSelectedCompanyId(companyId);
      await load();
    }, agencyEdit ? 'Agencia actualizada.' : 'Agencia creada.');
  }

  async function submitModel(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    const data = new FormData(event.currentTarget);
    if (!seatLayout.length) { setError('Agrega al menos un asiento al plano.'); return; }
    await perform(async () => {
      await saveVehicleModel(token, {
        name: String(data.get('name') ?? '').trim(),
        category: String(data.get('category') ?? '').trim() || null,
        seats: seatLayout,
      }, modelEdit?.vehicleModelId);
      setModelEdit(null); setDrawerKind(null);
      setSeatLayout([]);
      await load();
    }, modelEdit ? 'Modelo actualizado.' : 'Modelo creado.');
  }

  function openCreateDrawer() {
    setCompanyEdit(null); setAgencyEdit(null); setModelEdit(null); setSeatLayout([]); setDrawerKind(section);
    if (section === 'models') { setSelectedFloor(1); setEditorRows(5); setEditorColumns(4); }
  }

  function closeDrawer() {
    setDrawerKind(null); setCompanyEdit(null); setAgencyEdit(null); setModelEdit(null); setSeatLayout([]);
    setSelectedFloor(1); setEditorRows(5); setEditorColumns(4);
  }

  return (
    <section className="space-y-6" aria-labelledby="platform-admin-title">
      <div className="flex flex-wrap items-end justify-between gap-4">
        <div>
          <p className="crumbs mb-1.5 text-[13px] text-neutral-500">{section === 'models' ? 'Catálogos' : 'Plataforma'}</p>
          <h1 id="platform-admin-title" className="font-display text-3xl font-semibold text-neutral-900">{section === 'companies' ? 'Empresas de transporte' : sectionTitle}</h1>
          <p className="mt-2 max-w-2xl text-sm text-neutral-600">{section === 'companies' ? 'Desactivar una empresa oculta sus viajes de la búsqueda; las reservas pagadas se mantienen.' : section === 'agencies' ? 'Agrupadas por empresa. Cada agencia pertenece a una sola empresa.' : 'Cada modelo define el plano de asientos que heredan sus vehículos. Lo usan todas las empresas.'}</p>
        </div>
        <button type="button" onClick={openCreateDrawer} className={`${buttonClass} min-h-11 bg-primary-700 text-white hover:bg-primary-800`}>
          <Plus aria-hidden="true" size={16} /> {section === 'companies' ? 'Registrar empresa' : section === 'agencies' ? 'Nueva agencia' : 'Nuevo modelo'}
        </button>
      </div>

      {error ? <p role="alert" className="rounded-card border border-danger-200 bg-danger-50 p-4 text-sm text-danger-700">{error}</p> : null}
      {loading ? <div role="status" className="grid gap-4 md:grid-cols-2"><div className="h-32 animate-pulse rounded-card bg-neutral-100" /><div className="h-32 animate-pulse rounded-card bg-neutral-100" /></div> : null}
      {!loading && section === 'companies' ? <CompaniesSection companies={companies} agencies={agencies} edit={companyEdit} open={drawerKind === 'companies'} saving={saving} onEdit={startCompanyEdit} onToggle={toggleCompany} onSubmit={submitCompany} onCancel={closeDrawer} /> : null}
      {!loading && section === 'agencies' ? <AgenciesSection companies={companies} agencies={agencies} selectedCompanyId={selectedCompanyId} edit={agencyEdit} open={drawerKind === 'agencies'} saving={saving} onEdit={startAgencyEdit} onToggle={toggleAgency} onSubmit={submitAgency} onCancel={closeDrawer} onSelectCompany={setSelectedCompanyId} /> : null}
      {!loading && section === 'models' ? <ModelsSection models={models} edit={modelEdit} open={drawerKind === 'models'} seats={seatLayout} saving={saving} seatTool={seatTool} selectedFloor={selectedFloor} rows={modelEdit ? undefined : editorRows} columns={modelEdit ? undefined : editorColumns} onAddRow={() => setEditorRows(rows => rows + 1)} onRemoveRow={removeEditorRow} onAddColumn={() => setEditorColumns(columns => columns + 1)} onRemoveColumn={removeEditorColumn} onSelectTool={setSeatTool} onSelectFloor={setSelectedFloor} onEdit={startModelEdit} onToggle={toggleModel} onCellAction={applySeatTool} onSubmit={submitModel} onCancel={closeDrawer} /> : null}
      {confirmation ? <ConfirmDialog open title={confirmation.title} consequence={confirmation.consequence} confirmLabel={confirmation.confirmLabel} pending={confirmation.pending} error={confirmation.error} onConfirm={() => void confirmAction()} onCancel={() => setConfirmation(null)} /> : null}
      {toast ? <Toast message={toast.message} tone={toast.tone} onClose={() => setToast(null)} /> : null}
    </section>
  );
}

function CompaniesSection({ companies, agencies, edit, open, saving, onEdit, onToggle, onSubmit, onCancel }: {
  companies: CompanyAdmin[]; agencies: AgencyAdmin[]; edit: CompanyAdmin | null; open: boolean; saving: boolean; onEdit: (company: CompanyAdmin) => void;
  onToggle: (company: CompanyAdmin) => void; onSubmit: (event: FormEvent<HTMLFormElement>) => void; onCancel: () => void;
}) {
  return <>
    <div className="overflow-hidden rounded-2xl border border-neutral-200 bg-white shadow-sm"><div className="overflow-x-auto"><table className="w-full border-collapse text-sm"><thead className="bg-neutral-50 text-left text-xs uppercase tracking-wide text-neutral-500"><tr><th className="px-4 py-3">Empresa</th><th className="px-4 py-3">RUC</th><th className="px-4 py-3">Agencias</th><th className="px-4 py-3">Estado</th><th className="px-4 py-3 text-right">Acción</th></tr></thead><tbody>{companies.map(company => <tr className="border-t border-neutral-100 even:bg-[#FCFDFE]" key={company.companyId}><td className="h-14 px-4"><div className="flex items-center gap-3"><span aria-hidden="true" className="grid h-9 w-9 shrink-0 place-items-center rounded-[10px] bg-primary-700 text-xs font-bold text-white">{initials(company.name)}</span><b>{company.name}</b></div></td><td className="h-14 px-4 tabular-nums">{company.taxId ?? '—'}</td><td className="h-14 px-4 tabular-nums">{agencies.filter(agency => agency.companyId === company.companyId).length}</td><td className="h-14 px-4"><div className="flex items-center gap-2.5"><input aria-label={`${company.isActive ? 'Desactivar' : 'Activar'} empresa ${company.name}`} checked={company.isActive} className="h-[22px] w-[38px] cursor-pointer appearance-none rounded-full bg-neutral-300 transition-colors checked:bg-primary-700 before:block before:ml-[3px] before:mt-[3px] before:h-4 before:w-4 before:rounded-full before:bg-white before:transition-transform checked:before:translate-x-4" onChange={() => onToggle(company)} type="checkbox" /><RowStatusBadge status={company.isActive ? 'active' : 'inactive'} /></div></td><td className="h-14 px-4 text-right"><RowActions name={`empresa ${company.name}`} disabled={saving} actions={[{ id: 'edit', label: 'Editar', onSelect: () => onEdit(company) }]} /></td></tr>)}</tbody></table></div>{companies.length === 0 ? <EmptyState text="Aún no hay empresas registradas." /> : null}</div>
    {open ? <AdminDrawer onClose={onCancel} title={edit ? 'Editar empresa' : 'Registrar empresa'}><form key={edit?.companyId ?? 'new-company'} onSubmit={onSubmit} className="flex min-h-0 flex-1 flex-col"><div className="drawer-body"><Field label="Razón social" name="name" defaultValue={edit?.name ?? ''} required /><Field label="RUC" name="taxId" defaultValue={edit?.taxId ?? ''} inputMode="numeric" maxLength={11} /><p className="-mt-3 text-xs text-neutral-500">Se valida el formato; no se consulta SUNAT.</p><Field label="Persona de contacto" name="contactName" defaultValue={edit?.contactName ?? ''} /><Field label="Teléfono de contacto" name="contactPhone" defaultValue={edit?.contactPhone ?? ''} /></div><div className="drawer-foot"><button className={`${buttonClass} border border-neutral-200 bg-white text-neutral-700`} onClick={onCancel} type="button">Cancelar</button><button disabled={saving} className={`${buttonClass} bg-primary-700 text-white hover:bg-primary-800 disabled:opacity-60`}><Plus size={16} />{saving ? 'Guardando…' : edit ? 'Guardar cambios' : 'Registrar empresa'}</button></div></form></AdminDrawer> : null}
  </>;
}

function initials(value: string) { return value.split(/\s+/).filter(Boolean).slice(0, 2).map(part => part[0]?.toLocaleUpperCase('es-PE')).join(''); }

function AgenciesSection({ companies, agencies, selectedCompanyId, edit, open, saving, onEdit, onToggle, onSubmit, onCancel, onSelectCompany }: {
  companies: CompanyAdmin[]; agencies: AgencyAdmin[]; selectedCompanyId: string; edit: AgencyAdmin | null; open: boolean; saving: boolean;
  onEdit: (agency: AgencyAdmin) => void; onToggle: (agency: AgencyAdmin) => void; onSubmit: (event: FormEvent<HTMLFormElement>) => void; onCancel: () => void; onSelectCompany: (id: string) => void;
}) {
  return <>
    <div className="overflow-hidden rounded-2xl border border-neutral-200 bg-white shadow-sm"><div className="overflow-x-auto"><table className="w-full border-collapse text-sm"><thead className="bg-neutral-50 text-left text-xs uppercase tracking-wide text-neutral-500"><tr><th className="px-4 py-3">Agencia</th><th className="px-4 py-3">Dirección</th><th className="px-4 py-3">Ciudad</th><th className="px-4 py-3">Estado</th><th className="px-4 py-3 text-right">Acción</th></tr></thead><tbody>{companies.map(company => <Fragment key={company.companyId}><tr className="bg-neutral-50"><td className="h-10 px-4 text-sm text-neutral-700" colSpan={5}><b>{company.name}</b></td></tr>{agencies.filter(agency => agency.companyId === company.companyId).map(agency => <tr className="border-t border-neutral-100 even:bg-[#FCFDFE]" key={agency.agencyId}><td className="h-14 px-4 pl-8 font-semibold">{agency.name}</td><td className="h-14 px-4 text-neutral-600">{agency.address ?? '—'}</td><td className="h-14 px-4">{agency.city}</td><td className="h-14 px-4"><RowStatusBadge status={agency.isActive ? 'active' : 'inactive'} /></td><td className="h-14 px-4 text-right"><RowActions name={`agencia ${agency.name}`} disabled={saving} actions={[{ id: 'edit', label: 'Editar', onSelect: () => onEdit(agency) }, ...(agency.isActive ? [{ id: 'deactivate' as const, label: 'Desactivar', onSelect: () => onToggle(agency) }] : [{ id: 'activate' as const, label: 'Activar', onSelect: () => onToggle(agency) }])]} /></td></tr>)}</Fragment>)}</tbody></table></div>{agencies.length === 0 ? <EmptyState text="Aún no hay agencias registradas." /> : null}</div>
    {open ? <AdminDrawer onClose={onCancel} title={edit ? 'Editar agencia' : 'Nueva agencia'}><form key={edit?.agencyId ?? `new-agency-${selectedCompanyId}`} onSubmit={onSubmit} className="flex min-h-0 flex-1 flex-col"><div className="drawer-body"><label className="block text-sm font-medium text-neutral-700">Empresa<select className={inputClass} name="companyId" value={edit?.companyId ?? selectedCompanyId} onChange={event => onSelectCompany(event.target.value)} disabled={!!edit} required>{companies.map(company => <option key={company.companyId} value={company.companyId} disabled={!company.isActive}>{company.name}{company.isActive ? '' : ' · inactiva'}</option>)}</select></label><Field label="Nombre de la agencia" name="name" defaultValue={edit?.name ?? ''} required /><Field label="Dirección" name="address" defaultValue={edit?.address ?? ''} /><Field label="Ciudad" name="city" defaultValue={edit?.city ?? ''} required /><Field label="Teléfono" name="phone" defaultValue={edit?.phone ?? ''} /></div><div className="drawer-foot"><button className={`${buttonClass} border border-neutral-200 bg-white text-neutral-700`} onClick={onCancel} type="button">Cancelar</button><button disabled={saving || companies.every(company => !company.isActive)} className={`${buttonClass} bg-primary-700 text-white hover:bg-primary-800 disabled:opacity-60`}><Plus size={16} />{saving ? 'Guardando…' : edit ? 'Guardar cambios' : 'Crear agencia'}</button></div></form></AdminDrawer> : null}
  </>;
}

function ModelsSection({ models, edit, open, seats, saving, seatTool, selectedFloor, rows, columns, onAddRow, onRemoveRow, onAddColumn, onRemoveColumn, onSelectTool, onSelectFloor, onEdit, onToggle, onCellAction, onSubmit, onCancel }: {
  models: VehicleModelAdmin[]; edit: VehicleModelAdmin | null; open: boolean; seats: SeatPositionAdmin[]; saving: boolean; seatTool: SeatCellTool; selectedFloor: number;
  rows?: number; columns?: number; onAddRow: () => void; onRemoveRow: () => void; onAddColumn: () => void; onRemoveColumn: () => void;
  onSelectTool: (tool: SeatCellTool) => void; onSelectFloor: (floor: number) => void; onEdit: (model: VehicleModelAdmin) => void;
  onToggle: (model: VehicleModelAdmin) => void; onCellAction: (cell: SeatLayoutCell) => void; onSubmit: (event: FormEvent<HTMLFormElement>) => void; onCancel: () => void;
}) {
  const [draftName, setDraftName] = useState(edit?.name ?? '');
  useEffect(() => { setDraftName(edit?.name ?? ''); }, [edit?.vehicleModelId, edit?.name]);
  const used = !!edit?.isInUse;
  const floorCount = Math.max(1, ...seats.map(seat => seat.floor), /bus\s*2/i.test(draftName) ? 2 : 1);
  const orderedModels = [...models].sort((left, right) => left.capacity - right.capacity || left.name.localeCompare(right.name, 'es'));
  return <>
    <div className="grid gap-4 md:grid-cols-2">{orderedModels.map(model => <article key={model.vehicleModelId} className="flex flex-col rounded-card border border-neutral-200 bg-white p-5 shadow-sm">
      <div className="flex items-start justify-between gap-3"><div><h2 className="font-semibold text-neutral-900">{model.name}</h2><p className="mt-1 text-sm text-neutral-500">{model.category ?? 'Sin categoría'}</p></div><RowStatusBadge status={model.isActive ? 'active' : 'inactive'} /></div>
      <div className="my-4 overflow-x-auto rounded-card border border-neutral-100 bg-neutral-50 p-3"><SeatLayoutView seats={model.seats} modelName={model.name} compact ariaLabel={`Plano de ${model.name}`} /></div>
      <p className="text-sm text-neutral-600">{model.capacity} asientos · {model.isInUse ? 'En uso' : 'Sin vehículos asociados'}</p>
      <div className="mt-auto flex justify-end pt-4"><RowActions name={`modelo ${model.name}`} disabled={saving} actions={[{ id: 'edit', label: 'Editar', onSelect: () => onEdit(model) }, ...(model.isActive ? [{ id: 'deactivate' as const, label: 'Desactivar', onSelect: () => onToggle(model) }] : [{ id: 'activate' as const, label: 'Activar', onSelect: () => onToggle(model) }])]} /></div>
    </article>)}{models.length === 0 ? <div className="md:col-span-2"><EmptyState text="Aún no hay modelos registrados." /></div> : null}</div>
    {open ? <AdminDrawer onClose={onCancel} title={edit ? 'Editar modelo de vehículo' : 'Nuevo modelo de vehículo'}><form key={edit?.vehicleModelId ?? 'new-model'} onSubmit={onSubmit} className="flex min-h-0 flex-1 flex-col">
      <div className="drawer-body">
      <div><p className="text-xs font-semibold uppercase tracking-wide text-primary-700">{edit ? 'Editar modelo' : 'Nuevo modelo'}</p><h2 className="mt-1 text-lg font-semibold text-neutral-900">Plano de asientos</h2></div>
      {edit?.isInUse ? <p role="note" className="rounded-button bg-accent-50 p-3 text-xs text-neutral-700">Este modelo está en uso. Puedes editar su nombre, categoría o estado; el plano y la capacidad están bloqueados.</p> : null}
      <label className="block text-sm font-medium text-neutral-700">Nombre visible<input className={inputClass} name="name" value={draftName} onChange={event => setDraftName(event.target.value)} required /></label>
      <label className="block text-sm font-medium text-neutral-700">Categoría<select className={inputClass} name="category" defaultValue={edit?.category ?? ''}><option value="">Seleccionar</option><option>Sedán</option><option>Familiar</option><option>Transporte</option><option>Bus</option></select></label>
      <div>
        <div className="mb-2 flex items-center justify-between"><span className="text-sm font-medium text-neutral-700">Asientos: {seats.length}</span><span className="text-xs text-neutral-500">Se numeran de adelante hacia atrás</span></div>
        {!edit ? <div className="mb-3 flex flex-wrap gap-2" role="group" aria-label="Dimensiones del plano">
          <button type="button" onClick={onAddColumn} className={`${buttonClass} border border-neutral-200 bg-white text-neutral-700`}>Agregar columna</button>
          <button type="button" onClick={onRemoveColumn} disabled={(columns ?? 1) <= 1} className={`${buttonClass} border border-neutral-200 bg-white text-neutral-700 disabled:opacity-50`}>Quitar columna</button>
          <button type="button" onClick={onAddRow} className={`${buttonClass} border border-neutral-200 bg-white text-neutral-700`}>Agregar fila</button>
          <button type="button" onClick={onRemoveRow} disabled={(rows ?? 1) <= 1} className={`${buttonClass} border border-neutral-200 bg-white text-neutral-700 disabled:opacity-50`}>Quitar fila</button>
        </div> : null}
        <div className="flex flex-wrap gap-2" role="group" aria-label="Paleta del plano">{([['seat', 'Asiento'], ['aisle', 'Pasillo'], ['driver', 'Conductor'], ['empty', 'Vacío']] as const).map(([tool, label]) => <button key={tool} type="button" aria-pressed={seatTool === tool} onClick={() => onSelectTool(tool)} className={`${buttonClass} border ${seatTool === tool ? 'border-primary-700 bg-primary-100 text-primary-800' : 'border-neutral-200 bg-white text-neutral-700'}`}>{label}</button>)}</div>
        {floorCount > 1 ? <div className="mt-3 flex gap-2" role="group" aria-label="Pisos del vehículo">{Array.from({ length: floorCount }, (_, index) => index + 1).map(floor => <button key={floor} type="button" aria-pressed={selectedFloor === floor} onClick={() => onSelectFloor(floor)} className={`${buttonClass} border ${selectedFloor === floor ? 'border-primary-700 bg-primary-100 text-primary-800' : 'border-neutral-200 bg-white text-neutral-700'}`}>Piso {floor}</button>)}</div> : null}
        <p className="mt-2 text-xs text-neutral-500">Clic en una celda para cambiarla. El conductor queda fijo adelante; las columnas vacías forman el pasillo.</p>
        {seatTool === 'driver' ? <p role="status" className="mt-1 text-xs text-neutral-600">El conductor se deriva del plano y queda fijo en la parte delantera.</p> : null}
        <div className="mt-3 overflow-x-auto rounded-card border border-neutral-200 bg-neutral-50 p-3" role="group" aria-label="Editor visual del plano de asientos">
          <SeatLayoutView seats={seats} modelName={edit?.name || draftName} mode="edit" floor={selectedFloor} floorCount={floorCount} rows={edit ? undefined : rows} columns={edit ? undefined : columns} disabled={used} onCellAction={onCellAction} ariaLabel="Editor visual del plano de asientos" />
        </div>
      </div>
      </div>
      <div className="drawer-foot"><button className={`${buttonClass} border border-neutral-200 bg-white text-neutral-700`} onClick={onCancel} type="button">Cancelar</button><button disabled={saving || !seats.length && !used} className={`${buttonClass} bg-primary-700 text-white hover:bg-primary-800 disabled:opacity-60`}><Plus size={16} />{saving ? 'Guardando…' : edit ? 'Guardar cambios' : 'Crear modelo'}</button></div>
    </form></AdminDrawer> : null}
  </>;
}

function Field({ label, name, defaultValue, required = false, inputMode, maxLength }: { label: string; name: string; defaultValue: string; required?: boolean; inputMode?: 'numeric' | 'text'; maxLength?: number }) {
  return <label className="block text-sm font-medium text-neutral-700">{label}<input className={inputClass} name={name} defaultValue={defaultValue} required={required} inputMode={inputMode} maxLength={maxLength} /></label>;
}

function EmptyState({ text }: { text: string }) {
  return <div className="px-5 py-10 text-center"><p className="font-medium text-neutral-700">{text}</p><p className="mt-1 text-sm text-neutral-500">Usa el formulario para agregar un registro.</p></div>;
}
