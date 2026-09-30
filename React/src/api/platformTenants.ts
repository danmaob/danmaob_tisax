import { apiRequest } from './httpClient';
import type { PagedResult } from './PagedResult';

export type TenantStatus = 'Active' | 'Suspended' | 'Deactivated';

export interface TenantDto {
  id: string;
  name: string;
  status: TenantStatus;
  createdAtUtc: string;
  planId: string;
}

export interface TenantListQuery {
  status: TenantStatus | null;
  name: string;
  pageNumber: number;
  pageSize: number;
}

export async function listTenants(
  query: TenantListQuery,
  token: string
): Promise<PagedResult<TenantDto>> {
  const params = new URLSearchParams();
  if (query.status !== null) {
    params.append('status', query.status);
  }
  const trimmedName = query.name.trim();
  if (trimmedName !== '') {
    params.append('name', trimmedName);
  }
  params.append('pageNumber', String(query.pageNumber));
  params.append('pageSize', String(query.pageSize));

  return apiRequest<PagedResult<TenantDto>>(
    '/platform/tenants?' + params.toString(),
    { method: 'GET', token }
  );
}

export async function createTenant(
  name: string,
  token: string
): Promise<TenantDto> {
  return apiRequest<TenantDto>('/platform/tenants', {
    method: 'POST',
    body: { name },
    token
  });
}

export async function changeTenantPlan(
  tenantId: string,
  planId: string,
  token: string
): Promise<TenantDto> {
  return apiRequest<TenantDto>(
    '/platform/tenants/' + tenantId + '/plan',
    { method: 'PUT', body: { planId }, token }
  );
}

export async function suspendTenant(
  tenantId: string,
  token: string
): Promise<TenantDto> {
  return apiRequest<TenantDto>(
    '/platform/tenants/' + tenantId + '/suspend',
    { method: 'POST', token }
  );
}

export async function reactivateTenant(
  tenantId: string,
  token: string
): Promise<TenantDto> {
  return apiRequest<TenantDto>(
    '/platform/tenants/' + tenantId + '/reactivate',
    { method: 'POST', token }
  );
}

export async function deactivateTenant(
  tenantId: string,
  token: string
): Promise<TenantDto> {
  return apiRequest<TenantDto>(
    '/platform/tenants/' + tenantId + '/deactivate',
    { method: 'POST', token }
  );
}
