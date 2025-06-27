<template>
  <svg :class="props.svgClass" aria-hidden="true">
    <use :xlink:href="`#${props.iconName}`" />
  </svg>
</template>

<script setup lang="ts">
interface Props {
  svgClass?: string;
  iconName: string;
}

const props = withDefaults(defineProps<Props>(), {
  svgClass: '',
});
</script>

<style lang="scss" scoped>
.svg-icon {
  fill: currentColor !important;
  overflow: hidden;
}
</style>
