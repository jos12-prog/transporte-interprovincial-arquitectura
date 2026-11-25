import { useCallback, useEffect, useMemo, useState, type FormEvent, type ReactNode } from 'react';
import { Building2, Bus, MapPin, Plus, RefreshCw, Store } from 'lucide-react';
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

const inputClass = 'mt-1 min-h-11 w-full rounded-button border border-neutral-200 bg-white px-3 text-sm text-neutral-900 outline-none focus-visible:border-primary-600 focus-visible:ring-2 focus-visible:ring-primary-100';
const buttonClass = 'inline-flex min-h-10 items-center justify-center gap-2 rounded-button px-4 text-sm font-semibold transition-colors duration-150 focus-visible:outline focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-primary-700';

export function PlatformAdminPage() {
  const { session } = useAuth();
  const [section, setSection] = useState<Section>('companies');
  const [companies, setCompanies] = useState<CompanyAdmin[]>([]);
  const [agencies, setAgencies] = useState<AgencyAdmin[]>([]);
  const [models, setModels] = useState<VehicleModelAdmin[]>([]);
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState('');
  const [notice, setNotice] = useState('');
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
    setSection('companies');
  }

  function startAgencyEdit(agency: AgencyAdmin) {
    setAgencyEdit(agency);
    setSelectedCompanyId(agency.companyId);
    setSection('agencies');
  }

  function startModelEdit(model: VehicleModelAdmin) {
    setModelEdit(model);
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

  async function toggleCompany(company: CompanyAdmin) {
    if (company.isActive && !window.confirm(`¿Desactivar ${company.name}? Sus viajes dejarán de aparecer en la búsqueda.`)) return;
    await perform(async () => { await setCompanyActive(token, company.companyId, !company.isActive); await load(); });
  }

  async function toggleAgency(agency: AgencyAdmin) {
    if (agency.isActive && !window.confirm(`¿Desactivar ${agency.name}? Sus viajes dejarán de aparecer en la búsqueda.`)) return;
    await perform(async () => { await setAgencyActive(token, agency.agencyId, !agency.isActive); await load(); });
  }

  async function toggleModel(model: VehicleModelAdmin) {
    if (model.isActive && !window.confirm(`¿Desactivar ${model.name}?`)) return;
    await perform(async () => { await setVehicleModelActive(token, model.vehicleModelId, !model.isActive); await load(); });
  }

  async function perform(action: () => Promise<void>) {
    setSaving(true);
    setError('');
    setNotice('');
    try { await action(); }
    catch (cause) { setError(cause instanceof Error ? cause.message : 'No se pudo guardar el cambio.'); }
    finally { setSaving(false); }
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
      setCompanyEdit(null);
      await load();
      setNotice(companyEdit ? 'Empresa actualizada.' : 'Empresa creada.');
    });
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
      setAgencyEdit(null);
      setSelectedCompanyId(companyId);
      await load();
      setNotice(agencyEdit ? 'Agencia actualizada.' : 'Agencia creada.');
    });
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
      setModelEdit(null);
      setSeatLayout([]);
      await load();
      setNotice(modelEdit ? 'Modelo actualizado.' : 'Modelo creado.');
    });
  }

  return (
    <section className="space-y-6" aria-labelledby="platform-admin-title">
      <div className="flex flex-wrap items-end justify-between gap-4">
        <div>
          <p className="text-xs font-semibold uppercase tracking-wide text-primary-700">Plataforma · Superadministración</p>
          <h1 id="platform-admin-title" className="mt-1 font-display text-3xl font-bold text-neutral-900">{sectionTitle}</h1>
          <p className="mt-2 max-w-2xl text-sm text-neutral-600">Administra los operadores y el catálogo global que comparten sus agencias.</p>
        </div>
        <button type="button" onClick={() => void load()} disabled={loading} className={`${buttonClass} border border-neutral-200 bg-white text-neutral-700 hover:bg-neutral-50`}>
          <RefreshCw aria-hidden="true" size={16} /> Actualizar
        </button>
      </div>

      <nav aria-label="Catálogos de plataforma" className="flex flex-wrap gap-2 border-b border-neutral-200 pb-3">
        <SectionButton active={section === 'companies'} onClick={() => setSection('companies')} icon={<Building2 size={17} />} label="Empresas" />
        <SectionButton active={section === 'agencies'} onClick={() => setSection('agencies')} icon={<Store size={17} />} label="Agencias" />
        <SectionButton active={section === 'models'} onClick={() => setSection('models')} icon={<Bus size={17} />} label="Modelos de vehículo" />
      </nav>

      {error ? <p role="alert" className="rounded-card border border-danger-200 bg-danger-50 p-4 text-sm text-danger-700">{error}</p> : null}
      {notice ? <p role="status" className="rounded-card border border-primary-200 bg-primary-50 p-4 text-sm text-primary-800">{notice}</p> : null}
      {loading ? <div role="status" className="grid gap-4 md:grid-cols-2"><div className="h-32 animate-pulse rounded-card bg-neutral-100" /><div className="h-32 animate-pulse rounded-card bg-neutral-100" /></div> : null}
      {!loading && section === 'companies' ? <CompaniesSection key={companyEdit?.companyId ?? 'new-company'} companies={companies} edit={companyEdit} saving={saving} onEdit={startCompanyEdit} onToggle={toggleCompany} onSubmit={submitCompany} onCancel={() => setCompanyEdit(null)} /> : null}
      {!loading && section === 'agencies' ? <AgenciesSection key={agencyEdit?.agencyId ?? `new-agency-${selectedCompanyId}`} companies={companies} agencies={agencies} selectedCompanyId={selectedCompanyId} edit={agencyEdit} saving={saving} onEdit={startAgencyEdit} onToggle={toggleAgency} onSubmit={submitAgency} onCancel={() => setAgencyEdit(null)} onSelectCompany={setSelectedCompanyId} /> : null}
      {!loading && section === 'models' ? <ModelsSection key={modelEdit?.vehicleModelId ?? 'new-model'} models={models} edit={modelEdit} seats={seatLayout} saving={saving} seatTool={seatTool} selectedFloor={selectedFloor} rows={modelEdit ? undefined : editorRows} columns={modelEdit ? undefined : editorColumns} onAddRow={() => setEditorRows(rows => rows + 1)} onRemoveRow={removeEditorRow} onAddColumn={() => setEditorColumns(columns => columns + 1)} onRemoveColumn={removeEditorColumn} onSelectTool={setSeatTool} onSelectFloor={setSelectedFloor} onEdit={startModelEdit} onToggle={toggleModel} onCellAction={applySeatTool} onSubmit={submitModel} onCancel={() => { setModelEdit(null); setSeatLayout([]); setSelectedFloor(1); setEditorRows(5); setEditorColumns(4); }} /> : null}
    </section>
  );
}

