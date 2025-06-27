<template>
  <div class="flex cursor-pointer items-center p-[20px]">
    <el-icon :size="20">
      <app-icon icon-name="icon_log_out"></app-icon>
    </el-icon>
    <div class="ml-3" v-show="!isCollapse" @click="logout">系統登出</div>
  </div>
</template>

<script setup lang="ts">
import { LOGOUT } from '@/store/user/actionTypes';
import { computed, inject } from 'vue';
import { useRouter } from 'vue-router';
import { useStore } from 'vuex';

const store = useStore();
const router = useRouter();

const isCollapse = inject<boolean>('isCollapse');

const userId = computed<string>(() => store.state.user.userId);

const logout = async (): Promise<void> => {
  const response = await store.dispatch(`user/${LOGOUT}`, userId.value);
  if (response) {
    router.push('/login');
  }
};
</script>
