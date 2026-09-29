import { Container, Group, Image, Stack, Text, Title } from '@mantine/core';
import { useTranslation } from 'react-i18next';
import { LanguageSwitcher } from './i18n/LanguageSwitcher';

export default function App() {
  const { t } = useTranslation();
  return (
    <Container size="sm" py="xl" data-testid="app-root">
      <Stack gap="lg">
        <Group justify="space-between">
          <Image src="/brand/danmaob_logo_principal.png" alt={t('brand.logoAlt')} h={48} w="auto" fit="contain" />
          <LanguageSwitcher />
        </Group>
        <Title order={1}>{t('app.name')}</Title>
        <Text c="dimmed">{t('app.tagline')}</Text>
        <Text>{t('foundation.ready')}</Text>
      </Stack>
    </Container>
  );
}
