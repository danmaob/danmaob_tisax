import { render } from '@testing-library/react';

import type { RenderOptions } from '@testing-library/react';

import type { ReactElement } from 'react';

import { AppProviders } from '../app/AppProviders';
import '../i18n/i18n';

export function renderWithProviders(ui: ReactElement, options?: Omit<RenderOptions, 'wrapper'>) {
  return render(ui, { wrapper: AppProviders, ...options });
}
