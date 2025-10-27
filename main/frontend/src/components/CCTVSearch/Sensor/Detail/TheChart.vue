<template>
  <div class="chart mb-4">
    <Line :data="data" :options="options" />
  </div>
</template>

<script setup lang="ts">
import { GetSensorChartDataResponse } from '@/resource/sensor';
import {
  CategoryScale,
  Chart as ChartJS,
  Legend, LegendElement,
  LineElement, LegendItem,
  LinearScale,
  PointElement,
  Title,
  Tooltip,
} from 'chart.js';
import { computed, onMounted } from 'vue';
import { Line } from 'vue-chartjs';

interface Props {
  chartData: GetSensorChartDataResponse;
}

const props = defineProps<Props>();

ChartJS.register(
  CategoryScale,
  LinearScale,
  PointElement,
  LineElement,
  Title,
  Tooltip,
  Legend,
);

const data = computed<any>(() => {
  return {
    type: 'line',
    datasets:
      (props.chartData?.lstData || [])
        .map((dataset, index) => {
          if (dataset.label.toString().trim().includes("初始值")) {
            console.log('Dataset label: ' + dataset.label);
          }

          if (dataset.label === "初始值") {
            console.log('Exact match found: ' + dataset.label);
          }
          return {
            ...dataset,
            data: dataset.data.map(point => ({
              ...point,
              x: formatDate(new Date(point.x)),
            })),
            borderWidth: 1,
            pointRadius: 0, // 移除資料端點的圓圈
            fill: false, // 禁用區域填色
            ...props.chartData.sensorId.includes("EQ") ? { borderColor: getBorderColor(index) } : {}, // 動態設定線段顏色
            ...props.chartData.sensorId.includes("EQ") ? { backgroundColor: getBorderColor(index) } : {}, // 動態設定背景顏色
            ...dataset.label.charCodeAt(1) == 22987 ? { borderColor: 'rgb(160,160,160)' } : {},
            ...dataset.label.charCodeAt(1) == 22987 ? { backgroundColor: 'rgb(160,160,160)' } : {},
          }
        }) ?? [],
  };
});

function getBorderColor(index: number): string {
  const colors = ['rgb(192, 0, 0)', 'rgb(192, 192, 0)', 'rgb(0, 192, 0)'];
  return colors[index % colors.length]; // 根據索引選擇顏色
}

const options = computed(() => {
  return {
    responsive: true,
    maintainAspectRatio: false,
  };
});

const formatDate = (date: Date): string => {
  let add = 0;// 8 * 60 * 60 * 1000;

  // 複製原始日期物件，避免直接修改原始日期
  const adjustedDate = new Date(date.getTime() + add);

  const year = String(adjustedDate.getFullYear()).slice(-4);
  const month = String(adjustedDate.getMonth() + 1).padStart(2, '0');
  const day = String(adjustedDate.getDate()).padStart(2, '0');
  const hours = String(adjustedDate.getHours()).padStart(2, '0');
  const minutes = String(adjustedDate.getMinutes()).padStart(2, '0');
  return `${year}-${month}-${day} ${hours}:${minutes}`;
};
</script>

<style lang="scss" scoped>
.chart {
  height: 266px;
}
</style>
