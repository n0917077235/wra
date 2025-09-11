<template>
    <div id="app" class="app-container">
        <div id="olmap" class="cctv-map"></div>
        <div id="menuToggle" @click="toggleMenu">
            <img src="@/assets/image/layers.png">
        </div>
        <div id="layerControl">
            <div id="baseMaps" class="layer-content">
                <div v-for="(text, i) in mapTypeTexts">
                    <input type="radio" :id="'__map_id' + i" :value="i" v-model="mapType" @change="updateMapType" />
                    <label :for="'__map_id' + i">&nbsp;{{ text }}</label>
                </div>
            </div>
            <hr>
            <div class="layer-group">
                <h4 v-on:click="toggleLayerGroup('monitoringStations')"> {{ monitoringStationsExpanded ? '-' : '+' }}
                    監測站圖層 </h4>
                <div id="monitoringStations" class="layer-group-content" style="display:block">
                    <div v-for="x in getStationLayerLines()">
                        <label>
                            <input type="checkbox" :id="x.layerId" @click="toggleLayer(x.layerId)"
                                :checked="x.defaultVisible">
                            {{ x.text }}
                            <img :alt="x.text" :src="x.icon" width="20" height="20" class="icon">
                        </label>
                    </div>
                </div>
            </div>
            <hr>
            <div class="layer-group">
                <h4>
                    <label v-on:click="toggleLayerGroup('SenserMaps')">
                        {{ SenserMapsExpanded ? '-' : '+' }} 感測器圖層
                    </label>
                    <label>
                        <input type="checkbox" id="showValues" @click="toggleShowValues()" />
                        顯示數值
                    </label>
                </h4>
                <div id="SenserMaps" class="layer-group-content">
                    <div v-for="x in getSensorLayerLines()">
                        <label>
                            <input type="checkbox" :id="x.layerId" @click="toggleLayer(x.layerId)">
                            {{ x.text }}
                            <img :alt="x.text" :src="x.icon" width="20" height="20" class="icon">
                        </label>
                    </div>
                </div>
            </div>
            <hr>
            <div class="layer-group">
                <h4 v-on:click="toggleLayerGroup('NetworkMaps')"> {{ NetworkMapsExpanded ? '-' : '+' }} 資訊傳遞鏈路</h4>
                <div id="NetworkMaps" class="layer-group-content">
                    <label><input type="checkbox" id="layer13" @click="toggleLayer('layer13')"> 光纖線路<img
                            src="@/assets/image/darkred.png" width="20" height="20"
                            style="float:right; margin-right:5px;"></label><br>
                    <label><input type="checkbox" id="layer14" @click="toggleLayer('layer14')"> ADSL<img
                            src="@/assets/image/ADSL.png" width="20" height="20"
                            style="float:right; margin-right:5px;"></label><br>
                    <label><input type="checkbox" id="layer15" @click="toggleLayer('layer15')"> 4G<img
                            src="@/assets/image/4G_.png" width="20" height="20"
                            style="float:right; margin-right:5px;"></label><br>
                    <label><input type="checkbox" id="layer16" @click="toggleLayer('layer16')"> WiFi<img
                            src="@/assets/image/darkdarkred.png" width="20" height="20"
                            style="float:right; margin-right:5px;"></label><br>
                </div>
            </div>
            <hr>
            <div class="layer-group">
                <h4 v-on:click="toggleLayerGroup('RiverThreeMaps')"> {{ RiverThreeMapsExpanded ? '-' : '+' }} 河川/排水三線
                </h4>
                <div id="RiverThreeMaps" class="layer-group-content">
                    <label><input type="checkbox" id="layer24" @click="toggleLayer('layer24')"> 治理計畫線<img
                            src="@/assets/image/yellow.png" width="20" height="20"
                            style="float:right; margin-right:5px;"></label><br>
                    <label><input type="checkbox" id="layer25" @click="toggleLayer('layer25')"> 用地範圍線<img
                            src="@/assets/image/red.png" width="20" height="20"
                            style="float:right; margin-right:5px;"></label><br>
                    <label><input type="checkbox" id="layer26" @click="toggleLayer('layer26')"> 河川區域線/排水設施範圍線<img
                            src="@/assets/image/green.png" width="20" height="20"
                            style="float:right; margin-right:5px;"></label><br>
                </div>
            </div>
            <hr>
            <div class="layer-group">
                <h4 v-on:click="toggleLayerGroup('SafeManageMaps')"> {{ SafeManageMapsExpanded ? '-' : '+' }} 堤防護岸管理圖層
                </h4>
                <div id="SafeManageMaps" class="layer-group-content">
                    <label><input type="checkbox" id="layer17" @click="toggleLayer('layer17')"> 109年已施作透地雷達<img
                            src="@/assets/image/darkgreen.png" width="20" height="20"
                            style="float:right; margin-right:5px;"></label><br>
                    <label><input type="checkbox" id="layer18" @click="toggleLayer('layer18')"> 河川排水水道<img
                            src="@/assets/image/darkyellow.png" width="20" height="20"
                            style="float:right; margin-right:5px;"></label><br>
                    <label><input type="checkbox" id="layer19" @click="toggleLayer('layer19')"> 堤防管理里程(里程樁點)<img
                            src="@/assets/image/green-dot_.png" width="20" height="20"
                            style="float:right; margin-right:5px;"></label><br>
                    <label><input type="checkbox" id="layer20" @click="toggleLayer('layer20')"> 堤防管理里程(堤防護岸線)<img
                            src="@/assets/image/red.png" width="20" height="20"
                            style="float:right; margin-right:5px;"></label><br>
                </div>
            </div>
            <hr>
            <div id="baseMaps" class="layer-content">
                <label><input type="checkbox" id="layer21" @click="toggleLayer('layer21')"> 雙北橫移門即時啟閉<img
                        src='@/assets/image/blackdoorhalfopen.png' width="20" height="20"
                        style="float:right; margin-right:5px;"></label><br>
                <label><input type="checkbox" id="layer22" @click="toggleLayer('layer22')"> 我的標記 </label>
                <br>
                <label><input type="checkbox" id="layer23" @click="toggleLayer('layer23')"> 堤防護岸<img
                        src="@/assets/image/red.png" width="20" height="20"
                        style="float:right; margin-right:5px;"></label><br>
            </div>
        </div>
        <map-toolbar v-if="pMap && showDrawingToolbar" :map="pMap" :layers="layers"></map-toolbar>
        <Popup :overlay="popup.overlay" :content="popup.content" @initialized="onPopupInit"></Popup>
    </div>
