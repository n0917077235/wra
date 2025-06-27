import { apiClient } from './index';

export async function apiGetGeoJsonFileNmae(): Promise<string>
{
    const response = await apiClient.get<string>(
        '/GeoJson/GetGeoJsonFileName',
        {},
    );
    return response.data;
}

export async function apiGetGeoJsonDataByFileName(fn: string): Promise<string> {
    const response = await apiClient.post<Promise<string>>(
        '/GeoJson/GetGeoJsonDataByFileName?fileName='+fn,
        {},
    );
    return response.data;
}

export async function apiGetTansuiGps(): Promise<string> {
    const response = await apiClient.get<string>(
        '/GeoJson/GetTansuiGps',
        {},
    );
    return response.data;
}

export async function apiGetGps(url: string): Promise<string> {
    let response;
    if (url.includes("/GeoJson/GetGeoJsonDataByFileName?fileName=")) {
        response = await apiClient.post<string>(
            url,
            {});
    } else {
        response = await apiClient.get<string>(
            url,
            {},
        );
    }
    return response.data;
}

//11309
export interface GetIsoseismalRequest {
    userId: string;
    eventTime: string;
}
export interface GetIsoseismalResponse {
    result: boolean;
    geoJson: string;
    imageUrl: string;
    infoList: Array<string>;
}
export async function apiGetIsoseismal(infos: GetIsoseismalRequest): Promise<GetIsoseismalResponse|null> {
    let response;
    try {
        response = await apiClient.post<GetIsoseismalResponse>('Earthquake/Isoseismal',
            infos);
        return response.data;
    } catch (e:any) { return null ; }
}

    export async function getGetStationGps(): Promise<string> {
        const response = await apiClient.get<string>(
            '/GeoJson/GetStationGps',
            {},
        );
        return response.data;
    }
export async function getGeoJsonLayer(): Promise<{ [key: string]: string }> {
    const fileName = await apiGetGeoJsonFileNmae();
    const data = await apiGetGeoJsonDataByFileName(fileName);
    return {
        layer4: data,
        layer5: data,
        layer6: data,
        layer7: data,
        layer8: data,
        layer9: data,
        layer10: data,
        layer11: data,
        layer12: data,
        layer13: data,
        layer14: data,
        layer15: data,
        layer16: data,
        layer17: data,
        layer18: data,
        layer19: data,
        layer20: data,
        layer21: data,
        layer22: data,
        layer23: data,
        layer24: data,
        layer25: data,
        layer26: data,
    };
}
