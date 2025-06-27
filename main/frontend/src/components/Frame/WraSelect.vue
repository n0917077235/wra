<template>
  <el-form-item :label="props.label" :prop="props.name">
    <el-select
      v-model="select"
      v-bind="$attrs"
      :placeholder="props.placeholder"
      :suffix-icon="ArrowDownBold"
    >
      <el-option
        v-for="item in props.options"
        :key="props.withoutValueLabel ? item : (item as any).value"
        :label="props.withoutValueLabel ? item : (item as any)[props.labelName]"
        :value="props.withoutValueLabel ? item : (item as any)[props.valueName]"
      />
    </el-select>
  </el-form-item>
</template>

<script setup lang="ts">
import { ArrowDownBold } from '@element-plus/icons-vue';
import { computed } from 'vue';

interface Props {
  name: string;
  label: string;
  modelValue: string | number | boolean;
  options: unknown[];
  valueName: string;
  labelName: string;
  placeholder?: string;
  withoutValueLabel?: boolean;
}

const props = withDefaults(defineProps<Props>(), {
  placeholder: '選擇',
  withoutValueLabel: false,
});

const emits = defineEmits(['update:modelValue']);

const select = computed<string | number | boolean>({
  get(): string | number | boolean {
    return props.modelValue;
  },
  set(value: string | number | boolean): void {
    emits('update:modelValue', value);
  },
});
</script>
