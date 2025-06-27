<template>
  <div class="mr-2 flex min-h-[34px] items-center sm:mr-4 md:hidden">
    <div class="mr-2 flex sm:hidden">
      <el-icon :size="25">
        <app-icon icon-name="logo"></app-icon>
      </el-icon>
    </div>
    <img
      src="@/assets/image/logo-main.png"
      alt="logo"
      class="mr-2 hidden sm:block"
    />
    <el-icon :size="14" class="cursor-pointer" @click="handleMenuDrawer">
      <app-icon icon-name="icon_toggle_left" v-show="openDrawer"></app-icon>
      <app-icon icon-name="icon_toggle_right" v-show="!openDrawer"></app-icon>
    </el-icon>
  </div>
</template>

<script lang="ts" setup>
import { inject } from 'vue';

const openDrawer = inject<boolean>('openDrawer');
const handleMenuDrawer = inject('handleMenuDrawer');
</script>
