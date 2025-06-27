<template>
    <div>
        <el-tag v-if="title" hidden="hidden">{{ title }}</el-tag>
        <div id="TGMap" class="cctv-map"></div>
    </div>
</template>

<script lang="ts" setup>
    import { computed, nextTick, onMounted, ref, watch } from 'vue';
    import { useStore } from 'vuex';

    interface Props {
        title?: string;
        x?: string;
        y?: string;
    }

    const props = withDefaults(defineProps<Props>(), {
        x: '',
        y: '',
    });

    const store = useStore();

    const CameraArea = computed(() => store.state.image.currentCameraArea);
    const pMap = ref();
    const markerPoint = ref();
    const pTGMarker = ref();

    onMounted((): void => {
        init();
        updateWnd('121.44678969866742', '24.99384208911512');
    });

    watch(CameraArea.value, () => {
        updateWnd(
            CameraArea.value.x,
            CameraArea.value.y,
            CameraArea.value.stationNameA,
        );
    });

    watch(
        () => props.x,
        async () => {
            if (props.x && props.y) {
                await nextTick();
                updateWnd(props.x, props.y);
            }
        },
        { deep: true, immediate: true },
    );

    function init(): void {
        pMap.value = new TGOS.TGOnlineMap(
            document.getElementById('TGMap'),
            TGOS.TGCoordSys.EPSG3857,
        );
        pMap.value.setCenter(new TGOS.TGPoint(121.44678969866742, 24.99384208911512));
        markerPoint.value = new TGOS.TGPoint(121.44678969866742, 24.99384208911512);
        const markerImg = new TGOS.TGImage(
            markerlink,
            new TGOS.TGSize(50, 40),
            new TGOS.TGPoint(0, 0),
            new TGOS.TGPoint(20, 50),
        );
        pTGMarker.value = new TGOS.TGMarker(
            pMap.value,
            markerPoint.value,
            '十河局',
            markerImg,
        );
        pMap.value.setZoom(14);
    }
 // eslint-disable-next-line
    var markerlink = require('@/assets/image/purple-dot_.png');
    function updateWnd(x: string, y: string, title?: undefined): void {
        pMap.value.setCenter(new TGOS.TGPoint(x, y));
        if (title) {
            pTGMarker.value.setTitle(title);
        }

        markerPoint.value = new TGOS.TGPoint(x, y);
        const markerImg = new TGOS.TGImage(
            markerlink,
            new TGOS.TGSize(50, 40),
            new TGOS.TGPoint(0, 0),
            new TGOS.TGPoint(20, 50),
        );
        pTGMarker.value.setTitle(title);
        pTGMarker.value.setPosition(markerPoint.value);
        pTGMarker.value.setIcon(markerImg);
    }
</script>

<style lang="scss" scoped>
    :deep .cctv-map {
        height: 250px !important;
    }
</style>
