import { CameraByAreaIdSimpleResponse } from '@/resource/cctv';

export interface State {
  cameraAreaList: CameraByAreaIdSimpleResponse[];
  currentCameraArea: CameraByAreaIdSimpleResponse;
}
export const state: State = {
  cameraAreaList: [],
  currentCameraArea: {
    stationID: '',
    stationNameA: '',
    camName: '',
    streamMain: '',
    x: '',
    y: '',
  },
};
