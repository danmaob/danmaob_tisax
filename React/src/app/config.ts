export interface AppConfig { apiBaseUrl: string; }

let currentConfig: AppConfig | null = null;

export async function loadConfig(fetchImpl: typeof fetch = fetch): Promise<AppConfig> {
  const response = await fetchImpl('/config.json', { cache: 'no-store' });
  if (response.ok === false) {
    throw new Error('Configuration file could not be loaded.');
  }
  const data: unknown = await response.json();
  const apiBaseUrl = typeof data === 'object' && data !== null ? (data as { apiBaseUrl?: unknown }).apiBaseUrl : undefined;
  if (typeof apiBaseUrl !== 'string' || apiBaseUrl.length === 0) {
    throw new Error('Configuration file is invalid.');
  }
  currentConfig = { apiBaseUrl: apiBaseUrl.replace(/\/+$/, '') };
  return currentConfig;
}

export function getConfig(): AppConfig {
  if (currentConfig === null) {
    throw new Error('Configuration has not been loaded.');
  }
  return currentConfig;
}

export function setConfig(config: AppConfig): void {
  currentConfig = config;
}