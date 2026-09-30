import { useQuery } from '@tanstack/react-query'
import { listPlans } from '../../api/platformPlans'
import { requireAccessToken } from '../../session/accessToken'

export const planKeys = {
  all: ['platform', 'plans'] as const
}

export function usePlans() {
  return useQuery({
    queryKey: planKeys.all,
    queryFn: () => listPlans(requireAccessToken())
  })
}
