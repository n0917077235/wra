<template>
    <div id="popup" class="ol-popup">
        <a href="#" id="popup-closer" class="ol-popup-closer"></a>
        <div id="popup-content" class="ol-popup-content">
            <div v-if="isSimpleContent" :style="props.content?.style">
                {{ props.content.text }}
            </div>
            <div v-else-if="isSensorContent" class="sensor-content">
                <div class="station-name">{{ props.content.name }}</div>
                <div v-if="props.content.areaName">
                    所屬流域: {{ props.content.areaName }}
                </div>
                <hr>
                <div v-for="sensor in props.content.sensors">
                    <div>感測器:
                        <a v-if="sensor.getSensorPageUrl()" :href="sensor.getSensorPageUrl()" 
                            class="link" target="_blank">
                            {{ sensor.name }}
                        </a>
                        <span v-else>{{ sensor.name }}</span>
                    </div>
                    <div>監測時間: {{ sensor.lastDataTime }}</div>
                    <div>數值: {{ sensor.valueText }}</div>
                    <hr>
                </div>
                <div v-for="camera in props.content.cameras">
                    <div>監視器: {{ camera.name }}</div>
                    <a :href="camera.url" target="_blank">
                        <img :src="camera.url">
                    </a>
                    <hr class="camera-line">
                </div>
            </div>
            <div v-else-if="isCameraContent" class="sensor-content">
                <div class="station-name">{{ props.content.name }}</div>
                <a :href="props.content.url" target="_blank">
                    <img :src="props.content.url">
                </a>
            </div>
        </div>
    </div>
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

let isSimpleContent = computed(() => {
    return props.content?.mode === 'simple';
});

let isSensorContent = computed(() => {
    return props.content?.mode === 'sensor';
});

let isCameraContent = computed(() => {
    return props.content?.mode === 'camera';
});

</script>

<style lang="scss" scoped>
.ol-popup {
    position: absolute;
    background-color: white;
    box-shadow: 0 1px 4px rgba(0, 0, 0, 0.2);
    padding: 15px;
    border-radius: 10px;
    border: 1px solid #cccccc;
    bottom: 12px;
    left: -50px;
    min-width: 125px;
}

.ol-popup:after,
.ol-popup:before {
    top: 100%;
    border: solid transparent;
    content: " ";
    height: 0;
    width: 0;
    position: absolute;
    pointer-events: none;
}

.ol-popup:after {
    border-top-color: white;
    border-width: 10px;
    left: 48px;
    margin-left: -10px;
}

.ol-popup:before {
    border-top-color: #cccccc;
    border-width: 11px;
    left: 48px;
    margin-left: -11px;
}

.ol-popup-closer {
    text-decoration: none;
    position: absolute;
    top: 2px;
    right: 8px;
}

.ol-popup-closer:after {
    content: "✖";
}

.ol-popup-content {
    margin-top: 5px;
    max-height: 400px;
    overflow-y: scroll;
}

.station-name {
    font-weight: bold;
}

.sensor-content {
    width: 260px;
    font-size: 15px;
}

.camera-line {
    margin-top: 3px;
}

.link {
    color: blue;
    text-decoration: underline;
}
</style>
