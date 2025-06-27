<template>
  <div>
    <div class="mb-4 flex justify-between">
      <div class="text-black">{{ name }}</div>
      <the-check-all v-model="checkAll"></the-check-all>
    </div>
    <div class="grid grid-cols-3 gap-4">
      <the-checkbox
        v-for="(group, index) in props.options"
        :key="index"
        v-model="group.check"
        :name="group.name"
      ></the-checkbox>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, watch } from 'vue';
import TheCheckAll from './TheCheckAll.vue';
import TheCheckbox from './TheCheckbox.vue';

interface CheckboxList {
  name: string;
  check: boolean;
}

interface Props {
  name: string;
  options: CheckboxList[];
}

const props = defineProps<Props>();

const emits = defineEmits(['checkAll']);

const checkAll = ref<boolean>(false);

watch(checkAll, (newValue: boolean): void => {
  emits('checkAll', newValue);
});
</script>
