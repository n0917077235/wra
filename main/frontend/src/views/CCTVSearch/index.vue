<template>
  <div class="search">
    <div class="search-tabs flex">
      <router-link to="/search/monitor">
        <div
          class="mr-4 w-[80px] pb-2 text-center"
          :class="{ active: activeName === 'monitor' }"
        >
          監視器
        </div>
      </router-link>
      <router-link to="/search/sensor">
        <div
          class="mr-4 w-[80px] pb-2 text-center"
          :class="{ active: activeName === 'sensor' }"
        >
          感測器
        </div>
      </router-link>
    </div>
    <div class="search-main p-[16px]">
      <keep-alive>
        <router-view></router-view>
      </keep-alive>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue';
import { RouteRecordName, useRoute } from 'vue-router';

const route = useRoute();
const activeName = computed<RouteRecordName>(() => route.name ?? '');
</script>

<style lang="scss" scoped>
.search {
  &-tabs {
    .active {
      color: $primary;
      border-bottom: 2px solid $primary;
    }
  }
  &-main {
    border-radius: 0px 12px 0px 0px;
    box-shadow: -0.5px 0.5px 3px 0px rgba(0, 0, 0, 0.15);
  }
}
</style>
