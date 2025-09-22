<template>
  <div class="mb-[5px] py-[10px] md:mb-[24px]" :class="isCollapse ? 'px-[10px]' : 'px-[16px]'">
    <div v-show="isCollapse" class="flex items-center">
      <el-icon :size="25" class="mx-2">
        <app-icon icon-name="logo"></app-icon>
      </el-icon>
      <el-icon>
        <el-icon :size="20" class="cursor-pointer" @click="handleCollapseChange(false)">
          <app-icon icon-name="icon_toggle_right"></app-icon>
        </el-icon>
      </el-icon>
    </div>

    <div v-show="!isCollapse" class="flex items-center justify-between">
      <router-link to="/" class="mr-2 hidden sm:block">
        <img src="@/assets/image/logo-main.png" alt="logo" />
      </router-link>
      <el-icon :size="16" class="cursor-pointer" @click="handleCollapseChange(true)">
        <app-icon icon-name="icon_toggle_left"></app-icon>
      </el-icon>
    </div>
  </div>
</template>

<script lang="ts" setup>
import { inject } from 'vue';

const isCollapse = inject<boolean>('isCollapse');
const handleCollapseChange = inject('handleCollapse', (bool: boolean) => bool);
</script>
