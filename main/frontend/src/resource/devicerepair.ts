import { apiClient } from './index';

export interface UploadExcelResponse {
    fileName: string;
    filePath: string;
}

export async function apiUploadDeviceRepairExcel(file: File): Promise<UploadExcelResponse> {
    const formData = new FormData();
    formData.append('file', file);

    const response = await apiClient.post<UploadExcelResponse>(
        '/DeviceRepairHistory/upload',
        formData,
        {
            headers: {
                'Content-Type': 'multipart/form-data'
            }
        }
    );
    return response.data;
}

export async function apiSaveDeviceRepairHistory(data: any): Promise<any> {
    const response = await apiClient.post(
        '/DeviceRepairHistory/save',
        data,
        {
            headers: {
                'Content-Type': 'application/json'
            }
        }
    );
    return response.data;
}

export async function apiGetDeviceRepairOptions(): Promise<any> {
    const response = await apiClient.get('/DeviceRepairHistory/options')
    return response.data
}

export async function apiQueryDeviceRepairHistory(data: any): Promise<any> {
    const response = await apiClient.post(
        '/DeviceRepairHistory/query',
        data,
        {
            headers: {
                'Content-Type': 'application/json'
            }
        }
    );
    return response.data;
}

export async function apiDeleteDeviceRepairYear(year: number): Promise<any> {
    const response = await apiClient.post(
        '/DeviceRepairHistory/deleteYear',
        year,
        {
            headers: {
                'Content-Type': 'application/json'
            }
        }
    );
    return response.data;
}