import type { PropsWithChildren } from 'react';

export function AuthCard({ title, description, children }: PropsWithChildren<{ title: string; description: string }>) {
  return (
    <section className="rounded-modal border border-white/10 bg-[#081a17]/80 p-6 shadow-auth backdrop-blur-[20px] sm:p-9">
      <h2 className="font-display text-2xl font-semibold tracking-tight text-white">{title}</h2>
      <p className="mt-2 text-sm leading-6 text-white/70">{description}</p>
      <div className="mt-7">{children}</div>
    </section>
  );
}