</template>
<script lang="ts" setup>

import { apiGetGps, apiGetIsoseismal } from '@/resource/geojson';
import MapUtil from '@/resource/map/mapUtil';
import LineHighlight from '@/resource/map/lineHighlight';
import InfoWindowUtil from '@/resource/map/infoWindowUtil';
import { apiGetSensorMoreDataByStationName, apiGetSensorGeneralQueryData } from '@/resource/sensor';
import { computed, nextTick, onMounted, ref, watch } from 'vue';
import { useStore } from 'vuex';
import Popup from './Popup.vue';
import MapToolbar from './MapToolbar.vue';
import LayerMap from '@/resource/map/layerMap';
import LayerDef from '@/resource/layerDef';
import SensorProps from '@/resource/map/sensorProps';
import StationProps from '@/resource/map/stationProps';

const store2 = useStore();
const pMap = ref();
const el = document.getElementsByClassName('el');
const monitoringStationsExpanded = ref(true);
const SenserMapsExpanded = ref(false);
const NetworkMapsExpanded = ref(false);
const SafeManageMapsExpanded = ref(false);
const RiverThreeMapsExpanded = ref(false);
const showDrawingToolbar = ref(false);
const menuVisible = ref(true);
let layers = new LayerMap();

const popup = ref({
    content: {},
    container: undefined, // HtmlElement
    overlay: undefined,   // ol.Overlay
});

const changedItems = ref([]);

onMounted(() => {
    init();
    toggleLayer(LayerDef.TANSUI_STATION);
    toggleLayer(LayerDef.YANSANTZI_STATION);
    toggleLayer(LayerDef.EMBANKMENT_STATION);
    toggleLayer("layer18");
    toggleLayer("layer19");
});

