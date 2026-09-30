import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { setConfig } from '../app/config';
import { listPlans } from './platformPlans';

describe('platformPlans', () => {
  beforeEach(() => {
    setConfig({ apiBaseUrl: '/api/v1' });
  });

  afterEach(() => {
    vi.unstubAllGlobals();
  });

  it('lists the plans', async () => {
    const fetchMock = vi.fn(async () =>
      new Response(JSON.stringify([{ id: 'p1', code: 'FREE', name: 'Free', isActive: true, moduleCodes: ['Organization'] }]), { status: 200 })
    );

    vi.stubGlobal('fetch', fetchMock);

    const result = await listPlans('test-token');

    const [, init] = fetchMock.mock.calls[0] as unknown as [string, RequestInit];
    expect(init.method).toBe('GET');
    expect(result[0]?.name).toBe('Free');
  });
});
