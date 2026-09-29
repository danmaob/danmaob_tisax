import { SegmentedControl } from '@mantine/core';
import { useTranslation } from 'react-i18next';
import { isSupportedLanguage, storeLanguage } from './language';

export function LanguageSwitcher() {
  const { t, i18n } = useTranslation();
  const handleChange = (value: string) => {
    if (isSupportedLanguage(value)) {
      storeLanguage(value);
      void i18n.changeLanguage(value);
      document.documentElement.lang = value;
    }
  };

  return <SegmentedControl aria-label={t('language.label')} value={i18n.resolvedLanguage ?? i18n.language} onChange={handleChange} data={[{ value: 'es', label: t('language.option.es') }, { value: 'en', label: t('language.option.en') }]} />;
}
