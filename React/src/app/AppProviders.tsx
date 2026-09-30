import { MantineProvider } from '@mantine/core';
import { Notifications } from '@mantine/notifications';


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
      <Notifications position="top-right" />
      <QueryClientProvider client={queryClient}>
        {children}
      </QueryClientProvider>
    </MantineProvider>
  );
}
