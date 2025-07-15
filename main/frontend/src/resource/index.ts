import router from '@/router';
import store from '@/store';
import axios from 'axios';

export const apiClient = axios.create({
  baseURL: process.env.VUE_APP_DEBUG == 1
    ?  process.env.VUE_APP_API_URL_DEBUG
    :  process.env.VUE_APP_API_URL
});

apiClient.interceptors.request.use(
  function (config) {
    const userToken = (store.state as any).user.token;
    if (userToken) {
      config.headers.Authorization = `Bearer ${userToken}`;
    }
    return config;
  },
  function (error) {
    return Promise.reject(error);
  },
);

apiClient.interceptors.response.use(
  function (response) {
    return response;
  },
  function (error) {
      if (error.response && error.response.status === 401) {

      router.push('/login');
    }
    return Promise.reject(error);
  },
);
