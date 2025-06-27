<template>
  <div id="app">
    <CCTVMap :title="''"></CCTVMap>
  </div>
</template>

<script setup lang="ts">
import CCTVMap from '@/components/CCTV/CCTVMap2.vue';
</script>

<style lang="scss" scoped>
    #app {
        height: calc(100vh - 80px);
        width: 100%;
        //display: flex;

        margin: 0;
        padding: 0;
    }
//.cctv-map {
//  height: calc(100vh - 58px) !important;
//}
</style>
