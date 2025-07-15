/**
 * main.js
 *
 * Bootstraps Vuetify and other plugins then mounts the App`
 */

// Plugins
import { registerPlugins } from '@/plugins'
import 'vuetify/styles';
// Components
import App from './App.vue'
import "vuetify/dist/vuetify.min.css";
import router from './router';
// Composables
//import { createApp } from 'vue'
import { createApp } from 'vue/dist/vue.esm-bundler';
import { createI18n } from 'vue-i18n';
import i18n from './i18n';
import { createVuetify } from 'vuetify';
import * as components from 'vuetify/components';
import * as directives from 'vuetify/directives';
import { createPinia } from 'pinia';


//vue.config.productionTip = false;




const vuetify = createVuetify({
  components,
    directives,
    theme: {
      defaultTheme: 'light',
  },
});


const pinia = createPinia();
/*
fetch('public/config.js')
  .then(response => response.text())
  .then(text => {
    eval(text); 
    console.log('eval',text);
    const app = createApp(App)
    app.use(router);
    app.use(i18n);
    app.use(vuetify);
    app.use(pinia);
    registerPlugins(app);

    app.mount('#app');
  })
  .catch(error => {
    console.error('Error loading config.js:', error);
    
  });
*/

//const apiUrl = window.config.API_URL;
//console.log('apiurl',apiUrl);
const app = createApp(App)
app.use(router);
app.use(i18n);
app.use(vuetify);
app.use(pinia);
registerPlugins(app)

app.mount('#app')

