export const SUPPORTED_LANGUAGES = ['es', 'en'] as const;
export type SupportedLanguage = (typeof SUPPORTED_LANGUAGES)[number];
export const DEFAULT_LANGUAGE: SupportedLanguage = 'es';
export const LANGUAGE_STORAGE_KEY = 'danmaob.language';
export function isSupportedLanguage(value: string | null | undefined): value is SupportedLanguage {
  return value === 'es' || value === 'en';
}
export function resolveInitialLanguage(storedLanguage: string | null, browserLanguage: string | undefined): SupportedLanguage {
  if (isSupportedLanguage(storedLanguage)) {
    return storedLanguage;
  }
  const browserPrefix = (browserLanguage ?? '').slice(0, 2).toLowerCase();
  if (isSupportedLanguage(browserPrefix)) {
    return browserPrefix;
  }
  return DEFAULT_LANGUAGE;
}
export function readStoredLanguage(): string | null {
  try {
    return window.localStorage.getItem(LANGUAGE_STORAGE_KEY);
  } catch {
    return null;
  }
}
export function storeLanguage(language: SupportedLanguage): void {
  try {
    window.localStorage.setItem(LANGUAGE_STORAGE_KEY, language);
  } catch {
    return;
  }
}
