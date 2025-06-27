import { apiClient } from './index';

export interface CameraByAreaIdSimpleResponse {
  stationID: string;
  stationNameA: string;
  camName: string;
  x: string;
  y: string;
  streamMain: string;
}

export async function apiGetCameraByAreaIdSimple(
  payload: string,
): Promise<CameraByAreaIdSimpleResponse[]> {
  const response = await apiClient.post<CameraByAreaIdSimpleResponse[]>(
    `/VideoImage/GetCameraByAreaIdSimple?areaId=${payload}`,
    {},
  );
  return response.data;
}

export interface CameraBySearchPayload {
  areaId: string;
  stationId: string;
  isAlarm: string;
  keyword: string;
}

export interface CameraBySearchResponse {
  isAlarm: number;
  stationNameA: string;
  streamMain: string;
  camName: string;
}

export async function apiGetCameraBySearch(
  payload: CameraBySearchPayload,
): Promise<CameraBySearchResponse[]> {
  const response = await apiClient.post<CameraBySearchResponse[]>(
      `/VideoImage/GetCameraBySearch?areaId=${payload.areaId}&stationId=${payload.stationId}&isAlarm=${payload.isAlarm}&keyword=${payload.keyword}`,
     // `/VideoImage/GetCameraBySearch?areaId=${payload.areaId}&stationId=${payload.stationId}&isAlarm=${payload.isAlarm}&keyword=${payload.keyword}`,
    {},
  );
  return response.data;
}
