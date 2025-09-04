<template>
    <div class="box">
        <div v-for="x in getCurrentItems()" class="item">
            <div class="vl" v-if="x.isDivider"></div>
            <img v-else :src="getIconUrl(x.icon)" @click="onItemClicked(x)" :style="itemStyle(x)">
        </div>
    </div>
    <DrawingMenu :mode="fileMode" :load="load" :save="save" :changesUnsaved="changesUnsaved" @closed="onMenuClose">
    </DrawingMenu>
</template>

<script setup lang="ts">
import { onMounted, computed, ref, defineProps, reactive } from 'vue';
import MapDrawing from '@/resource/map/mapDrawing';
import DrawingMenu from './DrawingMenu.vue';
import FileMode from '@/resource/map/fileMode.js';

let editing = ref(false);
let changesUnsaved = ref(false);
let fileMode = ref(FileMode.HIDDEN);
let props = defineProps(['map', 'layers']);
let drawing = new MapDrawing(props.map, props.layers, changesUnsaved, fileMode, editing);

let items = reactive([
    { icon: 'cursor.png', action: () => drawing.setType('none'), isTool: true, active: false },
    { icon: 'text.png', action: () => drawing.setType('text'), isTool: true, active: false },
    { icon: 'icon_map.png', action: () => drawing.setType('marker'), isTool: true, active: false },
    { icon: 'line.png', action: () => drawing.setType('length'), isTool: true, active: false },
    { icon: 'polygon.png', action: () => drawing.setType('area'), isTool: true, active: false },
    { isDivider: true },
    { icon: 'folder.png', action: () => { fileMode.value = FileMode.OPEN } },
    { icon: 'save.png', action: () => { fileMode.value = FileMode.SAVE } },
    { icon: 'clear.png', action: () => clear() },
    { icon: 'delete.png', isEditor: true, action: () => drawing.deleteSelected() },
]);

onMounted(() => {
    window.addEventListener('beforeunload', e => {
        if (changesUnsaved.value) {
            e.preventDefault();
            return e.returnValue = "Are you sure you want to exit?";
        }
    });
});

function onItemClicked(x) {
    if (x.isTool) {
        for (let y of items.filter(x => x.isTool)) y.active = false;
        x.active = true;
    }

    x.action();
}

function itemStyle(x) {
    if (x.active) {
        return {
            backgroundColor: '#bce3ff',
            borderRadius: '5px',
        };
    }

    return {};
}

function getIconUrl(icon) {
    return require(`@/assets/image/map/${icon}`);
}

function getCurrentItems() {
    return items.filter(x => !x.isEditor || editing.value);
}

function onMenuClose() {
    fileMode.value = FileMode.HIDDEN;
}

async function save(name) {
    await drawing.save(name);
    changesUnsaved.value = false;
}

async function load(name) {
    await drawing.load(name);
    fileMode.value = FileMode.HIDDEN;
    changesUnsaved.value = false;
}

function clear() {
    if (!confirm('確認清除畫面上所有標記?')) return;
    drawing.clear();
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
