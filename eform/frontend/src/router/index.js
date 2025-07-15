
import { createRouter, createWebHistory } from 'vue-router';
import MaintainForm from "../pages/MaintainForm.vue";
import EditComment from "../pages/EditComment.vue";
import EditSchedule from '../pages/EditSchedule.vue';
import EditSheet from '../pages/EditSheet.vue';
import MaintainQuery from '../pages/MaintainQuery.vue'; //parent
import ChildComponent from '../pages/ChildComponent.vue'; //child
import ScheduleQuery from '../pages/ScheduleQuery.vue';
import MaintainList from '../pages/MaintainList.vue';
//import EditSchedule2 from '../pages/EditSchedule2.vue';

//import MaintainForm2 from "../pages/MaintainForm.vue";
import Login from "../pages/Login.vue";
import Logout from "../pages/Logout.vue";
import TestLayout from "../pages/TestLayout2.vue";
import { useAuthStore } from '../stores/auth';

const routes = [
    {
      path: "/maintainform",
      name: "MaintainForm1",
      component: MaintainForm,
    },
    {
      path: "/editschedule",
      name: "EditSchedule",
      component: EditSchedule,
    },
    {
      path: "/editcomment",
      name: "EditComment",
      component: EditComment,
    },
    {
      path: "/",
      name: "HomePage",
      component:  MaintainForm,
    },
    {
      path: "/Login",
      name: "Login",
      component:  Login,
    },
    {
      path: "/Logout",
      name: "Logout",
      component:  Logout,
    },
    {
      path: "/TestLayout",
      name: "TestLayout",
      component:  TestLayout,
    },
    {
      path: "/EditSheet",
      name: "EditSheet",
      component:  EditSheet,
    },
    
    {
      path: "/MaintainQuery",
      name: "MaintainQuery",
      component:  MaintainQuery,
      children: [
        {
          path: ':tableType/:recordTime',
          name: 'MaintainFormHistory',
          component: ChildComponent,
          props: true, 
        }
      ],
      
    },
    /*
    {
      path: "/MaintainFormHistory",
      name: "MaintainFormHistory",
      component:  MaintainFormHistory,
    },
    */
    {
      path: "/ScheduleQuery",
      name: "ScheduleQuery",
      component:  ScheduleQuery,
    },
    {
      path: "/MaintainList",
      name: "MaintainList",
      component:  MaintainList,
    },
    { 
      path: '/:catchAll(.*)', 
      redirect: '/maintain' }
  ];

  const router = createRouter({
    history: createWebHistory('/maintain'),
    routes,
    mode: 'hash'
  });

  
  router.beforeEach((to, from, next) => {
    console.log('from',from.path);
    //if (from.path.indexOf('maintainquery')!=-1)
    //  return;
    const authStore = useAuthStore();
    console.log('to',to.path);
    console.log('authStore',authStore.isAuthenticated);

    if (to.path !== '/login' && !authStore.isAuthenticated) {
      next('/login');
    } else {
      next();
    }
  });
  
  export default router;

  