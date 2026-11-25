import type { InputHTMLAttributes } from 'react';

export function AuthField({ label, id, ...props }: InputHTMLAttributes<HTMLInputElement> & { label: string; id: string }) {
  return (
    <label className="block text-sm font-medium text-white/90" htmlFor={id}>
      {label}
      <input {...props} id={id} className="mt-2 min-h-12 w-full rounded-button border border-white/20 bg-white/5 px-4 text-sm text-white outline-none transition-colors duration-150 placeholder:text-white/45 focus:border-accent-500 focus:ring-4 focus:ring-accent-500/15" />
    </label>
  );
}
