import { describe, expect, it } from 'vitest';

import { danmaobTheme } from './theme';

import { accentPalette, brandColors, primaryPalette } from './tokens';

describe('danmaobTheme', () => {
  it('uses the DANMAOB primary color at the primary shade', () => {
    expect(danmaobTheme.primaryColor).toBe('danmaob');
    expect(danmaobTheme.colors?.danmaob?.[6]).toBe(brandColors.primary);
  });

  it('builds both palettes from the brand colors', () => {
    expect(primaryPalette[6]).toBe(brandColors.primary);
    expect(accentPalette[2]).toBe(brandColors.accent);
  });
});
