<template>
    <div class="box">
        <div v-for="x in getCurrentItems()" class="item">
            <div class="vl" v-if="x.isDivider"></div>
            <img v-else :src="getIconUrl(x.icon)" @click="x.action">
        </div>
    </div>
    <DrawingMenu :mode="fileMode" :load="load" :save="save" :changesUnsaved="changesUnsaved" 
        @closed="onMenuClose">
    </DrawingMenu>
</template>

<script setup lang="ts">
import { onMounted, computed, ref, defineProps } from 'vue';
import MapMeasure from '@/resource/map/mapMeasure';
import DrawingMenu from './DrawingMenu.vue';
import FileMode from '@/resource/map/fileMode.js';

let editing = ref(false);
let changesUnsaved = ref(false);
let fileMode = ref(FileMode.HIDDEN);
let props = defineProps(['map', 'layers']);
let measure = new MapMeasure(props.map, props.layers, changesUnsaved, fileMode);

let items = [
    { icon: 'cursor.png', action: () => measure.setType('none') },
    { icon: 'icon_map.png', action: () => { } },
    { icon: 'line.png', action: () => measure.setType('line') },
    { icon: 'polygon.png', action: () => measure.setType('area') },
    { icon: 'circle.png', action: () => { } },
    { isDivider: true, isEditor: true, },
    { icon: 'text.png', isEditor: true, action: () => { } },
    { icon: 'delete.png', isEditor: true, action: () => { } },
    { isDivider: true },
    { icon: 'folder.png', action: () => { fileMode.value = FileMode.OPEN } },
    { icon: 'save.png', action: () => { fileMode.value = FileMode.SAVE } },
];

function getIconUrl(icon) {
    return require(`@/assets/image/map/${icon}`);
}

function getCurrentItems() {
    return items.filter(x => !x.isEditor || editing.value);
}

onMounted(() => {
    window.addEventListener('beforeunload', e => {
        if (changesUnsaved.value) {
            e.preventDefault();
            return e.returnValue = "Are you sure you want to exit?";
        }
    });
});

function onMenuClose() {
    fileMode.value = FileMode.HIDDEN;
}

async function save(name) {
    await measure.save(name);
    changesUnsaved.value = false;
}

async function load(name) {
    await measure.load(name);
    fileMode.value = FileMode.HIDDEN;
    changesUnsaved.value = false;
}
</script>

<style lang="scss" scoped>
.box {
    position: absolute;
    margin-left: 8px;
    bottom: 15px;
    z-index: 999;
    background-color: #fff;
    border: 1px black solid;
    border-radius: 8px;
    padding: 0 6px;
}

img {
    height: 25px;
    display: inline-block;
    background-color: #fff;
    cursor: pointer;
    padding: 3px;
    margin: 0 2px;
}

.vl {
    display: inline-block;
    vertical-align: top;
    height: 28px;
    border-left: 1px solid black;
}

.item {
    display: inline-block;
}
</style>
