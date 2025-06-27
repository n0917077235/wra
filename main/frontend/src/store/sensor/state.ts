import {
  WaterSensorAreaResponse,
  WaterSensorTypeResponse,
} from '@/resource/sensor';

export interface State {
  waterSensorAreaList: WaterSensorAreaResponse[];
  waterSensorTypeList: WaterSensorTypeResponse[];
}

export const state: State = {
  waterSensorAreaList: [],
  waterSensorTypeList: [],
};
