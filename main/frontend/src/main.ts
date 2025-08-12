import { createApp } from 'vue';
import App from './App.vue';
import router from './router';
import store from './store';

import { install } from '@/plugins';

import '@/styles/element/ol.scss';
import '@/styles/element/index.scss';
import '@/styles/preflight.scss';
import '@/styles/tailwind.scss';

import AppIcon from '@/components/AppIcon/index.vue';
import HoverInput from '@/components/Frame/HoverInput.vue';
import WraSelect from '@/components/Frame/WraSelect.vue';

// eslint-disable-next-line
const requireAll = (requireContext: any) =>
  requireContext.keys().map(requireContext);
const req = require.context('../src/assets/icon', true, /\.svg$/);
requireAll(req);

const app = createApp(App);

install(app);

app.use(store);
app.use(router);

app.component('AppIcon', AppIcon);
app.component('HoverInput', HoverInput);
app.component('WraSelect', WraSelect);

app.mount('#app');
