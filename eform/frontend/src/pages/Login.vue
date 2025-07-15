<template>
    <br/>
    <br/>
    <br/>
    <br/>
    <br/>
    <br/>
    <v-container class="fill-height" fluid>
      <v-row align="center" justify="center">
        <v-col cols="12" sm="8" md="6" lg="4" xl="3">
          <v-card class="mx-auto" max-width="400">
            <v-card-title class="text-h5">登入系統</v-card-title>
            <v-card-text>
              <v-form ref="form">
                <v-text-field
                  label="帳號"
                  prepend-icon="mdi-account"
                  type="text"
                  v-model="account"
                  required
                ></v-text-field>
                <v-text-field
                  label="密碼"
                  prepend-icon="mdi-lock"
                  type="password"
                  v-model="password"
                  required
                ></v-text-field>
              </v-form>
            </v-card-text>
            <v-card-actions>
              <v-spacer></v-spacer>
              <v-btn color="white" @click="handleLogin2" style="background-color: cornflowerblue; min-width:150px; max-width: 400px; margin-right: 10px; margin-bottom: 20px;"  >登入</v-btn>
              
            </v-card-actions>
            
          </v-card>

        </v-col>
        
      </v-row>
      <br/>
    </v-container>
  </template>
  
  <script>
  import { ref } from 'vue';
  import { useAuthStore } from '../stores/auth';
  import { useRouter } from 'vue-router';

  import { reactive } from "vue";
  import router from "../router"; 
  
  

 

  export default {
    //inheritAttrs:false,
    name: 'Login',
    data() {
      return {
        account: '',
        password: '',
      };
    },
    setup() {
      
    },
    mounted()
    {
      console.log('mounted');
      const authStore = useAuthStore();//if needs login comment these 3 lines
      authStore.loginId="su";
      router.push('/');
    },
    methods:{

      async handleLogin2() {
        //return true;
        const authStore = useAuthStore();
        //const router = useRouter();
      console.log(this.account+','+ this.password);
      const success = await authStore.login(this.account, this.password);
        if (success) {
          router.push('/');
        } else {
          console.error('Login failed');
        }
      
      },
    },
    
  };
  
  </script>
  
  <style scoped>
  .fill-height {
    height: 100vh;
  }
  </style>
  