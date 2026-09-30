import { Stack } from '@mantine/core'
import { TenantsPageHeader } from './TenantsPageHeader'

export function TenantsPage() {
  const handleCreate = () => undefined

  return (
    <Stack gap="md">
      <TenantsPageHeader onCreate={handleCreate} />
    </Stack>
  )
}
