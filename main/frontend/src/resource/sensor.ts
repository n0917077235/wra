import { apiClient } from './index';

export interface WaterSensorAreaResponse {
  areaId: string;
  areaName: string;
}

export interface SensorGeneralQueryDataResponse {
    areaName: string;
    stationName: string;
    sensorId: string;
    sensorType: string;
    sensorTypeName: string;
    sensorName: string;
    sensorStatus: string;
    lastDataTime: string;
    gps: string;
    gpsLink: string;
    value: string;
    differ1: string;
    differ2: string;
    direction: string;
    more: string;
    serialNo: string;
    unit: string;
    x: string;
    y: string;
    fIndex: string;
    eventTag: string;
    eqGrade: string;
    status: string;
    userType: string;
}
export async function apiGetWaterSensorArea(): Promise<
    WaterSensorAreaResponse[]
> {
    const response = await apiClient.get<WaterSensorAreaResponse[]>(
        '/Sensor/GetWaterSensorArea',
        {},
    );
    return response.data;
}


export async function apiGetWaterSensorAreaCCTV(): Promise<
  WaterSensorAreaResponse[]
> {
  const response = await apiClient.get<WaterSensorAreaResponse[]>(
    '/Sensor/GetWaterSensorAreaCCTV',
    {},
  );
  return response.data;
}

//1130920 還須修改
export async function apiGetWaterEmbankAlarm(): Promise<
    SensorGeneralQueryDataResponse[]
    > {
    //alert("apiGetWaterEmbankAlarm");
    const response = await apiClient.post<SensorGeneralQueryDataResponse[]>(
        "Sensor/GetSensorGeneralQueryData?parameters=%2523%2523",
        {},
    );
    //alert(response);
    //alert(response.data);
    return response.data;
}

export interface CameraByAreaIdResponse {
  camId: string;
  camName: string;
  areaId: string;
  areaName: string;
  stationID: string;
  stationNameA: string;
  streamMain: string;
}

export async function apiGetCameraByAreaId(
  payload: string,
): Promise<CameraByAreaIdResponse[]> {
  const response = await apiClient.post<CameraByAreaIdResponse[]>(
    `/VideoImage/GetCameraByAreaId?areaId=${payload}`,
    {},
  );
  return response.data;
}

export interface WaterSensorTypeResponse {
  sensorType: string;
  sensorTypeName: string;
  sensorTypeSimple: string;
  unit: string;
}

export async function apiGetWaterSensorType(): Promise<
  WaterSensorTypeResponse[]
> {
  const response = await apiClient.get<WaterSensorTypeResponse[]>(
    '/Sensor/GetWaterSensorType',
    {},
  );
  return response.data;
}

export interface SensorGeneralQueryDataResponse {
  areaName: string;
  stationName: string;
  sensorId: string;
  sensorType: string;
  sensorTypeName: string;
  sensorName: string;
  sensorStatus: string ;
  lastDataTime: string;
  gps: string ;
  gpsLink: string ;
    value: string;
    differ1: string;
  differ2: string;
  direction: string;
  more: string;
  serialNo: string;
  unit: string;
  x: string;
  y: string;
  fIndex: string;
  eventTag: string;
  eqGrade: string;
  status: string;
  userType: string;
}

export async function apiGetSensorGeneralQueryData(
  payload: string,
): Promise<SensorGeneralQueryDataResponse[]> {
   // alert(payload);
  const response = await apiClient.post<SensorGeneralQueryDataResponse[]>(
    `/Sensor/GetSensorGeneralQueryData?userGroupId=0&parameters=${payload}`,
    {},
  );
  return response.data;
}

export interface GetSensorChartDataRequest {
  sensorId: string;
  begin: string;
  end: string;
  duration: number;
  backgroundColorValue1: string;
  borderColorValue1: string;
  borderWidthValue1: number;
  backgroundColorValue2: string;
  borderColorValue2: string;
  borderWidthValue2: number;
  backgroundColorLevel1: string;
  borderColorLevel1: string;
  borderWidthLevel1: number;
  backgroundColorLevel2: string;
  borderColorLevel2: string;
  borderWidthLevel2: number;
  backgroundColorLevel3: string;
  borderColorLevel3: string;
  borderWidthLevel3: number;
}

interface Data {
  x: string;
  y: number;
}

interface LstData {
  label: string;
  backgroundColor: null;
  borderColor: null;
  borderWidth: number;
  fill: false;
  data: Data[];
}
//tag1130826
export interface GetSensorChartDataResponse {
  main: string;
  sensorId: string;
  chartTitle: string;
  xLabel: string;
  yLabel: string;
  lstData: LstData[];
}

export async function apiGetSensorChartData(
  payload: GetSensorChartDataRequest,
): Promise<GetSensorChartDataResponse> {
    //tag1130826
    //alert(payload.); payload 如何產生
  const response = await apiClient.post<GetSensorChartDataResponse>(
    `/Sensor/GetSensorChartData`,
    payload,
  );
  console.log(response.data);
  return response.data;
}

export async function apiGetSensorMoreDataByStationName(
    payload: string,
): Promise<string> {
    const encodedText = encodeURIComponent(payload);
    const response = await apiClient.post<string>(
        `/Sensor/GetSensorMoreDataByStationName?stationName=${encodedText}`,
        {},
    );

    return response.data;
}
