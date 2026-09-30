import { screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { MemoryRouter, Route, Routes } from 'react-router';
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { setConfig } from '../app/config';
import i18n from '../i18n/i18n';
import { endSession, getSession } from '../session/sessionStore';
import { renderWithProviders } from '../test/renderWithProviders';
import { PlatformLoginPage } from './PlatformLoginPage';

const token = 'header.' + btoa(JSON.stringify({ exp: Math.floor(Date.now() / 1000) + 600 })).replace(/=+$/, '') + '.signature';

const renderPage = () => renderWithProviders(<MemoryRouter initialEntries={['/platform/login']}><Routes><Route path="/platform/login" element={<PlatformLoginPage />} /><Route path="/platform" element={<div>platform-home</div>} /></Routes></MemoryRouter>);

describe('PlatformLoginPage', () => {
  beforeEach(async () => { setConfig({ apiBaseUrl: '/api/v1' }); await i18n.changeLanguage('es'); });
  afterEach(() => { vi.unstubAllGlobals(); endSession('logout'); });

  it('starts the session and opens the platform home on success', async () => {
    vi.stubGlobal('fetch', vi.fn(async () => new Response(JSON.stringify({ succeeded: true, accessToken: token }), { status: 200 })));
    const user = userEvent.setup();
    renderPage();
    await user.type(screen.getByLabelText('Correo electrónico'), 'admin@example.com');
    await user.type(screen.getByLabelText('Contraseña'), 'Str0ng!Passw0rd');
    await user.click(screen.getByRole('button', { name: 'Iniciar sesión' }));
    expect(await screen.findByText('platform-home')).toBeInTheDocument();
    expect(getSession()?.email).toBe('admin@example.com');
  });

  it('shows an error for invalid credentials', async () => {
    vi.stubGlobal('fetch', vi.fn(async () => new Response('"InvalidCredentials"', { status: 401 })));
    const user = userEvent.setup();
    renderPage();
    await user.type(screen.getByLabelText('Correo electrónico'), 'admin@example.com');
    await user.type(screen.getByLabelText('Contraseña'), 'wrong-password');
    await user.click(screen.getByRole('button', { name: 'Iniciar sesión' }));
    expect(await screen.findByText('Correo o contraseña incorrectos.')).toBeInTheDocument();
    expect(getSession()).toBeNull();
  });
});
