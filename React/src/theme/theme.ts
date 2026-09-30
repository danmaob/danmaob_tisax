import { createTheme, Title } from '@mantine/core';

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
  components: {
    Title: Title.extend({
      defaultProps: {
        c: 'danmaob.6',
      },
    }),
  },
});
