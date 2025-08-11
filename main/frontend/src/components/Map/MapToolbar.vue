<template>
    <div class="box">
        <img v-for="x in items" :src="getIconUrl(x.icon)">
    </div>
</template>

<script setup lang="ts">
import { onMounted, computed } from 'vue';

// const props = defineProps(['overlay', 'content']);
// const emit = defineEmits(['initialized']);

let icons = {
    marker: 'icon_map.svg',
    line: 'line.svg',
    openFile: 'folder.png',
    saveFile: 'save.png',
    circle: 'circle.png',
    pointer: 'cursor.png',
    text: 'text.png',
    area: 'polygon.png',
    delete: 'delete.png',
};

let items = [
    { icon: 'folder.png', action: () => { } },
    { icon: 'save.png', action: () => { } },
    { icon: 'cursor.png', action: () => { } },
    { icon: 'icon_map.png', action: () => { } },
    { icon: 'line.png', action: () => { } },
    { icon: 'polygon.png', action: () => { } },
    { icon: 'circle.png', action: () => { } },
    { icon: 'text.png', action: () => { } },
    { icon: 'delete.png', action: () => { } },
];

function getIconUrl(icon) {
    return require(`@/assets/image/map/${icon}`);
}

onMounted(() => {
    // const container = document.getElementById('popup');
    // const closer = document.getElementById('popup-closer');

    // closer.onclick = () => {
    //     props.overlay?.setPosition(undefined);
    //     closer.blur();
    //     return false;
    // };

    // emit('initialized', { container });
});


</script>

<style lang="scss" scoped>
.box {
    position: absolute;
    bottom: 15px;
    z-index: 999;
}

img {
    width: 32px;
    display: inline-block;
    background-color: #fff;
    border: 1px black solid;
    border-radius: 8px;
    padding: 5px;
    margin: 0 2px;
}
</style>
