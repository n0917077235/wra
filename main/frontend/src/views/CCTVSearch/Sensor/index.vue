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
      <!-- 新增：工具列（下載 CSV 按鈕） -->
      <div class="flex items-center justify-end mb-3">
        <el-button type="primary" size="small" @click="downloadCsv">
          下載 CSV
        </el-button>
      </div>

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
    const response = await apiGetSensorGeneralQueryData(data.groups, data.sensors);
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

/** ---------- 新增：CSV 下載 ---------- **/
// 1) 想要輸出的欄位（顯示名稱與對應資料來源）
const CSV_COLUMNS: Array<{
  header: string;                 // CSV 標題（中文）
  value: (row: any, idx: number) => unknown;  // 取值函式（idx 為序號用）
}> = [
  { header: '序號',          value: (_r, i) => i + 1 },
  { header: '組別',          value: r => r.areaName ?? '' },
  { header: '站點名稱',      value: r => r.stationName ?? r.siteName ?? '' },
  { header: '裝置名稱',      value: r => r.deviceName ?? r.sensorName ?? '' },
  { header: '感測器類別',    value: r => r.sensorTypeName ?? r.sensorType ?? '' },
  { header: '感測器編號',    value: r => r.sensorId ?? r.id ?? '' },
  { header: '狀態',          value: r => r.status ?? '' },
  { header: '最新資料時間',  value: r => r.lastDataTime ?? '' },
  { header: '量測值',        value: r => r.value ?? r.measure ?? '' },
  { header: '單位',          value: r => r.unit ?? '' },
  { header: '趨勢',          value: r => trendToText(r.direction ?? '') },
  // 需要再加欄位就往下放即可：
  // { header: '備註', value: r => r.note ?? '' },
];

function trendToText(src: unknown): string {
  // 兼容三種情況：枚舉值、箭頭符號、圖片網址/檔名
  const s = (src ?? '').toString().toLowerCase();
  if (!s) return '';
  if (['up', 'rise', 'rising', 'increase', 'inc', '↑'].some(k => s.includes(k))) return '↑';
  if (['down', 'fall', 'decrease', 'dec', '↓'].some(k => s.includes(k))) return '↓';
  if (['remove', 'equal', 'flat', 'same', 'steady', '＝', '='].some(k => s.includes(k))) return '＝';
  // 圖片檔名包含 up/down/equal 也能判斷
  if (/\b(up|arrow_up)\b/.test(s)) return '上升';
  if (/\b(down|arrow_down)\b/.test(s)) return '下降';
  if (/\b(equal|flat)\b/.test(s)) return '持平';
  return s; // 萬一不是圖示就原樣輸出
}

function toCsvValue(v: unknown): string {
  const s = (v ?? '').toString();
  const escaped = s.replace(/"/g, '""');
  return `"${escaped}"`;
}

function buildCsvByColumns(rows: any[]): string {
  const headerRow = CSV_COLUMNS.map(c => toCsvValue(c.header)).join(',');
  const dataRows = rows.map((r, i) =>
    CSV_COLUMNS.map(c => toCsvValue(c.value(r, i))).join(',')
  );
  return '\uFEFF' + [headerRow, ...dataRows].join('\r\n'); // 含 BOM，避免 Excel 亂碼
}

function downloadCsv() {
  const arr = tableData.value as unknown as Record<string, any>[];
  if (!arr?.length) return;

  const csv = buildCsvByColumns(arr);
  const blob = new Blob([csv], { type: 'text/csv;charset=utf-8;' });
  const url = URL.createObjectURL(blob);

  const pad = (n: number) => n.toString().padStart(2, '0');
  const d = new Date();
  const filename =
    `SensorDetail_${d.getFullYear()}${pad(d.getMonth()+1)}${pad(d.getDate())}_${pad(d.getHours())}${pad(d.getMinutes())}${pad(d.getSeconds())}.csv`;

  const a = document.createElement('a');
  a.href = url;
  a.download = filename;
  a.click();
  URL.revokeObjectURL(url);
}
/** ----------------------------------- **/
</script>
