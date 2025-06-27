<template>
  <el-tag>
    <el-icon :size="12" v-if="props.hasStatus">
      <app-icon v-if="props.status" icon-name="icon_status_red"></app-icon>
      <app-icon v-else icon-name="icon_status_green"></app-icon>
    </el-icon>
    {{ props.CamName }}
  </el-tag>
</template>

<script setup lang="ts">
interface Props {
  hasStatus?: boolean;
  status?: number;
  CamName: string;
}

const props = withDefaults(defineProps<Props>(), {
  hasStatus: false,
  status: 0,
});
</script>
