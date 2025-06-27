<template>
  <div class="flex h-screen flex-col rounded-r-lg bg-primary text-white">
    <ToggleLogo></ToggleLogo>
    <div class="h-screen">
        <el-menu :collapse="isCollapse"
                 :default-active="defaultActiveMenu"
                 router>
            <div v-for="menu in menus" :key="menu.index">
                <SubMenuItem v-if="menu.hasChildren"
                             :index="menu.index"
                             :icon-name="menu.iconName"
                             :page-name="menu.pageName"
                             :router-children="menu.children ?? []"></SubMenuItem>
                <MenuItem v-else
                          :index="menu.index"
                          :icon-name="menu.iconName"
                          :page-name="menu.pageName"></MenuItem>
            </div>
            <logout></logout>
        </el-menu>
    </div>
  </div>
</template>

<script lang="ts" setup>
import { computed, inject, reactive } from 'vue';
import { useRoute } from 'vue-router';
import Logout from './Logout.vue';
import MenuItem from './MenuItem.vue';
import SubMenuItem from './SubMenuItem.vue';
import ToggleLogo from './ToggleLogo.vue';

const route = useRoute();

const defaultActiveMenu = computed<string>(() => route.matched[1]?.path);

const isCollapse = inject<boolean>('isCollapse');

interface MainItem {
  index: string;
  iconName: string;
  pageName: string;
  hasChildren?: boolean;
  children?: ChildItem[];
}

interface ChildItem {
  index: string;
  name: string;
}

const menus = reactive<MainItem[]>([
  { index: '/cctv', iconName: 'icon_cctv', pageName: '監視站總覽' },
  { index: '/search', iconName: 'icon_search', pageName: '監控資料查詢' },
  { index: '/gis', iconName: 'icon_zones', pageName: '監測站總覽' },
  { index: '/test1', iconName: '', pageName: '員山子總覽' },
  { index: '/test2', iconName: '', pageName: '龍壽總覽' },
  { index: '/test3', iconName: '', pageName: '地圖總覽' },
  { index: '/test4', iconName: '', pageName: '雙北橫移門啟閉' },
    { index: '/test5', iconName: 'icon_waterLevel', pageName: '淡水河水位' },
    { index: '/test6', iconName: 'icon_waterLevel', pageName: '員山子水位' },
  { index: '/test7', iconName: '', pageName: '目視檢查成果' },
  { index: '/test8', iconName: '', pageName: '防水建造物圖資' },
  { index: '/test9', iconName: '', pageName: '透地雷達圖資' },
    { index: '/Earthquakes', iconName: 'icon_search', pageName: '地震儀總覽' },
  {
    index: '/test10',
    iconName: '',
    pageName: '自訂套圖',
    hasChildren: true,
    children: [
      //{ index: '/test10-1', name: '1.1地震儀總覽' },
        { index: '/test10-2', name: '1.1 員山子影像總覽' },
        { index: '/test10-3', name: '1.2 橫移門影像總覽' },
        { index: '/test10-4', name: '2.1 沉陷計-(1/2)' },
        { index: '/test10-5', name: '2.2 沉陷計-(2/2)' },
        { index: '/test10-6', name: '2.3 裂縫計-全' },
        { index: '/test10-7', name: '2.4 傾斜計-(1/3)' },
        { index: '/test10-8', name: '2.5 傾斜計-(2/3)' },
        { index: '/test10-9', name: '2.6 傾斜計-(3/3)' },
        { index: '/test10-10', name: '3.1 監測站影像' },
        { index: '/test10-11', name: '3.2 規劃課影像' },
        { index: '/test10-12', name: '3.3 水利署監測站' },
    ],
  },
    { index: '/test11', iconName: 'icon_downlaod', pageName: '文件下載' },
]);
</script>
