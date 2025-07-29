<template>
  <div v-if="showDetail && detail">
    <the-detail :detail="detail" @hideDetail="hideDetail"></the-detail>
  </div>
  <div v-show="!showDetail">
    <the-form :loading="loading" :areaIDs="[areaID]" :sensorTypes="[sensorType]" @submit="submit"
      @initialized="onFormInitialized">
    </the-form>
    <el-divider class="my-6" />

    <div v-show="tableData.length > 0">
      <div class="hidden sm:block">
        <the-table :table-data="tableData" @showDetail="getDetail"></the-table>
      </div>

      <div class="block sm:hidden">
        <mobile-card v-for="(item, index) in tableData" :key="index + 1" :index="index" :data="item"
          @showDetail="getDetail"></mobile-card>
      </div>
    </div>

    <div v-show="tableData.length === 0" class="text-center text-secondary">
      查無此結果
    </div>
  </div>
</template>

<script setup lang="ts">
import TheForm from '@/components/CCTVSearch/Sensor/Form/index.vue';
import MobileCard from '@/components/CCTVSearch/Sensor/MobileCard.vue';
import TheTable from '@/components/CCTVSearch/Sensor/TheTable.vue';
import {
  apiGetSensorGeneralQueryData,
  SensorGeneralQueryDataResponse,
} from '@/resource/sensor';
import { ref } from 'vue';
import TheDetail from './TheDetail.vue';
import MyUtil from '@/resource/myUtil.js';

const loading = ref<boolean>(false);
const tableData = ref<SensorGeneralQueryDataResponse[]>([]);

const getPayload = (groups: string[], sensors: string[]) => {
  const groupString = groups.map((group) => `0;${group}`).join(',');
  const sensorString = sensors.map((sensor) => `1;${sensor}`).join(',');
  return `${sensorString},${groupString}`;
};

const submit = async (data: {
  sensors: string[];
  groups: string[];
}): Promise<void> => {
  loading.value = true;
  try {
    let payload = getPayload(data.groups, data.sensors);
    const response = await apiGetSensorGeneralQueryData(payload);
    if (!response) return;
    tableData.value = response;
  } catch (error) {
    console.error(error);
  }

  loading.value = false;
};

const showDetail = ref<boolean>(false);
const detail = ref<SensorGeneralQueryDataResponse>();

const getDetail = (row: SensorGeneralQueryDataResponse): void => {
  console.log('td', tableData.value)
  console.log('row', row);
  detail.value = row;
  showDetail.value = true;
};

const hideDetail = () => {
  detail.value = undefined;
  showDetail.value = false;
};

let areaID = MyUtil.getUrlParam('areaID');
let sensorType = MyUtil.getUrlParam('sensorType');
let sensorId = MyUtil.getUrlParam('sensorId');

async function onFormInitialized() {
  if (areaID && sensorType) {
    await submit({ sensors: [sensorType], groups: [areaID] });
    let match = tableData.value.find(x => x.sensorId === sensorId);
    if (!match) return;
    getDetail(match);
  }
}
</script>
