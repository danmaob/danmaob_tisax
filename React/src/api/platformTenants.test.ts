import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { setConfig } from '../app/config';
import { listTenants, changeTenantPlan, deactivateTenant } from './platformTenants';

const token = 'test-token';
const tenant = {
  id: 't1',
  name: 'Acme',
  status: 'Active',
  createdAtUtc: '2026-09-30T00:00:00Z',
  planId: 'p1'
};
const emptyPage = {
  items: [],
  totalCount: 0,
  pageNumber: 2,
  pageSize: 20
};

describe('platformTenants', () => {
  let fetchMock: ReturnType<typeof vi.fn>;

  beforeEach(() => {
    setConfig({ apiBaseUrl: '/api/v1' });
    fetchMock = vi.fn(async () => new Response(JSON.stringify({}), { status: 200 }));
    vi.stubGlobal('fetch', fetchMock);
  });
  beforeEach(() => {
    setConfig({ apiBaseUrl: '/api/v1' });
  });

  afterEach(() => {
    vi.unstubAllGlobals();
  });

  it('builds the list path with every filter', async () => {
    fetchMock.mockImplementation(async () =>
      new Response(JSON.stringify(emptyPage), { status: 200 })
    );

    await listTenants(
      { status: 'Suspended', name: '  acme ', pageNumber: 2, pageSize: 20 },
      token
    );

    const [url] = fetchMock.mock.calls[0] as unknown as [string, RequestInit];
    expect(url).toBe(
      '/api/v1/platform/tenants?status=Suspended&name=acme&pageNumber=2&pageSize=20'
    );
    const response = await listTenants(
      { status: 'Suspended', name: '  acme ', pageNumber: 2, pageSize: 20 },
      token
    );
    expect(response.pageNumber).toBe(2);
  });

  it('omits empty filters from the list path', async () => {
    fetchMock.mockImplementation(async () =>
      new Response(JSON.stringify(emptyPage), { status: 200 })
    );

    await listTenants(
      { status: null, name: '   ', pageNumber: 1, pageSize: 20 },
      token
    );

    const [url] = fetchMock.mock.calls[0] as unknown as [string, RequestInit];
    expect(url).toBe('/api/v1/platform/tenants?pageNumber=1&pageSize=20');
  });

  it('changes the plan with PUT', async () => {
    fetchMock.mockImplementation(async () =>
      new Response(JSON.stringify(tenant), { status: 200 })
    );

    await changeTenantPlan('t1', 'p1', token);

    const [url, init] = fetchMock.mock.calls[0] as unknown as [
      string,
      RequestInit
    ];
    expect(url).toBe('/api/v1/platform/tenants/t1/plan');
    expect(init.method).toBe('PUT');
    expect(init.body).toBe('{"planId":"p1"}');
  });

  it('deactivates with POST and no body', async () => {
    fetchMock.mockImplementation(async () =>
      new Response(JSON.stringify(tenant), { status: 200 })
    );

    await deactivateTenant('t1', token);

    const [url, init] = fetchMock.mock.calls[0] as unknown as [
      string,
      RequestInit
    ];
    expect(url).toBe('/api/v1/platform/tenants/t1/deactivate');
    expect(init.method).toBe('POST');
    expect(init.body).toBeUndefined();
  });
});
