import { apiClient } from './index';

export interface FileTreeResponse {
  name: string;
  isFolder: boolean;
  subFolder: SubFolder[];
  iconType: string;
  iconPath: string;
  downloadLink: string;
}

export interface SubFolder {
  name: string;
  isFolder: boolean;
  iconType: string;
  iconPath: string;
  downloadLink: string;
}

export async function apiGetFileTree(): Promise<FileTreeResponse[]> {
  const response = await apiClient.get<FileTreeResponse[]>(
    `/Download/GetFileTree`,
    {},
  );

  return response.data;
}
