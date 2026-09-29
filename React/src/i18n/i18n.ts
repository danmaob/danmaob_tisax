import i18n from 'i18next';
import { initReactI18next } from 'react-i18next';
import en from './locales/en.json';
import es from './locales/es.json';
import { DEFAULT_LANGUAGE, readStoredLanguage, resolveInitialLanguage, SUPPORTED_LANGUAGES } from './language';

void i18n.use(initReactI18next).init({
  resources: { es: { translation: es }, en: { translation: en } },
  lng: resolveInitialLanguage(readStoredLanguage(), window.navigator.language),
  fallbackLng: DEFAULT_LANGUAGE,
  supportedLngs: [...SUPPORTED_LANGUAGES],
  initAsync: false,
  interpolation: { escapeValue: false }
});

document.documentElement.lang = i18n.language;

export default i18n;
