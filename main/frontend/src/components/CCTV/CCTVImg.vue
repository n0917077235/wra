<template>
  <div class="mb-[20px]" @click="handleImageClick" v-show="cctvImgSrc">
    <slot></slot>
    <img :src="cctvImgSrc" alt="cctv" class="cctv-img w-full object-cover" />
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue';
import { useStore } from 'vuex';

const store = useStore();

const cctvImgSrc = computed<string>(
  () => store.state.image.currentCameraArea?.streamMain ?? '',
);
const handleImageClick = (): void => {
  window.open(cctvImgSrc.value, '_blank');
};
</script>

<style lang="scss" scoped>
    .cctv-img {
        height: calc(100% - 300px) !important;
        @media (max-width: $lg) {
            height: 300px;
        }

        @media (max-width: $sm) {
            height: 190px;
        }
    }
</style>
