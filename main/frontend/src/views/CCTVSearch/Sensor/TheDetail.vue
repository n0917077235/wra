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
      <el-icon :size="35" class="cursor-pointer" ref="buttonRef" @click="visible = true"
        v-click-outside="onClickOutside">
        <app-icon icon-name="icon_map"></app-icon>
      </el-icon>

      <el-popover ref="popoverRef" :virtual-ref="buttonRef" trigger="click" virtual-triggering :width="500">
        <CCTVMap v-if="visible && showCCTVMap" :title="detail.stationName" :x="detail.x" :y="detail.y"></CCTVMap>
      </el-popover>
      <div class="hidden text-primary sm:block">定位</div>
    </div>
    <the-form @submit="submit" @download="onDownload"></the-form>
    <template v-if="loaded && data">
      <the-chart :chart-data="data"></the-chart>
      <the-table :table-data="data"></the-table></template>
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

// 下載：把 lstData[0].data 轉成 CSV（序號/資料時間/開門(%) / 狀態2）
const onDownload = (form: RuleForm) => {
  // 1) 取得來源陣列
  const pts = data.value?.lstData?.[0]?.data ?? [];
  if (!Array.isArray(pts) || pts.length === 0) {
    alert('目前沒有可匯出的資料');
    return;
  }

  // 2) 映射成固定四欄
  const rows = pts.map((p: any, i: number) => ({
    seq: i + 1,
    time: toSafeDateString(p?.x),   // 轉成 'YYYY-MM-DD HH:mm'
    openPct: p?.y ?? '',
    status2: ''                     // 下面再補
  }));

  // 3) 依開門(%)變化補狀態2：↑ / ↓ / =
  for (let i = 0; i < rows.length; i++) {
    if (i === 0) {
      rows[i].status2 = '='; // 第一筆固定顯示 "="
    } else {
      const prev = Number(rows[i - 1].openPct);
      const curr = Number(rows[i].openPct);
      rows[i].status2 = curr > prev ? '↑' : curr < prev ? '↓' : '=';
    }
  }

  // 4) 做 CSV
  const headers = ['序號', '資料時間', '閘門(%)', '狀態2'];
  const csv = toCSV([
    headers,
    ...rows.map(r => [r.seq, r.time, r.openPct, r.status2].map(escapeCell))
  ]);

  // 5) 檔名
  const [start, end] = getStartEndString(form);
  const filename = `export_${filenameSafe(props.detail.areaName)}_${filenameSafe(
    props.detail.stationName
  )}_${filenameSafe(props.detail.sensorName)}_${start.replace(/[: ]/g, '')}_${end.replace(
    /[: ]/g, ''
  )}_r${filenameSafe(form.range)}.csv`;

  // 6) 下載
  triggerDownload(csv, filename);
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

// 讓 Date/string/ISO/timestamp 都能變 'YYYY-MM-DD HH:mm'
function toSafeDateString(v: any): string {
  if (v == null || v === '') return '';
  if (v instanceof Date) return formatDateForName(v);

  if (typeof v === 'number') {
    const ms = v < 1e12 ? v * 1000 : v;
    return formatDateForName(new Date(ms));
  }

  if (typeof v === 'string') {
    // '2025-06-14T15:40:00' → new Date 可直接解析；為相容性替換 '-'
    const d = new Date(v.replace(/-/g, '/'));
    if (!isNaN(d.getTime())) return formatDateForName(d);
    return v;
  }

  if (typeof v?.valueOf === 'function') {
    const t = v.valueOf();
    if (typeof t === 'number' && !isNaN(t)) {
      return formatDateForName(new Date(t < 1e12 ? t * 1000 : t));
    }
  }
  return String(v ?? '');
}

function formatDateForName(d: Date): string {
  const y = d.getFullYear();
  const m = String(d.getMonth() + 1).padStart(2, '0');
  const day = String(d.getDate()).padStart(2, '0');
  const hh = String(d.getHours()).padStart(2, '0');
  const mm = String(d.getMinutes()).padStart(2, '0');
  return `${y}-${m}-${day} ${hh}:${mm}`;
}

function getStartEndString(form: RuleForm): [string, string] {
  const raw0 = form?.dateTime?.[0] ?? form?.idateTime?.[0];
  const raw1 = form?.dateTime?.[1] ?? form?.idateTime?.[1];
  return [toSafeDateString(raw0), toSafeDateString(raw1)];
}

function filenameSafe(s: unknown): string {
  return String(s ?? '').replace(/[\\/:*?"<>|]/g, '').trim();
}

function toCSV(rows: (string | number)[][]): string {
  return '\uFEFF' + rows.map(r => r.join(',')).join('\r\n'); // UTF-8 BOM for Excel
}

function escapeCell(v: any): string {
  if (v === null || v === undefined) return '';
  let s = String(v);
  if (/[",\r\n]/.test(s)) s = `"${s.replace(/"/g, '""')}"`;
  return s;
}

function triggerDownload(csvText: string, filename: string) {
  const blob = new Blob([csvText], { type: 'text/csv;charset=utf-8;' });
  const url = URL.createObjectURL(blob);
  const a = document.createElement('a');
  a.href = url;
  a.download = filename;
  document.body.appendChild(a);
  a.click();
  document.body.removeChild(a);
  URL.revokeObjectURL(url);
}
</script>
