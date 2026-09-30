import { ApiError } from './ApiError';

export function getApiErrorMessage(error: unknown, fallbackMessage: string): string {
  if (error instanceof ApiError && error.errorCode !== null) {
    const trimmed = error.message.trim();
    if (trimmed.length > 0) {
      return error.message;
    }
  }

  return fallbackMessage;
}
