interface RoleLandingProps {
  title: string;
}

export function RoleLanding({ title }: RoleLandingProps) {
  return (
    <section aria-label={`Inicio de ${title.toLowerCase()}`} className="mx-auto my-10 max-w-container rounded-card border border-neutral-200 bg-white p-8 shadow-sm">
      <p className="text-sm font-semibold text-primary-700">RutaAndina</p>
      <h1 className="mt-2 font-display text-2xl font-bold text-neutral-900">{title}</h1>
      <p className="mt-2 text-neutral-600">Este espacio está preparado para la siguiente etapa del producto.</p>
    </section>
  );
}