function SectionButton({ active, onClick, icon, label }: { active: boolean; onClick: () => void; icon: ReactNode; label: string }) {
  return <button type="button" aria-current={active ? 'page' : undefined} onClick={onClick} className={`${buttonClass} ${active ? 'bg-primary-100 text-primary-800' : 'bg-white text-neutral-600 hover:bg-neutral-50'}`}>{icon}{label}</button>;
}

function CompaniesSection({ companies, edit, saving, onEdit, onToggle, onSubmit, onCancel }: {
  companies: CompanyAdmin[]; edit: CompanyAdmin | null; saving: boolean; onEdit: (company: CompanyAdmin) => void;
  onToggle: (company: CompanyAdmin) => void; onSubmit: (event: FormEvent<HTMLFormElement>) => void; onCancel: () => void;
}) {
  return <div className="grid gap-6 xl:grid-cols-[minmax(0,1.5fr)_minmax(18rem,0.8fr)]">
    <div className="overflow-hidden rounded-card border border-neutral-200 bg-white shadow-sm">
      <div className="border-b border-neutral-100 px-5 py-4"><h2 className="font-semibold text-neutral-900">Operadores de transporte</h2><p className="mt-1 text-xs text-neutral-500">El estado controla la visibilidad pública de sus viajes.</p></div>
      {companies.length ? <div className="divide-y divide-neutral-100">{companies.map(company => <article key={company.companyId} className="flex flex-wrap items-center justify-between gap-4 px-5 py-4">
        <div><h3 className="font-semibold text-neutral-900">{company.name}</h3><p className="mt-1 text-xs text-neutral-500">{company.taxId ? `RUC ${company.taxId}` : 'RUC no registrado'}{company.contactName ? ` · ${company.contactName}` : ''}</p></div>
        <div className="flex items-center gap-3"><StatusBadge active={company.isActive} /><button type="button" onClick={() => onEdit(company)} className="text-sm font-semibold text-primary-700 hover:underline">Editar</button><button type="button" onClick={() => void onToggle(company)} className={`${buttonClass} border border-neutral-200 bg-white text-neutral-700 hover:bg-neutral-50`}>{company.isActive ? 'Desactivar' : 'Activar'}</button></div>
      </article>)}</div> : <EmptyState text="Aún no hay empresas registradas." />}
    </div>
    <form key={edit?.companyId ?? 'new-company'} onSubmit={onSubmit} className="space-y-4 rounded-card border border-neutral-200 bg-white p-5 shadow-sm">
      <div><p className="text-xs font-semibold uppercase tracking-wide text-primary-700">{edit ? 'Editar empresa' : 'Nueva empresa'}</p><h2 className="mt-1 text-lg font-semibold text-neutral-900">Datos del operador</h2></div>
      <Field label="Razón social" name="name" defaultValue={edit?.name ?? ''} required />
      <Field label="RUC" name="taxId" defaultValue={edit?.taxId ?? ''} inputMode="numeric" maxLength={11} />
      <Field label="Persona de contacto" name="contactName" defaultValue={edit?.contactName ?? ''} />
      <Field label="Teléfono de contacto" name="contactPhone" defaultValue={edit?.contactPhone ?? ''} />
      <div className="flex gap-2"><button disabled={saving} className={`${buttonClass} flex-1 bg-primary-700 text-white hover:bg-primary-800 disabled:opacity-60`}><Plus size={16} />{edit ? 'Guardar cambios' : 'Crear empresa'}</button>{edit ? <button type="button" onClick={onCancel} className={`${buttonClass} border border-neutral-200 text-neutral-700`}>Cancelar</button> : null}</div>
      <p className="text-xs text-neutral-500">El RUC se valida por formato; no se consulta SUNAT.</p>
    </form>
  </div>;
}

