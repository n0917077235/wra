<template>
  <el-menu-item :index="props.index">
    <el-icon :size="20">
      <app-icon :icon-name="props.iconName"></app-icon>
    </el-icon>
    <template #title>
      <div class="ml-2">{{ props.pageName }}</div>
    </template>
  </el-menu-item>
</template>

<script setup lang="ts">
interface Props {
  index: string;
  iconName: string;
  pageName: string;
}

const props = defineProps<Props>();
</script>
