<template>
  <div>
    <div class="mb-4 flex items-center">
      <el-icon :size="20" @click="hideDetail" class="cursor-pointer">
        <app-icon icon-name="icon_left_arrow"></app-icon>
      </el-icon>
      <div class="mx-4">
        第十河川分署-{{ detail.areaName }}-{{ detail.stationName }}-{{
          detail.sensorName
        }}
        (經緯: {{ detail.x }}, {{ detail.y }})
      </div>
      <el-icon
        :size="35"
        class="cursor-pointer"
        ref="buttonRef"
        @click="visible = true"
        v-click-outside="onClickOutside"
      >
        <app-icon icon-name="icon_map"></app-icon>
      </el-icon>

      <el-popover
        ref="popoverRef"
        :virtual-ref="buttonRef"
        trigger="click"
        virtual-triggering
        :width="500"
      >
        <CCTVMap
          v-if="visible && showCCTVMap"
          :title="detail.stationName"
          :x="detail.x"
          :y="detail.y"
        ></CCTVMap>
      </el-popover>
      <div class="hidden text-primary sm:block">定位</div>
    </div>
    <the-form @submit="submit"></the-form>
    <template v-if="loaded && data">
      <the-chart :chart-data="data"></the-chart>
      <the-table :table-data="data"></the-table
    ></template>
    <div></div>
  </div>
</template>

<script setup lang="ts">
import CCTVMap from '@/components/CCTV/CCTVMap.vue';
import TheChart from '@/components/CCTVSearch/Sensor/Detail/TheChart.vue';
import TheForm from '@/components/CCTVSearch/Sensor/Detail/TheForm.vue';
import TheTable from '@/components/CCTVSearch/Sensor/Detail/TheTable.vue';
import {
  GetSensorChartDataResponse,
  SensorGeneralQueryDataResponse,
  apiGetSensorChartData,
} from '@/resource/sensor';
import { ClickOutside as vClickOutside } from 'element-plus';
import { ref, unref, watch } from 'vue';

const buttonRef = ref();
const popoverRef = ref();
const onClickOutside = () => {
  unref(popoverRef).popperRef?.delayHide?.();
};

interface Props {
  detail: SensorGeneralQueryDataResponse;
}

const props = defineProps<Props>();

const emits = defineEmits(['hideDetail']);

const hideDetail = (): void => {
  emits('hideDetail');
};

interface RuleForm {
  dateTime: [string, string];
  range: number;
  idateTime: [string, string];
}

const submit = (ruleForm: RuleForm) => {
  getData(ruleForm);
};

const visible = ref(false);
const showCCTVMap = ref(false);
const loaded = ref<boolean>(false);
const data = ref<GetSensorChartDataResponse>();

watch(
  () => visible.value,
  (newValue) => {
    setTimeout(() => {
      showCCTVMap.value = newValue;
    }, 1000);
  },
);
const formatDate2 = (date: Date): string => {
  // 複製原始日期物件，避免直接修改原始日期
  const adjustedDate = new Date(date.getTime());

  const year = String(adjustedDate.getFullYear()).slice(-4);
  const month = String(adjustedDate.getMonth() + 1).padStart(2, '0');
  const day = String(adjustedDate.getDate()).padStart(2, '0');
  const hours = String(adjustedDate.getHours()).padStart(2, '0');
  const minutes = String(adjustedDate.getMinutes()).padStart(2, '0');
  return `${year}-${month}-${day} ${hours}:${minutes}`;
};

const getData = async (ruleForm: RuleForm): Promise<void> => {
  try {
    loaded.value = false;
    let time1 = "";
    let time2 = "";
    if (ruleForm.dateTime[0] instanceof Date) {
      time1 = formatDate2(ruleForm.dateTime[0]);
      time2 = formatDate2(ruleForm.dateTime[1]);
    } else {
      time1 = ruleForm.idateTime[0];
      time2 = ruleForm.idateTime[1];
    }

    let payl = {
      sensorId: props.detail.sensorId,
      begin: time1,
      end: time2,
      duration: ruleForm.range,
      backgroundColorValue1: '#2A78B6',
      borderColorValue1: '#2A78B6',
      borderWidthValue1: 1,
      backgroundColorValue2: '#123C57',
      borderColorValue2: '#123C57',
      borderWidthValue2: 1,
      backgroundColorLevel1: '#F22A2A',
      borderColorLevel1: '#F22A2A',
      borderWidthLevel1: 1,
      backgroundColorLevel2: '#FFB341',
      borderColorLevel2: '#FFB341',
      borderWidthLevel2: 1,
      backgroundColorLevel3: '#80BD34',
      borderColorLevel3: '#80BD34',
      borderWidthLevel3: 1,
    };

    const response = await apiGetSensorChartData(payl);
    
    if (response) {
      data.value = response;
    }
  } catch (error) {
    console.error(error);
  } finally {
    loaded.value = true;
  }
};
</script>
