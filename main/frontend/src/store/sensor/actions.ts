import {
    apiGetWaterSensorArea, apiGetWaterSensorAreaCCTV,
  apiGetWaterSensorType,
} from '@/resource/sensor';
import * as actionTypes from './actionTypes';
import * as mutationTypes from './mutationTypes';

export const actions = {
  async [actionTypes.GET_WATER_SENSOR_AREA_LIST](context: any) {
    try {
        const response = await apiGetWaterSensorAreaCCTV();
      if (!response) return;

      context.commit(mutationTypes.UPDATE_WATER_SENSOR_AREA_LIST, response);
      return response;
    } catch (err) {
      console.error(err);
    }
    }, async [actionTypes.GET_WATER_SENSOR_AREA_LIST2](context: any) {
        try {
            const response = await apiGetWaterSensorArea();
            if (!response) return;

            context.commit(mutationTypes.UPDATE_WATER_SENSOR_AREA_LIST2, response);
            return response;
        } catch (err) {
            console.error(err);
        }
    },
  async [actionTypes.GET_WATER_SENSOR_TYPE_LIST](context: any) {
    try {
      const response = await apiGetWaterSensorType();
      if (!response) return;

      context.commit(mutationTypes.UPDATE_WATER_SENSOR_TYPE_LIST, response);
      return response;
    } catch (err) {
      console.error(err);
    }
  },
};
