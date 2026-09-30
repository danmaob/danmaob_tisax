import { screen } from '@testing-library/react';
import { MemoryRouter, Route, Routes } from 'react-router';
import { beforeEach, describe, expect, it } from 'vitest';
import { endSession } from '../session/sessionStore';
import { renderWithProviders } from '../test/renderWithProviders';
import { RequirePlatformSession } from './RequirePlatformSession';

describe('RequirePlatformSession', () => {
  beforeEach(() => {
    endSession('logout');
  });

  it('redirects to the platform login when there is no session', async () => {
    renderWithProviders(
      <MemoryRouter initialEntries={['/platform']}>
        <Routes>
          <Route path="/platform" element={<RequirePlatformSession />}>
            <Route index element={<div>platform-content</div>} />
          </Route>
          <Route path="/platform/login" element={<div>login-page</div>} />
        </Routes>
      </MemoryRouter>,
    );
    expect(await screen.findByText('login-page')).toBeInTheDocument();
    expect(screen.queryByText('platform-content')).not.toBeInTheDocument();
  });
});
