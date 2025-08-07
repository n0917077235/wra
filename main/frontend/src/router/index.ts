import store from '@/store';
import { createRouter, createWebHistory, RouteRecordRaw } from 'vue-router';
import identity from './identity';
const routes: Array<RouteRecordRaw> = [
  {
    path: '/',
    component: () =>
      import(
        /* webpackChunkName: "base-layout" */ '@/views/Layouts/BaseLayout.vue'
      ),
    meta: {
      requiresAuth: true,
    },
    children: [
      {
        path: 'cctv',
        name: 'cctv',
        component: () =>
          import(/* webpackChunkName: "cctv" */ '@/views/CCTV/index.vue'),
        meta: {
            routerName: '監視站總覽',
            requiresAuth: true
        },
      },
      {
        path: 'gmap',
        name: 'gmap',
        component: () =>
          import(/* webpackChunkName: "gmap" */ '@/views/gmap.vue'),
        meta: {
          clearMainPadding: 1,
            routerName: '監測站總覽',
            requiresAuth: true
        },
      },
      {
        path: 'test1',
        name: 'test1',
        component: () =>
          import(/* webpackChunkName: "test1" */ '@/views/TEST1/index.vue'),
        meta: {
          clearMainPadding: 1,
            routerName: '員山子總覽',
            requiresAuth: true
        },
      },
      {
        path: 'test2',
        name: 'test2',
        component: () =>
          import(/* webpackChunkName: "test2" */ '@/views/TEST2/index.vue'),
        meta: {
          clearMainPadding: 1,
            routerName: '龍壽總覽',
            requiresAuth: true
        },
      },
      {
        path: 'test3',
        name: 'test3',
        component: () =>
          import(/* webpackChunkName: "test3" */ '@/views/TEST3/index.vue'),
        meta: {
          clearMainPadding: 1,
            routerName: '地圖總覽',
            requiresAuth: true
        },
      },
      {
        path: 'test4',
        name: 'test4',
        component: () =>
          import(/* webpackChunkName: "test4" */ '@/views/TEST4/index.vue'),
        meta: {
          clearMainPadding: 1,
            routerName: '雙北橫移門啟閉',
            requiresAuth: true
        },
      },
      {
        path: 'test5',
        name: 'test5',
        component: () =>
          import(/* webpackChunkName: "test5" */ '@/views/TEST5/index.vue'),
        meta: {
          clearMainPadding: 1,
            routerName: '淡水河水位',
            requiresAuth: true
        },
      },
      {
        path: 'test6',
        name: 'test6',
        component: () =>
          import(/* webpackChunkName: "test6" */ '@/views/TEST6/index.vue'),
        meta: {
          clearMainPadding: 1,
            routerName: '員山子水位',
            requiresAuth: true
        },
      },
      {
        path: 'test7',
        name: 'test7',
        component: () =>
          import(/* webpackChunkName: "test7" */ '@/views/TEST7/index.vue'),
        meta: {
          clearMainPadding: 1,
            routerName: '目視檢查成果',
            requiresAuth: true
        },
      },
      {
        path: 'test8',
        name: 'test8',
        component: () =>
          import(/* webpackChunkName: "test8" */ '@/views/TEST8/index.vue'),
        meta: {
          clearMainPadding: 1,
            routerName: '防水建造物圖資',
            requiresAuth: true
        },
      },
      {
        path: 'test9',
        name: 'test9',
        component: () =>
          import(/* webpackChunkName: "test9" */ '@/views/TEST9/index.vue'),
        meta: {
          clearMainPadding: 1,
            routerName: '透地雷達圖資',
            requiresAuth: true
        },
      },
      {
        path: 'test10',
        name: 'test10',
        component: () =>
          import(/* webpackChunkName: "test10" */ '@/views/TEST10/index.vue'),
        meta: {
          clearMainPadding: 1,
            routerName: '自訂套圖',
            requiresAuth: true
        },
      },
      {
        path: 'test10-1',
        name: 'test10-1',
        component: () =>
          import(/* webpackChunkName: "test10" */ '@/views/TEST10-1/index.vue'),
        meta: {
          clearMainPadding: 1,
            routerName: '1.1 地震儀總覽',
            requiresAuth: true
        },
      },
      {
        path: 'test10-2',
        name: 'test10-2',
        component: () =>
          import(/* webpackChunkName: "test10" */ '@/views/TEST10-2/index.vue'),
        meta: {
          clearMainPadding: 1,
            routerName: '1.1 員山子影像總覽',
            requiresAuth: true
        },
      },
      {
        path: 'test10-3',
        name: 'test10-3',
        component: () =>
          import(/* webpackChunkName: "test10" */ '@/views/TEST10-3/index.vue'),
        meta: {
          clearMainPadding: 1,
            routerName: '1.2 橫移門影像總覽', requiresAuth: true
        },
        },
        {
            path: 'test10-4',
            name: 'test10-4',
            component: () =>
                import(/* webpackChunkName: "test10" */ '@/views/TEST10-4/index.vue'),
            meta: {
                clearMainPadding: 1,
                routerName: '2.1 沉陷計-(1/2)', requiresAuth: true
            },
        },
        {
            path: 'test10-5',
            name: 'test10-5',
            component: () =>
                import(/* webpackChunkName: "test10" */ '@/views/TEST10-5/index.vue'),
            meta: {
                clearMainPadding: 1,
                routerName: '2.2 沉陷計-(2/2)', requiresAuth: true
            },
        },
        {
            path: 'test10-6',
            name: 'test10-6',
            component: () =>
                import(/* webpackChunkName: "test10" */ '@/views/TEST10-6/index.vue'),
            meta: {
                clearMainPadding: 1,
                routerName: '2.3 裂縫計-全', requiresAuth: true
            },
        },
        {
            path: 'test10-7',
            name: 'test10-7',
            component: () =>
                import(/* webpackChunkName: "test10" */ '@/views/TEST10-7/index.vue'),
            meta: {
                clearMainPadding: 1,
                routerName: '2.4 傾斜計-(1/3)', requiresAuth: true
            },
        },
        {
            path: 'test10-8',
            name: 'test10-8',
            component: () =>
                import(/* webpackChunkName: "test10" */ '@/views/TEST10-8/index.vue'),
            meta: {
                clearMainPadding: 1,
                routerName: '2.5 傾斜計-(2/3)', requiresAuth: true
            },
        },
        {
            path: 'test10-9',
            name: 'test10-9',
            component: () =>
                import(/* webpackChunkName: "test10" */ '@/views/TEST10-9/index.vue'),
            meta: {
                clearMainPadding: 1,
                routerName: '2.6 傾斜計-(3/3)', requiresAuth: true
            },
        },
        {
            path: 'test10-10',
            name: 'test10-10',
            component: () =>
                import(/* webpackChunkName: "test10" */ '@/views/TEST10-10/index.vue'),
            meta: {
                clearMainPadding: 1,
                routerName: '3.1 監測站影像', requiresAuth: true
            },
        },
        {
            path: 'test10-11',
            name: 'test10-11',
            component: () =>
                import(/* webpackChunkName: "test10" */ '@/views/TEST10-11/index.vue'),
            meta: {
                clearMainPadding: 1,
                routerName: '3.2 規劃課影像', requiresAuth: true
            },
        },
        {
            path: 'test10-12',
            name: 'test10-12',
            component: () =>
                import(/* webpackChunkName: "test10" */ '@/views/TEST10-12/index.vue'),
            meta: {
                clearMainPadding: 1,
                routerName: '3.3 水利署監測站', requiresAuth: true
            },
        },
      {
        path: 'Earthquakes',
        name: 'Earthquakes',
        component: () =>
          import(
            /* webpackChunkName: "test10" */ '@/views/Earthquakes/index.vue'
          ),
        meta: {
          clearMainPadding: 1,
            routerName: '地震儀總覽', requiresAuth: true
        },
      },
      {
        path: 'test11',
        name: 'test11',
        component: () =>
          import(/* webpackChunkName: "test11" */ '@/views/TEST11/index.vue'),
        meta: {
          clearMainPadding: 1,
            routerName: '文件下載', requiresAuth: true
        },
      },
      {
        path: 'search',
        name: 'search',
        component: () =>
          import(
            /* webpackChunkName: "cctv-search" */ '@/views/CCTVSearch/index.vue'
          ),
        meta: {
            routerName: '監控資料查詢', requiresAuth: true
        },
        children: [
          {
            path: '',
            name: 'search-page',
            redirect: '/search/monitor',
          },
          {
            path: 'monitor',
            name: 'monitor',
            component: () =>
              import(
                /* webpackChunkName: "cctv-search-monitor" */ '@/views/CCTVSearch/Monitor/index.vue'
                  ),
              meta: {
                  requiresAuth: true
              },
          },
          {
            path: 'sensor',
            name: 'sensor',
            component: () =>
              import(
                /* webpackChunkName: "cctv-search-sensor" */ '@/views/CCTVSearch/Sensor/index.vue'
                  ),
              meta: {
                  requiresAuth: true
              },
          },
        ],
      },
    ],
  },
  {
    path: '/',
    component: () =>
      import(
        /* webpackChunkName: "login-layout" */ '@/views/Layouts/LoginLayout.vue'
      ),
    meta: {},
    children: [
      {
        path: 'login',
        name: 'login',
        component: () =>
          import(/* webpackChunkName: "login" */ '@/views/Login/index.vue'),
      },
    ],
  },
];

const router = createRouter({
  history: createWebHistory(process.env.BASE_URL),
  routes,
});


router.beforeEach((to, from, next) => {
  //判斷版本是否一致 不一致則更新
  //versionTood.isNewVersion();
    // 確保在每次路由變更前檢查登入狀態
    const isAuthenticated = store.getters['user/isAuthenticated'];
  if (typeof to.meta?.routerName === 'string') {
    document.title = '淡水河流域監測系統';
  } else if (to.name === 'login') {
    document.title = '淡水河流域監測系統';
  }
    if (to.matched.some(record => record.meta.requiresAuth)) {
        // 需要登入的頁面
        if (isAuthenticated) {
            next(); // 允許進入頁面
        } else {
            next('/login'); // 重定向到登入頁面
        }
    } else {
        next(); // 允許進入非需登入頁面
    }
});

export default router;
identity.authorize();

