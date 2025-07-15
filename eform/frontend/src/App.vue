<template>
  <div id="app">
  <v-app id="inspire">
    
    <v-app-bar app clipped-left dense color="indigo" dark>
      <v-app-bar-nav-icon @click.stop="drawer = !drawer"></v-app-bar-nav-icon>
      <v-toolbar-title>淡水河流域監測系統維護電子表單子系統</v-toolbar-title>
    </v-app-bar>
    <v-navigation-drawer v-model="drawer" app clipped :width="drawerWidth" >
  
        <v-list dense nav >
          <v-list-item 
            v-for="item in items"
            :key="item.title"
            link
            @click="navigate(item.route)">
            
            <!--v-list-item :prepend-icon="item.icon" :title="item.title" :class="customClass" /-->
            <v-list-tile-action>
              <v-icon style="margin-left:1px; margin-bottom:5px;">{{ item.icon }}</v-icon>
            </v-list-tile-action>
  
            <v-list-tile-content>
              <v-list-tile-title style="margin-left:8px; font-size:18px;">{{ item.title }}</v-list-tile-title>
            </v-list-tile-content>
          </v-list-item>
        </v-list>
    </v-navigation-drawer>
  <div>
  <div>
        <router-view class="accent pa-0 align-start m" ></router-view>
  </div>
  </div>
  </v-app>
</div>
</template>

<script>

 export default {
    name: "App",
    
    
    data() {
      return {
        drawerWidth: 300,
        drawer: false,
        mini:false,
        customClass: 'menu-text',
      items: [
        { title: '首頁', icon: 'mdi-home' ,route:'/'},
        { title: '維護表單 [新增案件]', icon: 'mdi-file-document-edit-outline' , route:'/maintainform'},
        //{ title: '維護表單 [備註欄管理]', icon: 'mdi-file-document-outline' , route:'/editcomment'},
        { title: '維護表單 [預定進度管理]', icon: 'mdi-file-table' ,route:'/editschedule'},
        { title: '維護表單 [預定進度查詢]', icon: 'mdi-file-table' ,route:'/scheduleQuery'},
        { title: '維護表單 [維護報表查詢]', icon: 'mdi-file-table' ,route:'/maintainlist'},
        //{ title: '維護表單 [歷史記錄]]', icon: 'mdi-file-table' ,route:'/maintainquery'},
        //{ title: '員山子 Test', icon: 'mdi-login-variant' ,route:'/testlayout'},
        { title: '登出', icon: 'mdi-login-variant' ,route:'/logout'},
        //{ title: 'TEST', icon: 'mdi-test-tube' ,route:'/testlayout'},
      ],
      right: null,
      no_results_text: "無資料"
      }
    },

    computed: {
     
    },
    methods: {
        navigate(route) {
        this.drawer = false; // Close the drawer after selection
        console.log('route',route);
        this.$router.push(route); 
        }
    }
  };
  
  
</script>

<style>
.main-content {
  margin-top: 80px; 
}
.menu-text {
  font-size: 20px; 
}
</style>
