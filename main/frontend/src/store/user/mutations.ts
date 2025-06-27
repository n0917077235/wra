import * as mutationTypes from './mutationTypes';
import { State } from './state';

export const mutations = {
  [mutationTypes.UPDATE_USER_ID](state: State, id: string) {
    state.userId = id;
  },
  [mutationTypes.UPDATE_USER_NAME](state: State, userName: string) {
    state.userName = userName;
  },
  [mutationTypes.UPDATE_TOKEN](state: State, token: string) {
    state.token = token;
  },
};
