/* eslint-disable */
<template>
    <div class="cctv flex flex-col rounded-2xl p-[16px] lg:flex-row">
        <div class="mb-2 mr-[20px] w-[100%] lg:mb-0 lg:w-[20%] overflow-auto">
            <div class="mb-4 lg:mr-9">
                <el-form>
                    <wra-select v-model="select"
                                label="流域"
                                name="value"
                                :options="options"
                                value-name="areaId"
                                label-name="areaName"
                                class="w-full"></wra-select>
                </el-form>
            </div>
            <div>
                <CCTVList></CCTVList>
            </div>
        </div>
        <div class="flex-1 flex flex-col overflow-auto">
            <CCTVDetail v-if="hasData"></CCTVDetail>
        </div>
        </div>
</template>

<script setup lang="ts">
import CCTVDetail from '@/components/CCTV/CCTVDetail.vue';
import CCTVList from '@/components/CCTV/CCTVList.vue';
import { WaterSensorAreaResponse } from '@/resource/sensor';
import { GET_CAMERA_AREA_SIMPLE_LIST } from '@/store/image/actionTypes';
import { GET_WATER_SENSOR_AREA_LIST } from '@/store/sensor/actionTypes';
import { ref, watch } from 'vue';
import { useStore } from 'vuex';
const select = ref<string>('');
const hasData = ref<boolean>(false);
const store = useStore();

const options = ref<WaterSensorAreaResponse[]>([]);
const getWaterSensorArea = async (): Promise<void> => {
  const response = await store.dispatch(`sensor/${GET_WATER_SENSOR_AREA_LIST}`);
  if (response) {
    options.value = response;
  }
};
watch(
  () => select.value,
  (newValue: string): void => {
    getCameraByAreaIdSimple(newValue);
  },
);
const getCameraByAreaIdSimple = async (areaId: string): Promise<void> => {
    await store.dispatch(`image/${GET_CAMERA_AREA_SIMPLE_LIST}`, areaId);
  hasData.value = true;
};
getWaterSensorArea();
</script>

<style lang="scss" scoped>
    .cctv {
        box-shadow: -0.5px 0.5px 3px 0px rgba(0, 0, 0, 0.15);
        height: calc(100vh - 110px); /* 假設頭部高度為 64px */
    }
</style>
