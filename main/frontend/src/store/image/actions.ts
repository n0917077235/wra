import { apiGetCameraByAreaIdSimple } from '@/resource/cctv';
import * as actionTypes from './actionTypes';
import * as mutationTypes from './mutationTypes';

export const actions = {
  async [actionTypes.GET_CAMERA_AREA_SIMPLE_LIST](context: any, payload: any) {
    try {
      const response = await apiGetCameraByAreaIdSimple(payload);
      if (!response) return;

      context.commit(mutationTypes.UPDATE_CAMERA_AREA_SIMPLE_LIST, response);
      return response;
    } catch (err) {
      console.error(err);
    }
  },
};
