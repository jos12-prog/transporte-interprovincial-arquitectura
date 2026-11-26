import { useEffect, useRef, type ReactNode } from 'react';
import { Ban, CircleCheck, CircleX, CircleMinus, Clock, Pencil } from 'lucide-react';

export type RowAction = {
  id: 'edit' | 'activate' | 'deactivate' | 'cancel';
  label: string;
  onSelect: () => void;
};

const actionIcons = {
  edit: Pencil,
  activate: CircleCheck,
  deactivate: Ban,
  cancel: CircleX,
} as const;

export function RowActions({ name, actions, disabled = false }: { name: string; actions: RowAction[]; disabled?: boolean }) {
  const orderedActions = [...actions].sort((left, right) => {
    const rank = (action: RowAction) => action.id === 'edit' ? 0 : action.id === 'activate' ? 1 : action.id === 'deactivate' ? 2 : 3;
    return rank(left) - rank(right);
  });
  return <div className="flex min-w-max justify-end gap-2">
    {orderedActions.map(action => {
      const Icon = actionIcons[action.id];
      const style = action.id === 'edit'
        ? 'border-primary-600/30 text-primary-700 hover:bg-primary-100'
        : action.id === 'activate'
          ? 'border-success-600/30 text-[#15803D] hover:bg-success-100'
          : 'border-danger-600/30 text-danger-700 hover:bg-danger-100';
      return <button aria-label={`${action.label} ${name}`} className={`inline-flex h-8 shrink-0 items-center justify-center gap-[6px] rounded-lg border px-2.5 text-[13px] font-semibold transition-colors focus-visible:outline focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-primary-700 disabled:cursor-not-allowed disabled:opacity-50 lg:px-3 ${style}`} disabled={disabled} key={action.id} onClick={action.onSelect} title={`${action.label} ${name}`} type="button">
        <Icon aria-hidden="true" size={16} strokeWidth={1.8} /><span className="sr-only lg:not-sr-only">{action.label}</span>
      </button>;
    })}
  </div>;
}

export function StatusBadge({ status }: { status: 'active' | 'inactive' | 'scheduled' | 'cancelled' }) {
  const config = {
    active: { label: 'Activo', Icon: CircleCheck, style: 'b-success bg-success-100 text-[#15803D]' },
    inactive: { label: 'Inactivo', Icon: CircleMinus, style: 'b-neutral bg-neutral-100 text-neutral-600' },
    scheduled: { label: 'Programado', Icon: Clock, style: 'b-success bg-success-100 text-[#15803D]' },
    cancelled: { label: 'Cancelado', Icon: CircleX, style: 'b-danger bg-danger-100 text-[#B91C1C]' },
  }[status];
  const Icon = config.Icon;
  return <span className={`badge plain inline-flex h-6 items-center gap-[5px] rounded-full px-[9px] text-xs font-semibold ${config.style}`}><Icon aria-hidden="true" size={13} strokeWidth={2} />{config.label}</span>;
}

export function ConfirmDialog({ open, title, consequence, confirmLabel, pending = false, error, onConfirm, onCancel }: {
  open: boolean; title: string; consequence: string; confirmLabel: string; pending?: boolean; error?: string;
  onConfirm: () => void; onCancel: () => void;
}) {
  const ConfirmIcon = confirmLabel.toLocaleLowerCase('es-PE').includes('desactivar') ? Ban : CircleX;
  const dialog = useRef<HTMLDivElement>(null);
  const previousFocus = useRef<HTMLElement | null>(null);
  const onCancelRef = useRef(onCancel);
  const pendingRef = useRef(pending);
  onCancelRef.current = onCancel;
  pendingRef.current = pending;
  useEffect(() => {
    if (!open) return;
    previousFocus.current = document.activeElement instanceof HTMLElement ? document.activeElement : null;
    dialog.current?.querySelector<HTMLButtonElement>('[data-confirm]')?.focus();
    function keydown(event: KeyboardEvent) {
      if (event.key === 'Escape' && !pendingRef.current) { event.preventDefault(); onCancelRef.current(); return; }
      if (event.key !== 'Tab' || !dialog.current) return;
      const focusable = [...dialog.current.querySelectorAll<HTMLElement>('button:not(:disabled)')];
      if (!focusable.length) return;
      if (event.shiftKey && document.activeElement === focusable[0]) { event.preventDefault(); focusable.at(-1)?.focus(); }
      else if (!event.shiftKey && document.activeElement === focusable.at(-1)) { event.preventDefault(); focusable[0].focus(); }
    }
    document.addEventListener('keydown', keydown);
    return () => { document.removeEventListener('keydown', keydown); previousFocus.current?.focus(); };
  }, [open]);
  if (!open) return null;
  return <div className="scrim fixed inset-0 z-[70] grid place-items-center bg-[rgba(10,24,22,.45)] p-4" role="presentation">
    <div aria-labelledby="confirm-title" aria-modal="true" className="modal w-full max-w-[460px] rounded-3xl bg-white p-7 shadow-2xl" ref={dialog} role="alertdialog" tabIndex={-1}>
      <h2 className="text-lg font-semibold text-neutral-900" id="confirm-title">{title}</h2>
      <p className="mt-3 text-sm leading-6 text-neutral-600">{consequence}</p>
      {error ? <p className="mt-4 rounded-xl border border-danger-200 bg-danger-50 p-3 text-sm text-danger-700" role="alert">{error}</p> : null}
      <div className="mt-7 flex justify-end gap-2.5">
        <button className="btn-ghost inline-flex min-h-10 items-center justify-center rounded-xl border border-neutral-200 bg-white px-4 text-sm font-semibold text-neutral-700 shadow-sm hover:bg-neutral-50" disabled={pending} onClick={onCancel} type="button">Volver</button>
        <button className="btn-danger inline-flex min-h-10 items-center justify-center gap-2 rounded-xl bg-danger-600 px-4 text-sm font-semibold text-white disabled:cursor-wait disabled:opacity-70" data-confirm onClick={onConfirm} type="button">
          {pending ? <span aria-label="Guardando" className="h-4 w-4 animate-spin rounded-full border-2 border-white/40 border-t-white" role="status" /> : <ConfirmIcon aria-hidden="true" size={16} />}{pending ? 'Guardando…' : confirmLabel}
        </button>
      </div>
    </div>
  </div>;
}

