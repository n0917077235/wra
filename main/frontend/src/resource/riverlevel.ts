import { apiClient } from './index';

interface Area {
    AreaID: string;
    AreaName: string;
}

interface Sensor {
    SensorID: string;
    SensorNameA: string;
    LastDataTime: string;
    LastValue1: number;
    DataUnit: string;
    StationId: string;
    X: number;
    Y: number;
}

interface ApiResponse<T> {
    success: boolean;
    data: T;
    message?: string;
}

const RiverLevelAPI = {
    // 取得所有區域列表
    getAreaList: async (): Promise<ApiResponse<Area[]>> => {
        try {
            const response = await apiClient.get<ApiResponse<Area[]>>('/RiverLevel/arealist')
            return response.data
        } catch (error) {
            console.error('Error in getAreaList:', error)
            throw error
        }
    },

    // 根據區域ID取得水位感測器列表
    getSensorsByArea: async (areaId: string): Promise<ApiResponse<Sensor[]>> => {
        try {
            const response = await apiClient.post<ApiResponse<Sensor[]>>('/RiverLevel/sensors', {
                AreaID: areaId
            })
            return response.data
        } catch (error) {
            console.error('Error in getSensorsByArea:', error)
            throw error
        }
    }
}

export default RiverLevelAPI
