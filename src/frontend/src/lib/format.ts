const limaZone = 'America/Lima';

export function formatLimaTripDateTime(value: string | Date): string {
  const date = value instanceof Date ? value : new Date(value);
  const parts = new Intl.DateTimeFormat('es-PE', {
    weekday: 'short', day: '2-digit', month: 'short', hour: '2-digit', minute: '2-digit',
    hourCycle: 'h23', timeZone: limaZone,
  }).formatToParts(date);
  const part = (type: Intl.DateTimeFormatPartTypes) => parts.find(item => item.type === type)?.value ?? '';
  return `${part('weekday').replace('.', '')} ${part('day')} ${part('month').replace('.', '')} · ${part('hour')}:${part('minute')}`;
}

export function formatLimaDate(value: string | Date): string {
  const date = value instanceof Date ? value : /^\d{4}-\d{2}-\d{2}$/.test(value) ? new Date(`${value}T12:00:00Z`) : new Date(value);
  const parts = new Intl.DateTimeFormat('es-PE', { day: '2-digit', month: 'short', year: 'numeric', timeZone: limaZone }).formatToParts(date);
  const part = (type: Intl.DateTimeFormatPartTypes) => parts.find(item => item.type === type)?.value ?? '';
  return `${part('day')} ${part('month').replace('.', '')} ${part('year')}`;
}
