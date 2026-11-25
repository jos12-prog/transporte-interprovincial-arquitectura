import { Check, LoaderCircle } from 'lucide-react';

export type SubmitState = 'idle' | 'loading' | 'done';

export function SubmitButton({ state, children }: { state: SubmitState; children: string }) {
  return (
    <button className="flex min-h-12 w-full items-center justify-center gap-2 rounded-button bg-primary-600 px-5 py-3 text-sm font-semibold text-white transition-colors duration-150 hover:bg-primary-700 focus-visible:outline focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-accent-500" aria-busy={state === 'loading'}>
      {state === 'loading' ? <LoaderCircle className="animate-spin" aria-hidden="true" size={18} /> : null}
      {state === 'done' ? <Check aria-hidden="true" size={18} /> : null}
      <span aria-live="polite">{state === 'loading' ? 'Procesando…' : state === 'done' ? 'Listo' : children}</span>
    </button>
  );
}