function AgenciesSection({ companies, agencies, selectedCompanyId, edit, saving, onEdit, onToggle, onSubmit, onCancel, onSelectCompany }: {
  companies: CompanyAdmin[]; agencies: AgencyAdmin[]; selectedCompanyId: string; edit: AgencyAdmin | null; saving: boolean;
  onEdit: (agency: AgencyAdmin) => void; onToggle: (agency: AgencyAdmin) => void; onSubmit: (event: FormEvent<HTMLFormElement>) => void; onCancel: () => void; onSelectCompany: (id: string) => void;
}) {
  return <div className="grid gap-6 xl:grid-cols-[minmax(0,1.5fr)_minmax(18rem,0.8fr)]">
    <div className="space-y-4">{companies.map(company => {
      const rows = agencies.filter(agency => agency.companyId === company.companyId);
      return <section key={company.companyId} className="overflow-hidden rounded-card border border-neutral-200 bg-white shadow-sm"><div className="flex items-center justify-between border-b border-neutral-100 px-5 py-4"><div><h2 className="font-semibold text-neutral-900">{company.name}</h2><p className="text-xs text-neutral-500">Agencias asociadas a esta empresa</p></div><StatusBadge active={company.isActive} /></div>
        {rows.length ? <div className="divide-y divide-neutral-100">{rows.map(agency => <article key={agency.agencyId} className="flex flex-wrap items-center justify-between gap-3 px-5 py-4"><div><h3 className="font-semibold text-neutral-900">{agency.name}</h3><p className="mt-1 flex items-center gap-1 text-xs text-neutral-500"><MapPin size={13} />{agency.address ? `${agency.address} · ` : ''}{agency.city}</p></div><div className="flex items-center gap-3"><StatusBadge active={agency.isActive} /><button type="button" onClick={() => onEdit(agency)} className="text-sm font-semibold text-primary-700 hover:underline">Editar</button><button type="button" onClick={() => void onToggle(agency)} className={`${buttonClass} border border-neutral-200 bg-white text-neutral-700 hover:bg-neutral-50`}>{agency.isActive ? 'Desactivar' : 'Activar'}</button></div></article>)}</div> : <EmptyState text="Esta empresa aún no tiene agencias." />}
      </section>;
    })}</div>
    <form key={edit?.agencyId ?? `new-agency-${selectedCompanyId}`} onSubmit={onSubmit} className="space-y-4 rounded-card border border-neutral-200 bg-white p-5 shadow-sm">
      <div><p className="text-xs font-semibold uppercase tracking-wide text-primary-700">{edit ? 'Editar agencia' : 'Nueva agencia'}</p><h2 className="mt-1 text-lg font-semibold text-neutral-900">Ubicación</h2></div>
      <label className="block text-sm font-medium text-neutral-700">Empresa<select className={inputClass} name="companyId" value={edit?.companyId ?? selectedCompanyId} onChange={event => onSelectCompany(event.target.value)} disabled={!!edit} required>{companies.map(company => <option key={company.companyId} value={company.companyId} disabled={!company.isActive}>{company.name}{company.isActive ? '' : ' · inactiva'}</option>)}</select></label>
      <Field label="Nombre de la agencia" name="name" defaultValue={edit?.name ?? ''} required />
      <Field label="Dirección" name="address" defaultValue={edit?.address ?? ''} />
      <Field label="Ciudad" name="city" defaultValue={edit?.city ?? ''} required />
      <Field label="Teléfono" name="phone" defaultValue={edit?.phone ?? ''} />
      <div className="flex gap-2"><button disabled={saving || companies.every(company => !company.isActive)} className={`${buttonClass} flex-1 bg-primary-700 text-white hover:bg-primary-800 disabled:opacity-60`}><Plus size={16} />{edit ? 'Guardar cambios' : 'Crear agencia'}</button>{edit ? <button type="button" onClick={onCancel} className={`${buttonClass} border border-neutral-200 text-neutral-700`}>Cancelar</button> : null}</div>
    </form>
  </div>;
}

