<template>
  <div class="flex flex-col sm:flex-row">
    <el-popover placement="bottom-start" :width="450" trigger="click">
      <template #default>
        <div class="text-xs">
          <FormItem
            class="mb-4"
            name="組別"
            :options="groupList"
            @check-all="checkAllGroup"
          ></FormItem>

          <FormItem
            class="mb-4"
            name="感測器"
            :options="sensorList"
            @check-all="checkAllSensor"
          ></FormItem>
        </div>
      </template>
      <template #reference>
        <div
          class="flex h-[35px] w-full cursor-pointer items-center justify-between rounded border border-line p-[8px] text-sm text-black sm:w-[450px]"
        >
          <div>
            {{ groupText }},
            {{ sensorText }}
          </div>
          <el-icon :size="16" class="cursor-pointer">
            <app-icon icon-name="icon_down_arrow"></app-icon>
          </el-icon>
        </div>
      </template>
    </el-popover>

    <el-button
      type="primary"
      class="icon-button ml-0 mt-4 w-full sm:ml-4 sm:mt-0 sm:w-fit"
      :disabled="props.loading"
      @click="submit"
    >
      <el-icon :size="32" class="cursor-pointer">
        <app-icon icon-name="icon_search_button"></app-icon>
      </el-icon>
    </el-button>
  </div>
</template>

<script setup lang="ts">
import {
  WaterSensorAreaResponse,
  WaterSensorTypeResponse,
} from '@/resource/sensor';
import * as actionTypes from '@/store/sensor/actionTypes';
import { computed, ref } from 'vue';
import { useStore } from 'vuex';
import FormItem from './FormItem.vue';

interface Props {
  loading?: boolean;
}

const props = withDefaults(defineProps<Props>(), {
  loading: false,
});

const emits = defineEmits(['submit']);

const store = useStore();

interface CheckboxList {
  id: string;
  name: string;
  check: boolean;
}

const toggleAll = (list: CheckboxList[], newValue: boolean): void => {
  list.forEach((item) => {
    item.check = newValue;
  });
};

const checkAllGroup = (newValue: boolean): void => {
  toggleAll(groupList.value, newValue);
};

const checkAllSensor = (newValue: boolean): void => {
  toggleAll(sensorList.value, newValue);
};

const computeItemText = (
  list: CheckboxList[],
  count: number,
  pluralText: string,
): string => {
  const firstItem = list.find((f) => f.check)?.name ?? '';
  return count === 1 ? firstItem : `已選取 ${count} ${pluralText}`;
};

const groupText = computed<string>(() =>
  computeItemText(groupList.value, groupCount.value, '個組別'),
);

const sensorText = computed<string>(() =>
  computeItemText(sensorList.value, sensorCount.value, '種感測器'),
);

const groupCount = computed<number>(
  () => groupList.value.filter((f) => f.check).length,
);

const sensorCount = computed<number>(
  () => sensorList.value.filter((f) => f.check).length,
);

const groupList = ref<CheckboxList[]>([]);

const getWaterSensorArea = async (): Promise<void> => {
  const response = await store.dispatch(
    `sensor/${actionTypes.GET_WATER_SENSOR_AREA_LIST2}`,
  );
  if (response) {
    groupList.value = response.map((m: WaterSensorAreaResponse) => {
      return {
        id: m.areaId,
        name: m.areaName,
        check: false,
      };
    });
  }
};

const sensorList = ref<CheckboxList[]>([]);
const getWaterSensorType = async (): Promise<void> => {
  const response = await store.dispatch(
    `sensor/${actionTypes.GET_WATER_SENSOR_TYPE_LIST}`,
  );
  if (response) {
    sensorList.value = response.map((m: WaterSensorTypeResponse) => {
      return {
        id: m.sensorType,
        name: m.sensorTypeName,
        check: false,
      };
    });
  }
};

const submit = (): void => {
  const sensors = sensorList.value.filter((f) => f.check).map((m) => m.id);
  const groups = groupList.value.filter((f) => f.check).map((m) => m.id);

  if (sensors.length > 0 || groups.length > 0) {
    emits('submit', {
      sensors,
      groups,
    });
  }
};

getWaterSensorArea();
getWaterSensorType();
</script>
