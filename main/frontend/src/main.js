import Vue from 'vue';
import App from './App.vue';
import jQuery from 'jquery';
//import versionUpdate from '@/libs/versionUpdate';
import router from './router';
import store from './store';

import ElementPlus from 'element-plus' //11308
app.use(ElementPlus)
//北 1130717
//versionUpdate.isNewVersion();

Object.assign(window, { $: jQuery, jQuery });

const app = createApp(App);
app.config.globalProperties.$productionTip = true;


//北 1130717
app.use(router);
app.use(stroe);

app.mount('#app');

// 絋玂 TGOS 更
if (typeof TGOS === 'undefined') {
    console.error('TGOS API ゼ更');
} else {
    console.log('TGOS API 更');
}