function ModelsSection({ models, edit, seats, saving, seatTool, selectedFloor, rows, columns, onAddRow, onRemoveRow, onAddColumn, onRemoveColumn, onSelectTool, onSelectFloor, onEdit, onToggle, onCellAction, onSubmit, onCancel }: {
  models: VehicleModelAdmin[]; edit: VehicleModelAdmin | null; seats: SeatPositionAdmin[]; saving: boolean; seatTool: SeatCellTool; selectedFloor: number;
  rows?: number; columns?: number; onAddRow: () => void; onRemoveRow: () => void; onAddColumn: () => void; onRemoveColumn: () => void;
  onSelectTool: (tool: SeatCellTool) => void; onSelectFloor: (floor: number) => void; onEdit: (model: VehicleModelAdmin) => void;
  onToggle: (model: VehicleModelAdmin) => void; onCellAction: (cell: SeatLayoutCell) => void; onSubmit: (event: FormEvent<HTMLFormElement>) => void; onCancel: () => void;
}) {
  const [draftName, setDraftName] = useState(edit?.name ?? '');
  useEffect(() => { setDraftName(edit?.name ?? ''); }, [edit?.vehicleModelId, edit?.name]);
  const used = !!edit?.isInUse;
  const floorCount = Math.max(1, ...seats.map(seat => seat.floor), /bus\s*2/i.test(draftName) ? 2 : 1);
  const orderedModels = [...models].sort((left, right) => left.capacity - right.capacity || left.name.localeCompare(right.name, 'es'));
  return <div className="grid gap-6 xl:grid-cols-[minmax(0,1.5fr)_minmax(20rem,0.8fr)]">
    <div className="space-y-4">
      <label className="block max-w-md text-sm font-medium text-neutral-700">Modelo disponible para altas de vehículos<select aria-label="Modelo para alta de vehículo" className={inputClass} defaultValue=""><option value="">Seleccionar modelo activo</option>{models.filter(model => model.isActive).map(model => <option key={model.vehicleModelId} value={model.vehicleModelId}>{model.name}</option>)}</select></label>
      <div className="grid gap-4 md:grid-cols-2">{orderedModels.map(model => <article key={model.vehicleModelId} className="rounded-card border border-neutral-200 bg-white p-5 shadow-sm">
      <div className="flex items-start justify-between gap-3"><div><h2 className="font-semibold text-neutral-900">{model.name}</h2><p className="mt-1 text-sm text-neutral-500">{model.category ?? 'Sin categoría'}</p></div><StatusBadge active={model.isActive} /></div>
      <div className="my-4 overflow-x-auto rounded-card border border-neutral-100 bg-neutral-50 p-3"><SeatLayoutView seats={model.seats} modelName={model.name} compact ariaLabel={`Plano de ${model.name}`} /></div>
      <p className="text-sm text-neutral-600">{model.capacity} asientos · {model.isInUse ? 'En uso' : 'Sin vehículos asociados'}</p>
      <div className="mt-4 flex justify-end gap-3"><button type="button" onClick={() => onEdit(model)} className="text-sm font-semibold text-primary-700 hover:underline">Editar</button><button type="button" onClick={() => void onToggle(model)} className={`${buttonClass} border border-neutral-200 text-neutral-700 hover:bg-neutral-50`}>{model.isActive ? 'Desactivar' : 'Activar'}</button></div>
    </article>)}{models.length === 0 ? <div className="md:col-span-2"><EmptyState text="Aún no hay modelos registrados." /></div> : null}</div>
    </div>
    <form key={edit?.vehicleModelId ?? 'new-model'} onSubmit={onSubmit} className="space-y-4 rounded-card border border-neutral-200 bg-white p-5 shadow-sm">
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
      <div className="flex gap-2"><button disabled={saving || !seats.length && !used} className={`${buttonClass} flex-1 bg-primary-700 text-white hover:bg-primary-800 disabled:opacity-60`}><Plus size={16} />{edit ? 'Guardar cambios' : 'Crear modelo'}</button>{edit ? <button type="button" onClick={onCancel} className={`${buttonClass} border border-neutral-200 text-neutral-700`}>Cancelar</button> : null}</div>
    </form>
  </div>;
}

function Field({ label, name, defaultValue, required = false, inputMode, maxLength }: { label: string; name: string; defaultValue: string; required?: boolean; inputMode?: 'numeric' | 'text'; maxLength?: number }) {
  return <label className="block text-sm font-medium text-neutral-700">{label}<input className={inputClass} name={name} defaultValue={defaultValue} required={required} inputMode={inputMode} maxLength={maxLength} /></label>;
}

function StatusBadge({ active }: { active: boolean }) {
  return <span className={`rounded-pill px-3 py-1 text-xs font-semibold ${active ? 'bg-primary-100 text-primary-800' : 'bg-neutral-100 text-neutral-600'}`}>{active ? 'Activa' : 'Inactiva'}</span>;
}

function EmptyState({ text }: { text: string }) {
  return <div className="px-5 py-10 text-center"><p className="font-medium text-neutral-700">{text}</p><p className="mt-1 text-sm text-neutral-500">Usa el formulario para agregar un registro.</p></div>;
}
