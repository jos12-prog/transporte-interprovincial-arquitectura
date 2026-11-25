import { readFileSync } from 'node:fs';
import { join } from 'node:path';

describe('authentication motion accessibility', () => {
  it('keeps authentication state understandable while reducing motion', () => {
    const css = readFileSync(join(process.cwd(), 'src/styles.css'), 'utf8');
    expect(css).toContain('@media (prefers-reduced-motion: reduce)');
    expect(css).toContain('.animate-auth-enter { animation-name: auth-fade');
    expect(css).toContain('.animate-menu-in { animation: none');
    expect(css).toContain('.animate-spin { animation: none');
  });
});
