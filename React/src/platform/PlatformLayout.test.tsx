import { screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { MemoryRouter, Route, Routes } from 'react-router';
import { beforeEach, describe, expect, it } from 'vitest';
import i18n from '../i18n/i18n';
import { getSession, startSession } from '../session/sessionStore';
import { renderWithProviders } from '../test/renderWithProviders';
import { PlatformLayout } from './PlatformLayout';

const token = 'header.' + btoa(JSON.stringify({ exp: Math.floor(Date.now() / 1000) + 600 })).replace(/=+$/, '') + '.signature';

describe('PlatformLayout', () => {
  beforeEach(async () => {
    await i18n.changeLanguage('es');
  });

  it('shows the signed-in email and signs out', async () => {
    startSession(token, 'admin@example.com');

    const user = userEvent.setup();

    renderWithProviders(
      <MemoryRouter initialEntries={['/platform']}>
        <Routes>
          <Route path="/platform" element={<PlatformLayout />}>
            <Route index element={<div>platform-content</div>} />
          </Route>
          <Route path="/platform/login" element={<div>login-page</div>} />
        </Routes>
      </MemoryRouter>,
    );

    expect(screen.getAllByText('admin@example.com')).toHaveLength(2);
    expect(screen.getByText('platform-content')).toBeInTheDocument();

    await user.click(screen.getAllByRole('button', { name: 'Cerrar sesión' })[0] as HTMLElement);

    expect(await screen.findByText('login-page')).toBeInTheDocument();
    expect(getSession()).toBeNull();
  });
});
