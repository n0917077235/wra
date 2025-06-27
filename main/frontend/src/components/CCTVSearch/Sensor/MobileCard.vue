<template>
  <div class="mobile-card mb-3 rounded">
    <div
      class="mobile-card-header flex justify-between bg-primary p-2 text-white"
    >
      <div>#{{ index }} {{ props.data.areaName }}</div>
      <div
        class="flex cursor-pointer items-center"
        @click="showDetail(props.data)"
      >
        更多
        <el-icon :size="12" class="ml-1">
          <app-icon icon-name="icon_down_arrow_white"></app-icon>
        </el-icon>
      </div>
    </div>
    <div class="p-2 text-sm text-black">
      <div>站點名稱: {{ props.data.stationName }}</div>
      <div>裝置名稱: {{ props.data.sensorName }}</div>
      <div>狀態: {{ props.data.status }}</div>
      <div>即時資訊時間: {{ props.data.lastDataTime }}</div>
      <div>量測值: {{ props.data.value }}</div>
      <div class="flex items-center">
        趨勢:
        <el-icon :size="12" class="ml-2">
          <app-icon :icon-name="setStatus(props.data.differ1)"></app-icon>
        </el-icon>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { SensorGeneralQueryDataResponse } from '@/resource/sensor';

interface Props {
  index: number;
  data: SensorGeneralQueryDataResponse;
}

const props = defineProps<Props>();

const emits = defineEmits(['showDetail']);

const setStatus = (value: number): string => {
  if (value > 0) {
    return 'icon_up';
  } else if (value < 0) {
    return 'icon_down';
  } else {
    return 'icon_no_diff';
  }
};

const showDetail = (row: SensorGeneralQueryDataResponse): void => {
  emits('showDetail', row);
};
</script>

<style lang="scss" scoped>
.mobile-card {
  box-shadow: -0.5px 0.5px 3px 0px rgba(0, 0, 0, 0.15);
  .mobile-card-header {
    border-radius: 4px 4px 0px 0px;
  }
}
</style>
