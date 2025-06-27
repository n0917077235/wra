import { ActionTree } from 'vuex';
import { State } from './state';
//¥H¤W1130828
import { LoginRequest, apiLogin, apiLogout } from '@/resource/login';
import * as actionTypes from './actionTypes';
import * as mutationTypes from './mutationTypes';

export const actions = {
  async [actionTypes.LOGIN](context: any, payload: LoginRequest) {
    try {
      const response = await apiLogin(payload);
      if (!response) return;

      context.commit(mutationTypes.UPDATE_USER_ID, response.userId);
      context.commit(mutationTypes.UPDATE_USER_NAME, response.userName);
      context.commit(mutationTypes.UPDATE_TOKEN, response.token);
      return response;
    } catch (err) {
      console.error(err);
      alert('Login failed');
    }
  },
  async [actionTypes.LOGOUT](context: any, payload: string) {
    try {
      const response = await apiLogout(payload);
      if (!response) return;

      context.commit(mutationTypes.UPDATE_USER_ID, '');
      context.commit(mutationTypes.UPDATE_USER_NAME, '');
      context.commit(mutationTypes.UPDATE_TOKEN, '');
      return response;
    } catch (err) {
      console.error(err);
      alert('Logout failed');
    }
  },
};
