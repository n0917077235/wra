import router from '@/router';
import store from '@/store';
import { NavigationGuardNext, RouteLocationNormalized } from 'vue-router';

export default {
  authorize() {
    router.beforeEach(
      (
        to: RouteLocationNormalized,
        from: RouteLocationNormalized,
        next: NavigationGuardNext,
      ) => {
        if (to.matched.some((route) => route.meta?.requiresAuth)) {
          const userToken = (store.state as any).user.token;

          if (userToken) {
            next();
          } else {
            next('/login');
          }
        } else {
          next();
        }
      },
    );
  },
};
