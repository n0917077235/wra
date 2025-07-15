import { defineStore } from 'pinia';
import axios from 'axios';

const env = import.meta.env;
const apiUrl = (env.VITE_APP_API_ENDPOINT);

export const useAuthStore = defineStore('auth', {
  state: () => ({
    loginId: null,
    loginTime: null,
    userType:null,
    token:null,
    user: null,
  }),
  actions: {
    async login(userId, password) {
      try {
        const response = await axios.post(`${apiUrl}/api/Login/login`, {
          userId,
          password,
        });
        console.log(response.data);
        if (response.data.userId !=="undefined") {
          this.loginId = response.data.userId; // Assuming the response includes the user ID
          this.loginTime = new Date().toISOString();
          this.userType = response.data.userType;
          this.token=response.data.token;
          console.log('auth:',response.data);
          return true;
        } else {
          return false;
        }
      } catch (error) {
        console.error('An error occurred during login:', error);
        return false;
      }
    },
    logout() {
      this.loginId = null;
      this.loginTime = null;
      this.userType=null;
      this.token=null;
    },
  },
  getters: {
    isAuthenticated: (state) => !!state.loginId,
    getLoginId: (state) => state.loginId,
    getLoginTime: (state) => state.loginTime,
    getUserType:(state) => state.userType,
    getToken:(state) => state.token,
  },
});
