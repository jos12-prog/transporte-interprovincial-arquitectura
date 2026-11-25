import type { PropsWithChildren } from 'react';
import logo from '@/assets/brand/logo-horizontal.svg';
import background from '@/assets/auth/fondo.jpg';

export function AuthLayout({ children, eyebrow }: PropsWithChildren<{ eyebrow: string }>) {
  return (
    <main className="relative isolate flex min-h-screen items-center justify-center overflow-hidden bg-neutral-900 px-4 py-16 text-white sm:px-8">
      <img src={background} alt="" aria-hidden="true" className="absolute inset-0 -z-20 h-full w-full object-cover" />
      <div aria-hidden="true" className="absolute inset-0 -z-10 bg-[radial-gradient(ellipse_at_50%_48%,rgba(8,26,23,.52),rgba(8,26,23,.86)_72%),linear-gradient(180deg,rgba(6,35,30,.28),rgba(6,35,30,.72))]" />
      <LinkBrand logo={logo} />
      <div className="w-full max-w-[440px] animate-auth-enter sm:max-w-[480px]">
        <div className="mb-4 text-center text-xs font-semibold uppercase tracking-[.16em] text-white/70">{eyebrow}</div>
        {children}
      </div>
      <p className="absolute bottom-5 left-6 hidden text-sm text-white/65 sm:block">● Ayacucho, Perú</p>
      <p className="absolute bottom-5 right-6 hidden text-sm text-white/55 sm:block">© 2026 RutaAndina · Ayacucho, Perú</p>
    </main>
  );
}

function LinkBrand({ logo }: { logo: string }) {
  return <a href="/pasajero" className="absolute left-5 top-5 rounded bg-white px-3 py-2 sm:left-8 sm:top-8" aria-label="RutaAndina, ir al inicio"><img src={logo} alt="RutaAndina" className="h-8 w-auto" /></a>;
}
