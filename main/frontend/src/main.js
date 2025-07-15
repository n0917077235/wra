import jQuery from 'jquery';
import App from './App.vue';
//import versionUpdate from '@/libs/versionUpdate';
import router from './router';

import ElementPlus from 'element-plus'; //11308
app.use(ElementPlus)
//版控 1130717
//versionUpdate.isNewVersion();

Object.assign(window, { $: jQuery, jQuery });

const app = createApp(App);
app.config.globalProperties.$productionTip = true;


//版控 1130717
app.use(router);
app.use(stroe);

app.mount('#app');

// 確保 TGOS 已加載
if (typeof TGOS === 'undefined') {
    console.error('TGOS API 未加載');
} else {
    console.log('TGOS API 已加載');
}