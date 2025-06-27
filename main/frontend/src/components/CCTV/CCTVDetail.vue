<template>
    <div class="container w-[100%] lg:w-[75%] h-full">
        <div class="cctv-img-container">
            <CCTVImg>
                <CCTVStatus :CamName="camName"> </CCTVStatus>
            </CCTVImg>
        </div>
        <div class="cctv-map-container">
            <CCTVMap></CCTVMap>
        </div>
    </div>
</template>

<script setup lang="ts">
    import { computed } from 'vue';
    import { useStore } from 'vuex';

    import CCTVImg from './CCTVImg.vue';
    import CCTVMap from './CCTVMap.vue';
    import CCTVStatus from './CCTVStatus.vue';

    const store = useStore();

    const camName = computed<string>(
        () => store.state.image.currentCameraArea?.camName ?? '',
    );
</script>

<style scoped>
    .container {
        display: flex;
        flex-direction: column;
        height: 100%; /* 設置為母容器的高度 */
    }

    .cctv-img-container {
        flex: 2; /* 高度比例 2 */
        width: 100%;
    }

    .cctv-map-container {
        flex: 1; /* 高度比例 1 */
        width: 100%;
    }
</style>
