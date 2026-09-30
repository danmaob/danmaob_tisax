import { Button, Flex, Stack, Text, Title } from '@mantine/core'
import { useTranslation } from 'react-i18next'

export interface TenantsPageHeaderProps {
  onCreate: () => void
}

export function TenantsPageHeader({ onCreate }: TenantsPageHeaderProps) {
  const { t } = useTranslation()

  return (
    <Flex
      direction={{ base: 'column', sm: 'row' }}
      justify={{ base: 'flex-start', sm: 'space-between' }}
      align={{ base: 'stretch', sm: 'center' }}
      gap='sm'
    >
      <Stack gap={4}>
        <Title order={2}>{t('tenants.title')}</Title>
        <Text c="dimmed" size="sm">{t('tenants.description')}</Text>
      </Stack>
      <Button onClick={onCreate}>{t('tenants.actions.create')}</Button>
    </Flex>
  )
}
