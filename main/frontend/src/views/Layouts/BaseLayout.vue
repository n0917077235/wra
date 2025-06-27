<template>
  <div class="common-layout">
    <el-container>
      <el-aside :width="asideWidth">
        <the-menu></the-menu>
      </el-aside>

      <el-container class="h-screen">
        <el-header class="z-[10000]">
          <the-header @change-collapse="handleCollapseChange"> </the-header>
        </el-header>

        <el-drawer
          v-model="openDrawer"
          :with-header="false"
          direction="ltr"
          size="300px"
        >
          <the-menu></the-menu>
        </el-drawer>

        <el-main :class="{ 'p-0': clearMainPadding }">
          <router-view></router-view>
        </el-main>
      </el-container>
    </el-container>
  </div>
</template>

<script lang="ts" setup>
import TheHeader from '@/components/Layout/TheHeader/index.vue';
import TheMenu from '@/components/Layout/TheMenu/index.vue';
import { computed, provide, ref, watch } from 'vue';
import { useRoute } from 'vue-router';

const route = useRoute();

const matches = ref<boolean>(false);
const isCollapse = ref<boolean>(false);
const openDrawer = ref<boolean>(false);

const handleCollapseChange = (value: boolean): void => {
    isCollapse.value = value;
    //alert("handleCollapseChange");
};

const handleMenuDrawer = (): void => {
  openDrawer.value = !openDrawer.value;
};

provide('openDrawer', openDrawer);
provide('isCollapse', isCollapse);
provide('handleCollapse', handleCollapseChange);
provide('handleMenuDrawer', handleMenuDrawer);

const asideWidth = computed<string>(() => {
  if (matches.value) {
    return '0';
  }

  if (isCollapse.value) {
    return '82px';
  }

  return '300px';
});
const clearMainPadding = computed<boolean>(() => {
  return (route.matched[1]?.meta?.clearMainPadding || false) as boolean;
});
/* eslint-disable */
//add 1130802
    watch(
        () => isCollapse.value,
        () => {
            if (window.TGOS && window.pMap) {
                //alert("2" + window.pMap.weight);
                var n = window.pMap.getCenter();
                window.pMap.setCenter(n);
                window.pMap.fitBounds(window.pMap.getBounds());
                //window.pMap.resize();
                //window.resize
            }
        },
    );
/* eslint-disable */
watch(
  () => matches.value,
  () => {
    isCollapse.value = false;
    openDrawer.value = false;
  },
);

const onResize = (): void => {
    matches.value = window.matchMedia('(max-width: 767px)').matches;

};

onResize();
window.addEventListener('resize', onResize);
</script>