export function Toast({ message, tone, onClose }: { message: string; tone: 'success' | 'error'; onClose: () => void }) {
  const Icon = tone === 'success' ? CircleCheck : CircleX;
  const onCloseRef = useRef(onClose);
  onCloseRef.current = onClose;
  useEffect(() => {
    const timeout = window.setTimeout(() => onCloseRef.current(), 4000);
    return () => window.clearTimeout(timeout);
  }, [message]);
  return <div className="toast fixed right-5 top-5 z-[80] flex items-center gap-3 rounded-[14px] border border-neutral-200 bg-white px-4 py-3.5 text-sm shadow-lg" role="status">
    <span className={`grid h-7 w-7 shrink-0 place-items-center rounded-full ${tone === 'success' ? 'bg-success-100 text-success-600' : 'bg-danger-100 text-danger-600'}`}><Icon aria-hidden="true" size={16} /></span>
    <span>{message}</span>
    <button aria-label="Cerrar notificación" className="ml-2 text-neutral-500 hover:text-neutral-900" onClick={onClose} type="button">×</button>
  </div>;
}

export function AdminDrawer({ title, onClose, children }: { title: string; onClose: () => void; children: ReactNode }) {
  const dialog = useRef<HTMLElement>(null);
  const previousFocus = useRef<HTMLElement | null>(null);
  const onCloseRef = useRef(onClose);
  onCloseRef.current = onClose;
  useEffect(() => {
    previousFocus.current = document.activeElement instanceof HTMLElement ? document.activeElement : null;
    dialog.current?.querySelector<HTMLElement>('input,select,button,textarea')?.focus();
    function keydown(event: KeyboardEvent) {
      if (event.key === 'Escape') { event.preventDefault(); onCloseRef.current(); return; }
      if (event.key !== 'Tab' || !dialog.current) return;
      const focusable = [...dialog.current.querySelectorAll<HTMLElement>('a[href],button:not(:disabled),input:not(:disabled),select:not(:disabled),textarea:not(:disabled)')];
      if (!focusable.length) return;
      if (event.shiftKey && document.activeElement === focusable[0]) { event.preventDefault(); focusable.at(-1)?.focus(); }
      else if (!event.shiftKey && document.activeElement === focusable.at(-1)) { event.preventDefault(); focusable[0].focus(); }
    }
    document.addEventListener('keydown', keydown);
    return () => { document.removeEventListener('keydown', keydown); previousFocus.current?.focus(); };
  }, []);
  return <div className="fixed inset-0 z-50">
    <button aria-label="Cerrar" className="fixed inset-0 bg-[rgba(10,24,22,.45)]" onClick={onClose} type="button" />
    <aside aria-labelledby="admin-drawer-title" aria-modal="true" className="drawer fixed inset-y-0 right-0 z-[51] flex w-full flex-col bg-white shadow-[-24px_0_48px_rgba(17,27,25,.16)] sm:max-w-[480px]" ref={dialog} role="dialog" tabIndex={-1}>
      <div className="flex items-center justify-between border-b border-neutral-200 px-6 py-5"><h2 className="text-lg font-semibold text-neutral-900" id="admin-drawer-title">{title}</h2><button aria-label="Cerrar" className="grid h-8 w-8 place-items-center rounded-lg text-neutral-500 hover:bg-neutral-100" onClick={onClose} type="button">×</button></div>
      {children}
    </aside>
  </div>;
}

export type ConfirmRequest = { title: string; consequence: string; confirmLabel: string; operation: () => Promise<void> };