function getStationLayerLines() {
    return StationProps.all;
}

function getSensorLayerLines() {
    let sensors = SensorProps.all
        .filter(x => x.layerId !== LayerDef.SLIDING_GATE)
        .map(x => ({
            layerId: x.layerId,
            text: x.text,
            icon: SensorProps.getIconUrls(x)[0],
        }));

    return [
        ...sensors,
        {
            layerId: 'layer12',
            text: '等震度圖',
            icon: require('@/assets/image/intensityblack.png'),
        },
    ];
}

function toggleMenu() {
    menuVisible.value = !menuVisible.value;
    const layerControl = document.getElementById('layerControl');
    if (!layerControl) return;
    if (menuVisible.value) {
        layerControl.style.display = 'block';
    } else {
        layerControl.style.display = 'none';
    }
}

let mapTypeTexts = ['Google地圖', '衛星圖', '國土測繪地圖'];

// 地圖模式類型
let mapSources = [
    // roadmap
    new ol.source.XYZ({
        url: 'https://mt1.google.com/vt/lyrs=m&hl=zh-TW&x={x}&y={y}&z={z}'
    }),

    // satellite
    new ol.source.XYZ({
        url: 'https://mt1.google.com/vt/lyrs=y&hl=zh-TW&x={x}&y={y}&z={z}'
    }),

    // NLSC map
    new ol.source.XYZ({
        url: "https://wmts.nlsc.gov.tw/wmts/EMAP5/default/EPSG:3857/{z}/{y}/{x}.png"
    }),
];

let gmapLayer = new ol.layer.Tile({
    source: mapSources[0]
});

const mapType = ref(0);

function updateMapType() {
    gmapLayer.setSource(mapSources[mapType.value]);
};

function onPopupInit(e) {
    popup.value.container = e.container;
}

async function init() {
    let elem = document.getElementById('olmap');

    let myOptions = {
        layers: [gmapLayer],
        view: new ol.View({
            center: [0, 0],
            zoom: 12,
        }),
        target: 'olmap'
    }

    let map = new ol.Map(myOptions);
    MapUtil.setCenter(map, 24.99384208911512, 121.44678969866742);
    pMap.value = map;
    getLocation();
}

function getLocation() {
    if (!navigator.geolocation) {
        console.log("Cannot get geolocation from the browser.");
        return;
    }

    navigator.geolocation.getCurrentPosition(showPosition, console.log, {
        enableHighAccuracy: true,
        timeout: 10000,
        maximumAge: 0
    });
}

function showPosition(position) {
    var mey = position.coords.latitude;
    var mex = position.coords.longitude;
    addUserLocationMarker(mex, mey);
    MapUtil.setCenter(pMap.value, mey, mex);
}

function addUserLocationMarker(x: Number, y: Number) {
    let map = pMap.value;

    let markers = new ol.layer.Vector({
        source: new ol.source.Vector(),
        style: new ol.style.Style({
            image: new ol.style.Icon({
                anchor: [0.5, 1],
                src: require('@/assets/image/me4.png'),
            })
        })
    });

    map.addLayer(markers);
    let marker = new ol.Feature(new ol.geom.Point(ol.proj.fromLonLat([x, y])));
    markers.getSource().addFeature(marker);
}

function toggleLayerGroup(groupId) {
    const content = document.getElementById(groupId);
    if (content == null) return;
    if (content.style.display === "none" || content.style.display === "") {
        content.style.display = "block";
    } else {
        content.style.display = "none";
    }
    switch (groupId) {
        case 'monitoringStations':
            monitoringStationsExpanded.value = !monitoringStationsExpanded.value;
            break;
        case 'SenserMaps':
            SenserMapsExpanded.value = !SenserMapsExpanded.value;
            break;
        case 'NetworkMaps':
            NetworkMapsExpanded.value = !NetworkMapsExpanded.value;
            break;
        case 'SafeManageMaps':
            SafeManageMapsExpanded.value = !SafeManageMapsExpanded.value;
            break;
        case 'RiverThreeMaps':
            RiverThreeMapsExpanded.value = !RiverThreeMapsExpanded.value;
            break;
    }
}

