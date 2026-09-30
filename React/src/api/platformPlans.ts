import { apiRequest } from './httpClient';

export interface PlanDto {
  id: string;
  code: string;
  name: string;
  isActive: boolean;
  moduleCodes: string[];
}

export async function listPlans(token: string): Promise<PlanDto[]> {
  return apiRequest<PlanDto[]>('/platform/plans', {
    method: 'GET',
    token
  });
}
