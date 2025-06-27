<template>
  <el-sub-menu
    :index="props.index"
    :expand-open-icon="ArrowUpBold"
    :expand-close-icon="ArrowDownBold"
    :collapse-open-icon="ArrowLeftBold"
    :collapse-close-icon="ArrowRightBold"
  >
    <template #title>
      <el-icon :size="20">
        <app-icon :icon-name="props.iconName"></app-icon>
      </el-icon>
      <div class="ml-2" v-show="!isCollapse">{{ props.pageName }}</div>
    </template>
    <el-menu-item
      v-for="children in routerChildren"
      :key="children.index"
      :index="children.index"
    >
      <span class="ml-5">{{ children.name }}</span>
    </el-menu-item>
  </el-sub-menu>
</template>

<script setup lang="ts">
import {
  ArrowDownBold,
  ArrowLeftBold,
  ArrowRightBold,
  ArrowUpBold,
} from '@element-plus/icons-vue';
import { inject } from 'vue';

const isCollapse = inject<boolean>('isCollapse');

interface RouterChildren {
  index: string;
  name: string;
}

interface Props {
  index: string;
  iconName: string;
  pageName: string;
  routerChildren: RouterChildren[];
}

const props = defineProps<Props>();
</script>
