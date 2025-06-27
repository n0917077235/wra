<template>
  <div>
    <CCTVMap :title="''"></CCTVMap>
  </div>
</template>

<script setup lang="ts">
import CCTVMap from '@/components/CCTV/CCTVMap.vue';
</script>

<style lang="scss" scoped>
:deep .cctv-map {
  height: calc(100vh - 58px) !important;
}
</style>
