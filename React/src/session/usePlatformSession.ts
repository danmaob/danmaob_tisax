import { useSyncExternalStore } from 'react';
import { getSession, subscribe } from './sessionStore';
import type { PlatformSession } from './sessionStore';

export function usePlatformSession(): PlatformSession | null {
  return useSyncExternalStore(subscribe, getSession);
}