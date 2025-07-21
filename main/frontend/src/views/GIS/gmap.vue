<template>
  <div id="app">
    <MyOlMap :title="''"></MyOlMap>
  </div>
</template>

<script setup lang="ts">
import MyOlMap from '@/components/CCTV/MyOlMap.vue';
</script>

<style lang="scss" scoped>
    #app {
        height: calc(100vh - 80px);
        width: 100%;
        margin: 0;
        padding: 0;
    }
</style>
