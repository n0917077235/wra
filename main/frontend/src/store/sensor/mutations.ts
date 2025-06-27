import {
  WaterSensorAreaResponse,
  WaterSensorTypeResponse,
} from '@/resource/sensor';
import * as mutationTypes from './mutationTypes';
import { State } from './state';

export const mutations = {
  [mutationTypes.UPDATE_WATER_SENSOR_AREA_LIST](
    state: State,
    array: WaterSensorAreaResponse[],
  ) {
    state.waterSensorAreaList = array;
    },
    [mutationTypes.UPDATE_WATER_SENSOR_AREA_LIST2](
        state: State,
        array: WaterSensorAreaResponse[],
    ) {
        state.waterSensorAreaList = array;
    },
  [mutationTypes.UPDATE_WATER_SENSOR_TYPE_LIST](
    state: State,
    array: WaterSensorTypeResponse[],
  ) {
    state.waterSensorTypeList = array;
  },
};
