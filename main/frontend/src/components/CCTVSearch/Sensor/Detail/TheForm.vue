<template>
  <el-form :inline="true" :model="ruleForm" class="flex flex-wrap">
    <el-form-item label="資料時間" prop="dateTime">
      <el-date-picker
        v-model="ruleForm.dateTime"
        type="datetimerange"
        format="YYYY-MM-DD HH:mm:ss"
      />
    </el-form-item>

    <wra-select
      v-model="ruleForm.range"
      label="資料間隔"
      name="range"
      :options="options"
      value-name="value"
      label-name="label"
      class="w-full sm:w-fit"
      style="width:200px"
    ></wra-select>

    <el-button
      type="primary"
      class="icon-button mb-2 w-full sm:w-fit"
      @click="submit"
    >
      <el-icon :size="32" class="cursor-pointer">
        <app-icon icon-name="icon_search_button"></app-icon>
      </el-icon>
    </el-button>

    <el-button type="primary" class="icon-button w-full sm:w-fit">
      <el-icon :size="32" class="cursor-pointer">
        <el-icon :size="19">
          <app-icon icon-name="icon_download"></app-icon>
        </el-icon>
      </el-icon>
    </el-button>
  </el-form>
</template>

<script setup lang="ts">
import { reactive, ref, watch } from 'vue';

const emits = defineEmits(['submit']);
    
interface Option {
  value: number;
  label: string;
}

const options = ref<Option[]>([
  {
    value: 10,
    label: '10分鐘',
  },
  {
    value: 30,
    label: '30分鐘',
  },
  {
    value: 60 * 1,
    label: '1小時',
  },
  {
    value: 60 * 4,
    label: '4小時',
  },
  {
    value: 60 * 8,
    label: '8小時',
  },
  {
    value: 60 * 12,
    label: '12小時',
  },
]);

interface RuleForm {
  dateTime: [string, string];
  range: number;
  idateTime: [string, string];
}

const ruleForm = reactive<RuleForm>({
  dateTime: ["", ""],
  range: 10,
  idateTime: ["", ""],
});

watch(ruleForm.dateTime, () => {
  alert(ruleForm.dateTime);
});

const adjustToPreviousTenMinutes = (currentTime: Date): Date => {
  const adjustedMinute =
    currentTime.getMinutes() - (currentTime.getMinutes() % 10);
  const adjustedTime = new Date(currentTime);
  adjustedTime.setMinutes(adjustedMinute, 0, 0);
  return adjustedTime;
};

const getInitDateTime = (): void => {
  const currentDateTime = new Date();
  const adjustedTime = adjustToPreviousTenMinutes(currentDateTime);

  const startTime = new Date(adjustedTime.getTime() - 24 * 60 * 60 * 1000);
  const endTime = new Date(adjustedTime);

  ruleForm.dateTime = [formatDate(startTime), formatDate(endTime)];
  ruleForm.idateTime = [formatDate2(startTime), formatDate2(endTime)];
};

const formatDate = (date: Date): string => {
  const year = date.getFullYear();
  const month = String(date.getMonth() + 1).padStart(2, '0');
  const day = String(date.getDate()).padStart(2, '0');
  const hours = String(date.getHours()).padStart(2, '0');
  const minutes = String(date.getMinutes()).padStart(2, '0');
  const seconds = String(date.getSeconds()).padStart(2, '0');
  return `${year}-${month}-${day} ${hours}:${minutes}:${seconds}`;
};

const formatDate2 = (date: Date): string => {
  // 複製原始日期物件，避免直接修改原始日期
  const adjustedDate = new Date(date.getTime());
  const year = adjustedDate.getFullYear();
  const month = String(adjustedDate.getMonth() + 1).padStart(2, '0');
  const day = String(adjustedDate.getDate()).padStart(2, '0');
  const hours = String(adjustedDate.getHours()).padStart(2, '0');
  const minutes = String(adjustedDate.getMinutes()).padStart(2, '0');
  const seconds = String(adjustedDate.getSeconds()).padStart(2, '0');
  return `${year}-${month}-${day} ${hours}:${minutes}:${seconds}`;
};

const submit = (): void => {
  emits('submit', ruleForm);
};

getInitDateTime();
submit();
</script>
