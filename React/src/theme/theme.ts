import { createTheme } from '@mantine/core';

import { accentPalette, fontFamily, headingFontWeight, primaryPalette } from './tokens';

export const danmaobTheme = createTheme({
  primaryColor: 'danmaob',
  primaryShade: 6,
  colors: {
    danmaob: primaryPalette,
    danmaobAccent: accentPalette,
  },
  fontFamily,
  headings: {
    fontFamily,
    fontWeight: headingFontWeight,
  },
});
