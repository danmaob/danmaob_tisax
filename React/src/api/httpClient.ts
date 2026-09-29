import { getConfig } from '../app/config';
import i18n from '../i18n/i18n';
import { endSession } from '../session/sessionStore';
import { ApiError } from './ApiError';

export interface RequestOptions {
  method?: 'GET' | 'POST' | 'PUT' | 'DELETE';
  body?: unknown;
  token?: string | null;
}

async function toApiError(response: Response): Promise<ApiError> {
  let errorCode: string | null = null;
  let message = response.statusText;
  try {
    const data: unknown = await response.json();
    if (typeof data === 'object' && data !== null) {
      const problem = data as { errorCode?: unknown; title?: unknown };
      if (typeof problem.errorCode === 'string') {
        errorCode = problem.errorCode;
      }
      if (typeof problem.title === 'string') {
        message = problem.title;
      }
    }
  } catch {
    errorCode = null;
  }
  return new ApiError(response.status, errorCode, message);
}

export async function apiRequest<T>(path: string, options: RequestOptions = {}): Promise<T> {
  const headers: Record<string, string> = { Accept: 'application/json', 'Accept-Language': i18n.language };
  if (options.body !== undefined) {
    headers['Content-Type'] = 'application/json';
  }
  if (typeof options.token === 'string' && options.token.length > 0) {
    headers.Authorization = 'Bearer ' + options.token;
  }
  const response = await fetch(getConfig().apiBaseUrl + path, { method: options.method ?? 'GET', headers, body: options.body === undefined ? undefined : JSON.stringify(options.body) });
  if (response.ok === false) {
    const error = await toApiError(response);
    if (error.status === 401 && typeof options.token === 'string') {
      endSession('expired');
    }
    throw error;
  }
  if (response.status === 204) {
    return undefined as T;
  }
  return (await response.json()) as T;
}