const markers = Array<TGOS.TGMarker>();

function updateToolbarVisibility(layerGroup, visible) {
    if (layerGroup === LayerDef.DRAWING) showDrawingToolbar.value = visible;
}

// Toogle the visibility of a layer group.
// A layer group can represent one or several layers.
// For example, layer group 'layer13' corresponds to 'layer13_1', 'layer13_2', etc
async function toggleLayer(layerGroup) {
    var el = document.getElementById(layerGroup);
    let map = pMap.value;
    let myLayers = layers.get(layerGroup);
    updateToolbarVisibility(layerGroup, el.checked);

    if (myLayers !== undefined) {
        // Layer already exists. Toggle visibility.
        myLayers.forEach(x => x.setVisible(el.checked));
    } else if (el.checked) {
        // Add layer
        let toAdd = getLayerGeojsonUrls(layerGroup);

        for (let x of toAdd) {
            await addLayer(x.layerId, x.url, layerGroup);
        }
    }
}

function getLayerGeojsonUrls(layerGroup) {
    let single = url => [{ layerId: layerGroup, url }];
    let byFile = file => `/GeoJson/GetGeoJsonDataByFileName?fileName=${file}`;
    let sensor = SensorProps.find(layerGroup);
    if (sensor !== undefined) return single(sensor.geojsonUrl);
    let station = StationProps.find(layerGroup);
    if (station !== undefined) return single(station.geojsonUrl);

    if (layerGroup === 'layer12') return single("/Earthquake/GetEQEventRangeIntensity");
    // also: 等震度圖 await addLayer(layerId, "等震度圖");

    if (layerGroup === 'layer13') {
        let baseUrl = '/GeoJson/GetGeoJsonDataByFileName?fileName=';

        return [
            { layerId: layerGroup + '_1', url: byFile("三重.json") },
            { layerId: layerGroup + '_2', url: byFile("基隆.json") },
            { layerId: layerGroup + '_3', url: byFile("新店.json") },
            { layerId: layerGroup + '_4', url: byFile("板橋.json") },
            { layerId: layerGroup + '_5', url: byFile("汐止.json") },
        ];
    }

    if (layerGroup === 'layer14') return single("/GeoJson/GetAdslGps");
    if (layerGroup === 'layer15') return single("/GeoJson/Get4GGps");
    if (layerGroup === 'layer16') return single(byFile("GPS08.json"));
    if (layerGroup === 'layer17') return single(byFile("GPS01.json"));
    if (layerGroup === 'layer18') return single(byFile("GPS02.json"));
    if (layerGroup === 'layer19') return single("/GeoJson/GetDamPointGps");
    if (layerGroup === 'layer20') return single(byFile("GPS04.json"));
    if (layerGroup === LayerDef.DRAWING) return single("/GeoJson/GetEmpty");
    if (layerGroup === 'layer23') return single(byFile("GPS05.json"));
    if (layerGroup === 'layer26') return single(byFile("a河川區域線.geojson"));
    if (layerGroup === 'layer25') return single(byFile("b用地範圍線.geojson"));
    if (layerGroup === 'layer24') return single(byFile("c治理計畫線.geojson"));
    return [];
}

// Returns null if data is not available.
async function getLayers(layerId, funcName) {
    if (layerId == "layer12") {
        let infos = { eventTime: "" };
        let v = await apiGetIsoseismal(infos);
        let layers = [];

        if (v == null) {
            //提示未找到等震度圖
            let el = document.getElementById(layerId);
            el.checked = false;
            return null;
        }

        let layer2 = {
            type: "FeatureCollection",
            features: v.infoList.slice(2).map((info) => {
                const [id, name, latitude, longitude, intensity] = info.split(";");
                return {
                    type: "Feature",
                    geometry: {
                        type: "Point",
                        coordinates: [parseFloat(longitude), parseFloat(latitude)],
                    },
                    properties: {
                        id,
                        name,
                        intensity,
                        //image: '@/assets/image/Station_WaterGate_.png', // 根據震度選擇圖片
                    },
                };
            }),
        };

        return [v.geoJson, layer2];
    }

    return [await apiGetGps(funcName)];
}

