<template>
  <el-table :data="props.tableData">
    <el-table-column
      type="index"
      label="序號"
      align="center"
      header-align="center"
      min-width="60"
    />
    <el-table-column
      prop="areaName"
      label="組別"
      align="center"
      header-align="center"
      min-width="120"
    />
    <el-table-column
      prop="stationName"
      label="站點名稱"
      align="center"
      header-align="center"
      min-width="120"
    />
    <el-table-column
      prop="sensorName"
      label="裝置名稱"
      align="center"
      header-align="center"
      min-width="150"
    />
    <el-table-column
      prop="status"
      label="狀態"
      align="center"
      header-align="center"
      min-width="80"
      v-if="true"
    />
    <el-table-column
      prop="lastDataTime"
      label="即時資訊時間"
      align="center"
      header-align="center"
      min-width="190"
    />
    <el-table-column
      prop="value"
      label="量測值"
      align="center"
      header-align="center"
      min-width="100"
    />
    <el-table-column
      label="趨勢"
      align="center"
      header-align="center"
      min-width="80"
    >
      <template #default="scope">
        <el-icon :size="12">
          <app-icon :icon-name="setStatus(scope.row.differ1)"></app-icon>
        </el-icon>
      </template>
    </el-table-column>
    <el-table-column
      label="更多資訊"
      align="center"
      header-align="center"
      min-width="80"
    >
      <template #default="scope">
        <el-icon
          :size="20"
          class="cursor-pointer"
          @click="showDetail(scope.row)"
        >
          <app-icon icon-name="icon_eye"></app-icon>
        </el-icon>
      </template>
    </el-table-column>
  </el-table>
  <div class="mt-4 text-right text-xs text-tertiary">
    共搜索 {{ props.tableData.length }} 筆資料
  </div>
</template>

<script setup lang="ts">
import { SensorGeneralQueryDataResponse } from '@/resource/sensor';

interface Props {
  tableData: SensorGeneralQueryDataResponse[];
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
