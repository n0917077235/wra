<template>
  <div class="cursor-pointer text-primary" @click="checkAll = !checkAll">
    {{ checkAll ? '取消' : '' }}全選
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue';

interface Props {
  modelValue: boolean;
}

const props = defineProps<Props>();

const emits = defineEmits(['update:modelValue']);

const checkAll = computed<boolean>({
  get(): boolean {
    return props.modelValue;
  },
  set(value: boolean): void {
    emits('update:modelValue', value);
  },
});
</script>
