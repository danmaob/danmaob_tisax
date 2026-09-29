import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { setConfig } from '../app/config';
import i18n from '../i18n/i18n';
import { getSession, startSession } from '../session/sessionStore';
import { ApiError } from './ApiError';
import { apiRequest } from './httpClient';

const token = 'header.' + btoa(JSON.stringify({ exp: Math.floor(Date.now() / 1000) + 600 })).replace(/=+$/, '') + '.signature';

describe('apiRequest', () => {
  beforeEach(async () => {
    setConfig({ apiBaseUrl: '/api/v1' });
    await i18n.changeLanguage('en');
  });
  afterEach(() => {
    vi.unstubAllGlobals();
  });

  it('sends the language and the bearer token', async () => {
    const fetchMock = vi.fn(async () => new Response(JSON.stringify({ value: 1 }), { status: 200 }));
    vi.stubGlobal('fetch', fetchMock);
    const result = await apiRequest<{ value: number }>('/platform/plans', { token });
    expect(result.value).toBe(1);
    const [url, init] = fetchMock.mock.calls[0] as unknown as [string, RequestInit];
    expect(url).toBe('/api/v1/platform/plans');
    expect((init.headers as Record<string, string>)['Accept-Language']).toBe('en');
    expect((init.headers as Record<string, string>).Authorization).toBe('Bearer ' + token);
  });

  it('maps the problem details into an ApiError', async () => {
    vi.stubGlobal('fetch', vi.fn(async () => new Response(JSON.stringify({ title: 'Conflict', errorCode: 'Tenant.NameAlreadyExists' }), { status: 409 })));
    const error = await apiRequest('/platform/tenants', { method: 'POST', body: { name: 'X' }, token }).catch((caught: unknown) => caught);
    expect(error).toBeInstanceOf(ApiError);
    expect((error as ApiError).status).toBe(409);
    expect((error as ApiError).errorCode).toBe('Tenant.NameAlreadyExists');
  });

  it('ends the session when an authenticated request returns 401', async () => {
    startSession(token, 'admin@example.com');
    vi.stubGlobal('fetch', vi.fn(async () => new Response('', { status: 401 })));
    await apiRequest('/platform/plans', { token }).catch(() => undefined);
    expect(getSession()).toBeNull();
  });
});