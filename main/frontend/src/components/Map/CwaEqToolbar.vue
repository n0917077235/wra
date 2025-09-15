<template>
    <div class="box">
        地震時間&nbsp;
        <select v-model="selectedTime" @change="updateLayer">
            <option v-for="x in all" :value="x.Time">{{ x.Time }}</option>
        </select>
    </div>
</template>

<script setup lang="ts">
import { onMounted, computed, ref, defineProps, reactive } from 'vue';
import { apiClient } from '@/resource/index.ts';

let props = defineProps(['map', 'layers']);
let selectedTime = ref(undefined);
let all = ref([]);

onMounted(async () => {
    let res = await apiClient.get('/Earthquake/GetCwaEvents');
    let arr = res.data.reverse().map(x => ({ Time: x.Time.replace('T', ' '), Points: x.Points }));
    all.value = arr;
    if (arr.length > 0) selectedTime.value = arr[0].Time;
});

async function updateLayer() {
    let t = encodeURIComponent(selectedTime.value);
    let res = await apiClient.get(`/Earthquake/GetCwaEvent?time=${t}`)
    
    let allLayers = props.layers.get('layer12');
    if (allLayers.length === 0) return null;
    let layer = allLayers[0];
    let source = layer.getSource();
    source.clear();

    let features = new ol.format.GeoJSON().readFeatures(res.data, {
        dataProjection: 'EPSG:4326',
        featureProjection: 'EPSG:3857'
    });

    source.addFeatures(features);
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
    padding: 6px;
}
</style>
