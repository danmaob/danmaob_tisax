import { MantineProvider } from '@mantine/core';

import type { ReactNode } from 'react';

import { danmaobTheme } from '../theme/theme';

interface AppProvidersProps {
  children: ReactNode;
}

export function AppProviders({ children }: AppProvidersProps) {
  return (
    <MantineProvider theme={danmaobTheme} defaultColorScheme="light">
      {children}
    </MantineProvider>
  );
}
