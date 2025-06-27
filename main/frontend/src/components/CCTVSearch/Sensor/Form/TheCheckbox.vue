<template>
  <div class="flex cursor-pointer items-center" @click="value = !value">
    <el-icon :size="16" class="mr-1">
      <app-icon
        :icon-name="props.modelValue ? 'icon_checkbox_filled' : 'icon_checkbox'"
      ></app-icon>
    </el-icon>
    {{ props.name }}
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue';

interface Props {
  modelValue: boolean;
  name: string;
}

const props = defineProps<Props>();

const emits = defineEmits(['update:modelValue']);

const value = computed<boolean>({
  get(): boolean {
    return props.modelValue;
  },
  set(value: boolean): void {
    emits('update:modelValue', value);
  },
});
</script>
