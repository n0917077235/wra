<template>
    <v-btn @click="logout" color="white" style="background-color: cornflowerblue; min-width:150px; max-width: 400px; margin-right: 10px; margin-bottom: 20px;">登出</v-btn>
  </template>
  
  <script>
  import { useAuthStore } from '../stores/auth';
  import { useRouter } from 'vue-router';
  import { onMounted } from 'vue';

  export default {
    name: 'LogoutButton',
    setup() {
      const authStore = useAuthStore();
      const router = useRouter();

      onMounted(() => {
        authStore.logout();
        router.push('/login'); 
      });

      return {};
    },
  };
  </script>
  