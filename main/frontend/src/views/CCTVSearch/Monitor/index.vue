<template>
  <div>
    <the-form @submit="submit"></the-form>
    <el-divider class="mb-6" />
    <div class="grid grid-cols-1 gap-x-8 sm:grid-cols-2">
      <CCTVImg
        v-for="(cctv, index) in cctvList"
        :key="index"
        :streamMain="cctv.streamMain"
      >
        <CCTVStatus
          :hasStatus="true"
          :status="cctv.isAlarm"
          :CamName="cctv.camName"
        ></CCTVStatus>
      </CCTVImg>
    </div>
  </div>
</template>

<script setup lang="ts">
import CCTVStatus from '@/components/CCTV/CCTVStatus.vue';
import CCTVImg from '@/components/CCTVSearch/Monitor/CCTVImg.vue';
import TheForm from '@/components/CCTVSearch/Monitor/TheForm.vue';
import {
  CameraBySearchPayload,
  CameraBySearchResponse,
  apiGetCameraBySearch,
} from '@/resource/cctv';
import { ref } from 'vue';

const cctvList = ref<CameraBySearchResponse[]>([]);
const submit = async (ruleForm: CameraBySearchPayload): Promise<void> => {
  try {
    const response = await apiGetCameraBySearch(ruleForm);
    if (!response) return;
    cctvList.value = response;
  } catch (error) {
    console.error(error);
  }
};
</script>
