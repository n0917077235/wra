<template>
  <div class="mb-[20px]">
    <slot></slot>
    <img
      :src="props.streamMain"
      alt="cctv"
      class="cctv-img w-full rounded-r-lg rounded-bl-lg"
    />
  </div>
</template>

<script setup lang="ts">
interface Props {
  streamMain: string;
}

const props = defineProps<Props>();
</script>

<style lang="scss" scoped>
.cctv-img {
  height: 360px;
  @media (max-width: $xl) {
    height: 193px;
  }
}
</style>
