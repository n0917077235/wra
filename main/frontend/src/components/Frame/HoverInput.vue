<template>
  <el-form-item
    :label="props.label"
    :prop="props.name"
    class="hover-input"
    :class="{ active: active }"
  >
    <el-input
      v-bind="$attrs"
      v-model="input"
      :placeholder="props.placeholder"
      @focus="onFocus = true"
      @blur="onFocus = false"
    />
  </el-form-item>
</template>

<script setup lang="ts">
import { computed, ref } from 'vue';

interface Props {
  name: string;
  label: string;
  modelValue: string | number;
  placeholder?: string;
}

const props = withDefaults(defineProps<Props>(), {
  placeholder: '',
});

const emits = defineEmits(['update:modelValue']);

const onFocus = ref<boolean>(false);

const input = computed<string | number>({
  get(): string | number {
    return props.modelValue;
  },
  set(value: string | number): void {
    emits('update:modelValue', value);
  },
});

const active = computed<boolean>(
  () => !!props.modelValue || onFocus.value || !!props.placeholder,
);
</script>
