<template>
    <div id="map" class="map-container"></div>
</template>

<script>
  export default {
    name: "MapComponent",
    mounted() {
        this.methods.initMap();
    },
    methods: {
      initMap() {
          const map = new TGOS.TGOnlineMap(document.getElementById('map'), TGOS.TGCoordSys.EPSG3857);

      // 基礎地圖圖層
      const baseLayer = new TGOS.TGImageServiceLayer(map, '電子地圖', {
        visible: true
      });

      // 自訂圖層
      const layer1 = new TGOS.TGShpLayer('URL_TO_YOUR_LAYER_1', {
        map: map,
        visible: false
      });
      const layer2 = new TGOS.TGShpLayer('URL_TO_YOUR_LAYER_2', {
        map: map,
        visible: false
      });
      const layer3 = new TGOS.TGShpLayer('URL_TO_YOUR_LAYER_3', {
        map: map,
        visible: false
      });
    },
    //toggleLayer(layer, isVisible) {
    //  layer.setVisible(isVisible);
    //}
  }
};
</script>

<style>
    .map-container {
        width: 100%;
        height: 600px;
    }
</style>
