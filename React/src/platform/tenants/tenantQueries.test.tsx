import { act, renderHook, waitFor } from '@testing-library/react';
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { AppProviders } from '../../app/AppProviders';
import { setConfig } from '../../app/config';
import { queryClient } from '../../app/queryClient';
import type { TenantListQuery } from '../../api/platformTenants';
import { endSession, startSession } from '../../session/sessionStore';
import { useSuspendTenant, useTenantList } from './tenantQueries';

const token = 'header.' + btoa(JSON.stringify({ exp: Math.floor(Date.now() / 1000) + 600 })).replace(/=+$/, '') + '.signature';
const tenant = { id: 't1', name: 'Acme', status: 'Active', createdAtUtc: '2026-09-30T00:00:00Z', planId: 'p1' };
const page = { items: [tenant], totalCount: 1, pageNumber: 1, pageSize: 20 };
const query: TenantListQuery = { status: null, name: '', pageNumber: 1, pageSize: 20 };

describe('tenantQueries', () => {
  beforeEach(() => {
    setConfig({ apiBaseUrl: '/api/v1' });
    startSession(token, 'admin@example.com');
  });

  afterEach(() => {
    queryClient.clear();
    vi.unstubAllGlobals();
    endSession('logout');
  });

  it('loads a page of tenants', async () => {
    vi.stubGlobal('fetch', vi.fn(async () =>
      new Response(JSON.stringify(page), { status: 200 })
    ));

    const { result } = renderHook(() => useTenantList(query), { wrapper: AppProviders });

    await waitFor(() => expect(result.current.isSuccess).toBe(true));

    expect(result.current.data?.items[0]?.name).toBe('Acme');
  });

  it('refreshes the list after an action', async () => {
    const fetchMock = vi.fn(async (
      _url: string,
      _init?: RequestInit
    ) =>
      new Response(JSON.stringify((_init?.method === 'POST') ? tenant : page), { status: 200 })
    );
    vi.stubGlobal('fetch', fetchMock);

    const { result } = renderHook(() => ({ list: useTenantList(query), suspend: useSuspendTenant() }), { wrapper: AppProviders });

    await waitFor(() => expect(result.current.list.isSuccess).toBe(true));

    expect(fetchMock).toHaveBeenCalledTimes(1);

    await act(async () => {
      await result.current.suspend.mutateAsync('t1');
    });

    await waitFor(() => expect(fetchMock).toHaveBeenCalledTimes(3));

    expect(fetchMock.mock.calls[1]?.[0]).toBe('/api/v1/platform/tenants/t1/suspend');
  });
});
