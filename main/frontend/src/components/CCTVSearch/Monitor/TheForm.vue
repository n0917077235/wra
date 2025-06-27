<template>
  <el-form :inline="true" :model="ruleForm" class="flex flex-wrap">
    <wra-select
      v-model="ruleForm.areaId"
      label="組別"
      name="areaId"
      :options="areaOptions"
      value-name="areaId"
      label-name="areaName"
      class="w-full sm:w-fit"
      style="width:150px"
    ></wra-select>

    <wra-select
      v-model="ruleForm.stationId"
      label="站名"
      name="stationId"
      :options="stationOptions"
      value-name="stationID"
      label-name="stationNameA"
      class="w-full sm:w-fit"
      style="width:200px"
    ></wra-select>

    <wra-select
      v-model="ruleForm.isAlarm"
      label="狀態"
      name="isAlarm"
      :options="statusOptions"
      value-name="value"
      label-name="label"
      class="w-full sm:w-fit"
      style="width:150px"
    ></wra-select>

    <el-form-item label="關鍵字" prop="keyword" class="w-full sm:w-fit">
      <el-input v-model="ruleForm.keyword" placeholder="請輸入關鍵字" />
    </el-form-item>

    <el-button
      type="primary"
      class="icon-button w-full sm:w-fit"
      @click="submit"
    >
      <el-icon :size="32" class="cursor-pointer">
        <app-icon icon-name="icon_search_button"></app-icon>
      </el-icon>
    </el-button>
  </el-form>
</template>

<script setup lang="ts">
import { CameraBySearchPayload } from '@/resource/cctv';
import {
  CameraByAreaIdResponse,
  WaterSensorAreaResponse,
  apiGetCameraByAreaId,
} from '@/resource/sensor';
import { GET_WATER_SENSOR_AREA_LIST } from '@/store/sensor/actionTypes';
import { reactive, ref, watch } from 'vue';
import { useStore } from 'vuex';

const ruleForm = reactive<CameraBySearchPayload>({
  areaId: '',
  stationId: '',
  isAlarm: '',
  keyword: '',
});

const emits = defineEmits(['submit']);

const store = useStore();

const areaOptions = ref<WaterSensorAreaResponse[]>([]);
const getWaterSensorArea = async (): Promise<void> => {
  const response = await store.dispatch(`sensor/${GET_WATER_SENSOR_AREA_LIST}`);
  if (response) {
    areaOptions.value = response;
  }
};

watch(
  () => ruleForm.areaId,
  (newValue: string): void => {
    ruleForm.stationId = '';
    getCameraByAreaId(newValue);
  },
);

const stationOptions = ref<CameraByAreaIdResponse[]>([]);
const getCameraByAreaId = async (value: string | number): Promise<void> => {
  try {
    const response = await apiGetCameraByAreaId(value.toString());
    if (!response) return;

    const set = new Set();
    const result = response.filter((item) =>
      !set.has(item.stationID) ? set.add(item.stationID) : false,
    );
    stationOptions.value = result;
  } catch (error) {
    console.error(error);
  }
};

interface Status {
  value: string | boolean;
  label: string;
}

const statusOptions = ref<Status[]>([
  {
    value: '',
    label: '全部',
  },
  {
    value: false,
    label: '正常',
  },
  {
    value: true,
    label: '無回應',
  },
]);

const submit = (): void => {
  emits('submit', ruleForm);
};

getWaterSensorArea();
</script>
