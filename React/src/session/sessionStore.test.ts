import { afterEach, describe, expect, it, vi } from 'vitest';
import { endSession, getLastEndReason, getSession, readTokenExpiryMs, startSession } from './sessionStore';

const buildToken = (exp: number) => 'header.' + btoa(JSON.stringify({ exp })).replace(/=+$/, '') + '.signature';

describe('sessionStore', () => {
  afterEach(() => {
    vi.useRealTimers();
    endSession('logout');
  });

  it('reads the expiry from the token', () => {
    expect(readTokenExpiryMs(buildToken(1900000000))).toBe(1900000000000);
  });

  it('ends the session as expired when the token expires', () => {
    vi.useFakeTimers();
    const nowMs = Date.now();
    startSession(buildToken(Math.floor(nowMs / 1000) + 60), 'admin@example.com', nowMs);
    expect(getSession()?.email).toBe('admin@example.com');
    vi.advanceTimersByTime(61000);
    expect(getSession()).toBeNull();
    expect(getLastEndReason()).toBe('expired');
  });

  it('ends the session on logout', () => {
    startSession(buildToken(Math.floor(Date.now() / 1000) + 600), 'admin@example.com');
    endSession('logout');
    expect(getSession()).toBeNull();
    expect(getLastEndReason()).toBe('logout');
  });
});