async function getSensorGeneralInfo(layerId) {
    let item = SensorProps.find(layerId);
    if (item === undefined) return [];
    return await apiGetSensorGeneralQueryData([], [item.sensorType]);
}

async function addLayer(layerId, funcName, layerGroup = null) {
    if (layerGroup === null) layerGroup = layerId;
    let map = pMap.value;
    let tasks = [getLayers(layerId, funcName), getSensorGeneralInfo(layerId)];
    let [myLayers, sensorInfo] = await Promise.all(tasks);

    try {
        let activeLayer = myLayers[0];
        let layerProps = await MapUtil.getLayerProps(layerId);

        let vectorSource = new ol.source.Vector({
            features: new ol.format.GeoJSON().readFeatures(activeLayer, {
                dataProjection: 'EPSG:4326',
                featureProjection: 'EPSG:3857'
            }),
        });

        let vectorLayer = new ol.layer.Vector({
            source: vectorSource,
            zIndex: layerProps.zIndex,
            style: MapUtil.styleFunction(layerProps, () => showValues.value, sensorInfo),
        });

        map.addLayer(vectorLayer);
        addPopupOverlay(map);
        layers.add(layerGroup, vectorLayer);

        let layerItems = {
            layerProps,
            map,
            popup: popup.value,
            vectorLayer,
            changedItems,
        };

        if (layerId !== LayerDef.DRAWING) LineHighlight.enable(layerItems);
        InfoWindowUtil.enable(layerItems);
    }
    catch (e) {
        console.log(e);
    }
}

function addPopupOverlay(map) {
    const overlay = new ol.Overlay({
        element: popup.value.container,
    });

    map.addOverlay(overlay);
    popup.value.overlay = overlay;
}

//感測器圖層顯示數值
const showValues = ref(false);

function toggleShowValues() {
    showValues.value = !showValues.value;
    refreshSensorLayers();
}

function refreshSensorLayers() {
    let all = SensorProps.getAllLayers();

    for (let k of layers.keys()) {
        if (all.includes(k)) {
            for (let x of layers.get(k)) {
                x.getSource().changed();
            }
        }
    }
}
</script>

<style lang="scss" scoped>
.cctv-map {
    height: calc(100vh - 80px);
    width: 100%;
    position: relative;
    top: 0 !important;
    left: 0 !important;
    right: 0 !important;
    bottom: 0 !important;
}

#layerControl {
    position: absolute;
    top: 80px;
    right: 10px;
    background: white;
    padding: 10px;
    border: 1px solid #ccc;
    width: 275px;
    max-height: calc(100vh - 155px);
    /* 限制選單高度，防止溢出視窗 */
    overflow-y: auto;
    /* 當內容超出最大高度時，顯示滾動條 */
    z-index: 1000;
    /* 確保選單在頂層顯示 */
}

.layer-group {
    margin-bottom: 10px;
}

.layer-group h4 {
    margin: 0;
    cursor: pointer;
}

.layer-group-content {
    display: none;
    padding-left: 10px;
}

#menuToggle {
    position: absolute;
    top: 70px;
    right: 5px;
    width: 20px;
    height: 20px;
    background-color: #4cff00;
    /* 按鈕的背景顏色 */
    border-radius: 50%;
    /* 圓形按鈕 */
    cursor: pointer;
    z-index: 1001;
    /* 確保圖示在選單上方 */
}

.modal {
    display: none;
    position: fixed;
    z-index: 1;
    padding-top: 80px;
    left: 0;
    top: 0;
    width: 100%;
    height: 100%;
    overflow: auto;
    background-color: rgb(0, 0, 0);
    background-color: rgba(0, 0, 0, 0.4);
}

.modal-content {
    background-color: #fefefe;
    margin: auto;
    padding: 20px;
    border: 1px solid #888;
    width: 90%;
}

.icon {
    float: right;
    margin-right: 5px;
}
</style>
