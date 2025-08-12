<template>
    <div class="box">
        <div v-for="x in getCurrentItems()" class="item">
            <div class="vl" v-if="x.isDivider"></div>
            <img v-else :src="getIconUrl(x.icon)" @click="x.action">
        </div>
    </div>
</template>

<script setup lang="ts">
import { onMounted, computed, ref, defineProps } from 'vue';
import MapMeasure from '@/resource/map/mapMeasure';

let editing = ref(false);
let changesUnsaved = ref(false);
const props = defineProps(['map', 'layers']);

let measure = new MapMeasure(props.map, props.layers, changesUnsaved);

// const emit = defineEmits(['initialized']);

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
    { icon: 'folder.png', action: () => { } },
    { icon: 'save.png', action: () => measure.save() },
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
