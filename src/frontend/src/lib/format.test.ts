import { formatLimaDate, formatLimaTripDateTime } from './format';

describe('Lima date formatting', () => {
  it('formats trip departure in Spanish without seconds in America/Lima', () => {
    expect(formatLimaTripDateTime('2026-11-25T13:00:00Z')).toBe('mié 25 nov · 08:00');
  });

  it('formats fare dates as day, abbreviated month, and year', () => {
    expect(formatLimaDate('2026-10-26')).toBe('26 oct 2026');
  });
});
