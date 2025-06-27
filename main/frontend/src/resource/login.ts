import { apiClient } from './index';

export interface LoginRequest {
  username: string;
  password: string;
}

interface LoginResponse {
  userId: string;
  userName: string;
  token: string;
}

export async function apiLogin(payload: LoginRequest): Promise<LoginResponse> {
  const encodedUsername = encodeURIComponent(payload.username);
  const encodedPassword = encodeURIComponent(payload.password);
  const response = await apiClient.post<LoginResponse>(
    `/authen/LoginSmart?Username=${encodedUsername}&Password=${encodedPassword}`,
    {},
  );

  return response.data;
}

export async function apiLogout(payload: string): Promise<LoginResponse> {
  const response = await apiClient.post<LoginResponse>(
    `/authen/Logout?userId=${payload}`,
    {},
  );
  return response.data;
}
