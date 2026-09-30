import { describe, expect, it } from 'vitest';
import { ApiError } from './ApiError';
import { getApiErrorMessage } from './errorMessage';

describe('getApiErrorMessage', () => {
  it('returns the server message when the error has an error code', () => {
    const error = new ApiError(409, 'Tenant.NameAlreadyExists', 'Ya existe un tenant con ese nombre.');
    expect(getApiErrorMessage(error, 'fallback')).toBe('Ya existe un tenant con ese nombre.');
  });

  it('returns the fallback when the error has no error code', () => {
    const error = new ApiError(500, null, 'Internal Server Error');
    expect(getApiErrorMessage(error, 'fallback')).toBe('fallback');
  });

  it('returns the fallback for errors that are not ApiError', () => {
    const error = new TypeError('Failed to fetch');
    expect(getApiErrorMessage(error, 'fallback')).toBe('fallback');
  });
});
