import { keepPreviousData, useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import {
  changeTenantPlan,
  createTenant,
  deactivateTenant,
  listTenants,
  reactivateTenant,
  suspendTenant,
} from '../../api/platformTenants';
import type { TenantListQuery } from '../../api/platformTenants';
import { requireAccessToken } from '../../session/accessToken';

export const tenantKeys = {
  all: ['platform', 'tenants'] as const,
  list: (query: TenantListQuery) => ['platform', 'tenants', 'list', query] as const,
};

export interface ChangeTenantPlanVariables {
  tenantId: string;
  planId: string;
}

export function useTenantList(query: TenantListQuery) {
  return useQuery({
    queryKey: tenantKeys.list(query),
    queryFn: () => listTenants(query, requireAccessToken()),
    placeholderData: keepPreviousData,
  });
}

export function useCreateTenant() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (name: string) => createTenant(name, requireAccessToken()),
    onSettled: () => queryClient.invalidateQueries({ queryKey: tenantKeys.all }),
  });
}

export function useChangeTenantPlan() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (variables: ChangeTenantPlanVariables) =>
      changeTenantPlan(variables.tenantId, variables.planId, requireAccessToken()),
    onSettled: () => queryClient.invalidateQueries({ queryKey: tenantKeys.all }),
  });
}

export function useSuspendTenant() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (tenantId: string) => suspendTenant(tenantId, requireAccessToken()),
    onSettled: () => queryClient.invalidateQueries({ queryKey: tenantKeys.all }),
  });
}

export function useReactivateTenant() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (tenantId: string) => reactivateTenant(tenantId, requireAccessToken()),
    onSettled: () => queryClient.invalidateQueries({ queryKey: tenantKeys.all }),
  });
}

export function useDeactivateTenant() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (tenantId: string) => deactivateTenant(tenantId, requireAccessToken()),
    onSettled: () => queryClient.invalidateQueries({ queryKey: tenantKeys.all }),
  });
}
