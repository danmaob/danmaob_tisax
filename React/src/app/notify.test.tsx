import { act, screen } from '@testing-library/react';
import { describe, expect, it } from 'vitest';
import { renderWithProviders } from '../test/renderWithProviders';
import { showSuccess } from './notify';

describe('notify', () => {
  it('shows a success notification', async () => {
    renderWithProviders(<div>host</div>);

    await act(async () => {
      showSuccess('Tenant creado');
    });

    const notification = await screen.findByText('Tenant creado');
    expect(notification).toBeInTheDocument();
  });
});
