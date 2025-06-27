import { CameraByAreaIdSimpleResponse } from '@/resource/cctv';
import * as mutationTypes from './mutationTypes';
import { State } from './state';

export const mutations = {
  [mutationTypes.UPDATE_CAMERA_AREA_SIMPLE_LIST](
    state: State,
    array: CameraByAreaIdSimpleResponse[],
  ) {
    state.cameraAreaList = array;
  },

  [mutationTypes.UPDATE_CCTV_CAMERA](
    state: State,
    array: CameraByAreaIdSimpleResponse,
  ) {
    state.currentCameraArea.stationID = array.stationID;
    state.currentCameraArea.stationNameA = array.stationNameA;
    state.currentCameraArea.camName = array.camName;
    state.currentCameraArea.streamMain = array.streamMain;
    state.currentCameraArea.x = array.x;
    state.currentCameraArea.y = array.y;
  },
};
