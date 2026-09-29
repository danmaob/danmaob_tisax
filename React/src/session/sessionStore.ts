export interface PlatformSession { accessToken: string; email: string; expiresAtMs: number; }
export type SessionEndReason = 'logout' | 'expired';
let session: PlatformSession | null = null;
let lastEndReason: SessionEndReason | null = null;
let expiryTimer: ReturnType<typeof setTimeout> | null = null;
const listeners = new Set<() => void>();

function notify(): void {
  listeners.forEach((listener) => listener());
}

function clearExpiryTimer(): void {
  if (expiryTimer !== null) {
    clearTimeout(expiryTimer);
    expiryTimer = null;
  }
}

export function readTokenExpiryMs(token: string): number {
  const payload = token.split('.')[1];
  if (payload === undefined) {
    throw new Error('Invalid access token.');
  }
  const claims = JSON.parse(atob(payload.replace(/-/g, '+').replace(/_/g, '/'))) as { exp?: unknown };
  if (typeof claims.exp !== 'number') {
    throw new Error('Access token has no expiry.');
  }
  return claims.exp * 1000;
}

export function getSession(): PlatformSession | null {
  return session;
}

export function getLastEndReason(): SessionEndReason | null {
  return lastEndReason;
}

export function subscribe(listener: () => void): () => void {
  listeners.add(listener);
  return () => { listeners.delete(listener); };
}

export function endSession(reason: SessionEndReason): void {
  clearExpiryTimer();
  session = null;
  lastEndReason = reason;
  notify();
}

export function startSession(accessToken: string, email: string, nowMs: number = Date.now()): void {
  const expiresAtMs = readTokenExpiryMs(accessToken);
  clearExpiryTimer();
  session = { accessToken, email, expiresAtMs };
  lastEndReason = null;
  expiryTimer = setTimeout(() => endSession('expired'), Math.max(expiresAtMs - nowMs, 0));
  notify();
}
