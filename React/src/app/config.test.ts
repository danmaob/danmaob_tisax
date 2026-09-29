import { describe, expect, it } from 'vitest';
import { getConfig, loadConfig } from './config';

describe('loadConfig', () => {
  it('loads the API base URL and removes trailing slashes', async () => {
    const fetchImpl = async () => new Response(JSON.stringify({ apiBaseUrl: 'https://api.example.com/api/v1/' }), { status: 200 });
    const config = await loadConfig(fetchImpl);
    expect(config.apiBaseUrl).toBe('https://api.example.com/api/v1');
    expect(getConfig().apiBaseUrl).toBe('https://api.example.com/api/v1');
  });
  it('rejects a configuration without apiBaseUrl', async () => {
    const fetchImpl = async () => new Response(JSON.stringify({ other: 'value' }), { status: 200 });
    await expect(loadConfig(fetchImpl)).rejects.toThrow('Configuration file is invalid.');
  });
});