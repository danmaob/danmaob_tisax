import { Paper, Stack, Text, Title } from '@mantine/core';
import { useTranslation } from 'react-i18next';

export function PlatformHomePage() {
  const { t } = useTranslation();

  return (
    <Paper withBorder radius="md" p="lg">
      <Stack gap="xs">
        <Title order={2}>{t('platform.home.title')}</Title>
        <Text c="dimmed">{t('platform.home.description')}</Text>
      </Stack>
    </Paper>
  );
}
