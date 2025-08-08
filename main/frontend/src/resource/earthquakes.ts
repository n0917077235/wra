import { apiClient } from './index';

export interface EQEventRangeResponse {
    event: string;
    status: string;
}

export interface EarthquakeBySearchPayload {
  year: string;
  month: string;
    event: string;
    checked1: boolean;
    checked2: boolean;
    intensity: string;
}
export interface EarthquakeByRange {
  range: string;
    eventTime: string;
    intensity: string;
}
export interface EQEventResponse {
  sensorId: string;
  sensorName: string;
  recordTime: string;
  intensity: string;
  grade: string;
  pga: string;
  pgv: string;
  eventGroup: string;
  eventTag: string;
  areaName: string;
}
export interface EarthquakeBySensorId {
    sensorId: string;
    eventTag: string;
}

export interface EQEventDetailResponse {
    xLabel: string;
    yLabel: string;
    zLabel: string;
    lstRecordTime: string[];
    lstX: string[];
    lstY: string[];
    lstZ: string[];
}

export async function apiGetEQEventRange(
  payload: EarthquakeBySearchPayload,
): Promise<EQEventRangeResponse[]> {
  const response = await apiClient.post<EQEventRangeResponse[]>(
      `/Earthquake/GetEQEventRangeIntensity?year=${payload.year}&month=${payload.month}&intensity=${payload.intensity}`,
    {},
  );
  
  return response.data;
}

export async function apiGetEQEvent(
  payload: EarthquakeByRange,
): Promise<EQEventResponse[]> {
  const response = await apiClient.post<EQEventResponse[]>(
    `/Earthquake/GetEQEvent?range=${payload.range}&eventTime=${payload.eventTime}`,
    {},
  );

  return response.data;
}

export async function apiGetEQEventNew(
    payload: EarthquakeByRange,
): Promise<EQEventResponse[]> {
    const response = await apiClient.post<EQEventResponse[]>(
        `/Earthquake/GetEQEventNew?range=${payload.range}&eventTime=${payload.eventTime}&intensity=${payload.intensity}`,
        {},
    );

    return response.data;
}

export async function apiGetEQEventDetail(
    payload: EarthquakeBySensorId,
): Promise<EQEventDetailResponse> {
    const response = await apiClient.post<EQEventDetailResponse>(
        `/Earthquake/GetEQEventDetail?sensorId=${payload.sensorId}&eventTag=${payload.eventTag}`,
        {},
    );
    
    return response.data;
}