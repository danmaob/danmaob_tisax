import { MantineProvider } from '@mantine/core';

import { QueryClientProvider } from '@tanstack/react-query';

import type { ReactNode } from 'react';

import { danmaobTheme } from '../theme/theme';

import { queryClient } from './queryClient';

interface AppProvidersProps {
  children: ReactNode;
}

export function AppProviders({ children }: AppProvidersProps) {
  return (
    <MantineProvider theme={danmaobTheme} defaultColorScheme="light">
      <QueryClientProvider client={queryClient}>
        {children}
      </QueryClientProvider>
    </MantineProvider>
  );
}
