<template>
  <el-scrollbar class="cctv-list">
    <div class="flex flex-row lg:flex-col">
      <div
        class="mb-0 mr-4 flex items-center sm:mb-[12px]"
        v-for="cctv in cctvList"
        :key="cctv.stationID"
      >
        <el-card
          class="box-card mr-1 cursor-pointer"
          :class="{ 'box-active': active === cctv.stationID }"
          @click="updateCctvCamera(cctv)"
        >
          <template #header>{{ cctv.camName }}</template>
          <img :src="cctv.streamMain" alt="cctv" class="w-full object-cover" />
        </el-card>
        <div class="hidden w-[14px] lg:block">
          <el-icon
            :size="14"
            class="cursor-pointer"
            v-show="active === cctv.stationID"
          >
            <app-icon icon-name="icon_right"></app-icon>
          </el-icon>
        </div>
      </div>
    </div>
  </el-scrollbar>
</template>

<script setup lang="ts">
import { CameraByAreaIdSimpleResponse } from '@/resource/cctv';
import { computed, ref } from 'vue';
import { useStore } from 'vuex';

const store = useStore();
const cctvList = computed<CameraByAreaIdSimpleResponse[]>(
  () => store.state.image.cameraAreaList,
);
const active = ref<string>('');

const updateCctvCamera = (cctv: CameraByAreaIdSimpleResponse): void => {
  store.commit('image/UPDATE_CCTV_CAMERA', cctv);
};
</script>

<style lang="scss" scoped>
.cctv-list {
  height: calc(100% - 55px);
  @media (max-width: $lg) {
    height: 162px;
  }
  @media (max-width: $sm) {
    height: 100px;
  }
}
</style>
