import { apiRequest } from './httpClient';

interface PlatformLoginResponse {
  accessToken?: string | null;
}

export async function loginPlatformAdmin(email: string, password: string): Promise<string> {
  const result = await apiRequest<PlatformLoginResponse>('/platform/auth/login', { method: 'POST', body: { email, password } });
  if (typeof result.accessToken !== 'string' || result.accessToken.length === 0) {
    throw new Error('The login response has no access token.');
  }
  return result.accessToken;
}