import { describe, expect, it } from 'vitest';
import { resolveInitialLanguage } from './language';

describe('resolveInitialLanguage', () => {
  it('returns the stored language when it is supported', () => {
    expect(resolveInitialLanguage('en', 'es-MX')).toBe('en');
  });
  it('falls back to the browser language prefix', () => {
    expect(resolveInitialLanguage(null, 'en-US')).toBe('en');
  });
  it('defaults to Spanish when nothing is supported', () => {
    expect(resolveInitialLanguage('fr', 'de-DE')).toBe('es');
  });
});
