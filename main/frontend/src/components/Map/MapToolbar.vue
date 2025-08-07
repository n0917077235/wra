<template>
    
</template>

<script setup lang="ts">
import { onMounted, computed } from 'vue';

const props = defineProps(['overlay', 'content']);
const emit = defineEmits(['initialized']);

onMounted(() => {
    const container = document.getElementById('popup');
    const closer = document.getElementById('popup-closer');

    closer.onclick = () => {
        props.overlay?.setPosition(undefined);
        closer.blur();
        return false;
    };

    emit('initialized', { container });
});


</script>

<style lang="scss" scoped>

</style>
