<template>
  <el-table :data="data">
    <el-table-column prop="i" label="序號" align="center" header-align="center" />
    <el-table-column prop="x" label="資訊時間" align="center" header-align="center" />
    <el-table-column prop="y" :label="props.tableData.yLabel" align="center" header-align="center" />
    <el-table-column label="狀態2" align="center" header-align="center">
      <template #default="scope">
        <el-icon :size="12">
          <app-icon :icon-name="scope.row.status"></app-icon>
        </el-icon>
      </template>
    </el-table-column>
  </el-table>
</template>

<script setup lang="ts">
import { GetSensorChartDataResponse } from '@/resource/sensor';
import { computed } from 'vue';

interface Props {
  tableData: GetSensorChartDataResponse;
}

const props = defineProps<Props>();

const setStatus = (value: number): string => {
  if (value > 0) {
    return 'icon_up';
  } else if (value < 0) {
    return 'icon_down';
  } else {
    return 'icon_no_diff';
  }
};

interface Data {
  i: number;
  x: string;
  y: number;
  status: string;
}

const data = computed<any>(() => {
  let index = 1;

  const allData: Data[] = (props.tableData?.lstData || [])
    .filter((dataSet) => !dataSet.label.includes('警戒'))
    .filter((dataSet) => !dataSet.label.includes('初始值'))
    .flatMap((dataSet) =>
      dataSet.data.map((dataPoint, j) => {
        const currentY = Number(dataPoint.y);
        const previousY: number | undefined =
          j > 0 ? Number(dataSet.data[j - 1].y) : undefined;

        return {
          i: index++,
          x: formatDate(new Date(dataPoint.x)),
          y: dataPoint.y,
          status: previousY !== undefined
            ? setStatus(currentY - previousY)
            : 'icon_no_diff',
        };
      }),
    );

  return allData;
});

const formatDate = (date: Date): string => {
  let add = 0;//8 * 60 * 60 * 1000;

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
