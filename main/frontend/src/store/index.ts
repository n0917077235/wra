import { createStore } from 'vuex';
import createPersistedState from 'vuex-persistedstate';
import image from './image';
import sensor from './sensor';
import user from './user';
import drawings from './drawings';

export default createStore({
  modules: {
    user,
    sensor,
        image,
        drawings, // 確保這裡有正確注冊 drawings 模組
  },
  plugins: [
    createPersistedState({
      paths: ['user'],
    }),
  ],
});
