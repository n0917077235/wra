<template>
    <div id="app" class="app-container">
        <el-tag v-if="title">{{ title }}</el-tag>
        <div id="olmap" class="cctv-map"></div>
        <div id="menuToggle" @click="toggleMenu">
            <img src="@/assets/image/layers.png">
        </div>
        <div id="layerControl">
            <div id="baseMaps" class="layer-content">
                <label>
                    <input type="checkbox" id="MapType1" :checked="mapType == '1'" @click="changeMapType('1')">
                    衛星圖
                </label>
                <br>
                <label>
                    <input type="checkbox" id="MapType2" :checked="mapType == '2'" @click="changeMapType('2')">
                    電子地圖
                </label>
                <br>
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
                        src='@/assets/image/blackdooropen.png' width="20" height="20"
                        style="float:right; margin-right:5px;"></label><br>
                <label><input type="checkbox" id="layer22" @click="drawmap()"> 自訂圖層 </label><button id="cleardrawed"
                    @click="cleardrawed()"> 清除 </button> <br>
                <label><input type="checkbox" id="layer23" @click="toggleLayer('layer23')"> 堤防護岸<img
                        src="@/assets/image/red.png" width="20" height="20"
                        style="float:right; margin-right:5px;"></label><br>
            </div>
        </div>
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
import LayerMap from '@/resource/map/layerMap';
import LayerDef from '@/resource/layerDef';
import SensorProps from '@/resource/map/sensorProps';
import StationProps from '@/resource/map/stationProps';

interface Props {
    title?: string;
    x?: string;
    y?: string;
}

const props = withDefaults(defineProps<Props>(), {
    x: '',
    y: '',
});

const store2 = useStore();
const userId = computed<string>(() => store2.state.user.userId);
const CameraArea = computed(() => store2.state.image.currentCameraArea);
const pMap = ref();
const markerPoint = ref();
const pTGMarker = ref();
const el = document.getElementsByClassName('el');
const monitoringStationsExpanded = ref(true);
const SenserMapsExpanded = ref(false);
const NetworkMapsExpanded = ref(false);
const SafeManageMapsExpanded = ref(false);
const RiverThreeMapsExpanded = ref(false);

const popup = ref({
    content: {},
    container: undefined, // HtmlElement
    overlay: undefined,   // ol.Overlay
});

const changedItems = ref([]);

onMounted(() => {
    store2.dispatch('drawings/loadDrawings'); // 使用命名空間調用 action
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
        .filter(x => x.layerId !== LayerDef.SLIDING_DOOR)
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

function cleardrawed() {
    const savedDrawings = store2.state.drawings.drawings;
    if (savedDrawings) {
        store2.dispatch('drawings/clearDrawings');
    }
    if (dm != null) dm.clearAllGraphics();
    pDatadrawed.setMap(null);
    //drawSavedDrawings();
}

var pDatadrawed = new TGOS.TGData({ map: pMap.value });
function drawSavedDrawings() {
    pDatadrawed.setMap(null);
    pDatadrawed = new TGOS.TGData({ map: pMap.value });
    try {
        const savedDrawings = store2.state.drawings.drawings;
        if (savedDrawings) {
            savedDrawings.forEach(drawing => {

                var graphics = pDatadrawed.addGeoJson(drawing, { idPropertyName: "GEOJSON" });
                graphics.forEach((g) => {
                    //if (g == "undefined") alert(g);
                    g.setProperty("strokeColor", "#FF0000");
                    if (g['geometry']["type"].toLowerCase() == "linestring") {
                        var style1 = {
                            strokeColor: "#CC0000",
                            strokeWeight: 4,
                        };
                        pDatadrawed.overrideStyle(g, style1);
                    }
                }
                )
            });
        }
    } catch (e) { alert(e); }
}

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

const menuVisible = ref(true);

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

// 地圖模式類型
let mapSources = [
    // roadmap
    new ol.source.XYZ({
        url: 'https://mt1.google.com/vt/lyrs=m&hl=zh-TW&x={x}&y={y}&z={z}'
    }),

    // satellite
    new ol.source.XYZ({
        url: 'https://mt1.google.com/vt/lyrs=y&hl=zh-TW&x={x}&y={y}&z={z}'
    })
];

let gmapLayer = new ol.layer.Tile({
    source: mapSources[0]
});

const mapType = ref('2');
const changeMapType = (tp: string) => {
    if (mapType.value == tp) {
        var element = document.getElementById('MapType' + tp) as HTMLInputElement;
        element.checked = true;
    } else {
        // 根據地圖模式切換 TGOS 地圖類型
        mapType.value = tp;
        if (tp == '1') {
            // 切換為衛星圖
            gmapLayer.setSource(mapSources[1]);
        }
        else if (tp == '2') {
            // 切換為電子地圖
            gmapLayer.setSource(mapSources[0]);
        }
    }
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

var dm: TGOS.TGDrawing;

function drawmap() {
    var el = document.getElementById('layer22') as HTMLInputElement;
    if (el.checked) {
        if (dm == null) {
            dm = new TGOS.TGDrawing();
            dm.setMap(pMap.value);
            dm.setOptions({
                drawingControl: true,  //顯示繪圖管理器
                drawingControlOptions: {
                    position: "bottom_left",
                    drawingModes: ["MARKER", "LINESTRING", "EDIT"]
                    //設定繪圖管理器顯示於右下角
                },
                markerOptions: {  //設定繪製標記的樣式
                    //dragable: true,
                    flat: false
                },
                polylineOptions: {  //設定繪製折線的樣式
                    strokeWeight: 3,
                    //strokeDasharray: ". ", //線段樣式
                    strokeColor: '#00AAAA',
                    strokeOpacity: 0.7
                },
                polygonOptions: {  //設定繪製多邊形的樣式
                    fillColor: '#ffdd55',
                    fillOpacity: 0.5,
                    strokeWeight: 2,
                    strokeColor: '#ffdd00',
                    strokeOpacity: 0.5
                },
                circleOptions: {  //設定繪製圓形的樣式
                    fillColor: '#55cc55',
                    fillOpacity: 0.6,
                    strokeWeight: 4,
                    strokeColor: '#22cc22',
                    strokeOpacity: 0.6
                },
                envelopeOptions: {  //設定繪製矩形的樣式
                    fillColor: '#ff5555',
                    fillOpacity: 0.4,
                    strokeWeight: 3,
                    strokeColor: '#ff0000',
                    strokeOpacity: 0.4
                }
            });
            // 監聽繪圖完成事件
            TGOS.TGEvent.addListener(dm, 'overlay_complete', function (e) {
                //alert(e.type);
                let geoJson;
                switch (e.type.toLowerCase()) {
                    case 'marker':
                        geoJson = {
                            type: 'Feature',
                            geometry: {
                                type: 'Point',
                                coordinates: [e.overlay.position.x, e.overlay.position.y]
                            },
                            properties: {}
                        };
                        break;
                    case 'linestring':
                        const path2 = [];
                        const linePath = e.overlay.getPath().getPath();
                        for (let i = 0; i < linePath.length; i++) {
                            const point = linePath[i] as TGOS.TGPoint;
                            path2.push([point.x, point.y]);
                        };
                        geoJson = {
                            type: 'Feature',
                            geometry: {
                                type: 'LineString',
                                coordinates: path2
                            },
                            properties: {}
                        };
                        break;
                    /*  NOT Store
                    case 'polyline':
                        //alert("510");
                        const path = e.overlay.getPath().map(point => [point.x, point.y]);
                        geoJson = {
                            type: 'Feature',
                            geometry: {
                                type: 'Polyline',
                                coordinates: path
                            },
                            properties: {}
                        };
                        break;
                    case 'polygon':
                        const path3 = [];
                        const rings = e.overlay.getPath();
                        for (let i = 0; i < rings.length; i++) {
                            const point = rings[i] as TGOS.TGPoint;
                            path3.push([point.x, point.y]);
                        };
                        geoJson = {
                            type: 'Feature',
                            geometry: {
                                type: 'Polygon',
                                coordinates: path3
                            },
                            properties: {}
                        };
                        break;
                    case 'circle':
                        geoJson = {
                            type: 'Feature',
                            geometry: {
                                type: 'Point',
                                coordinates: e.overlay.center,
                            },
                            properties: {
                                radius: e.overlay.radius,
                            }
                        };
                        break;
                    */
                    // Add more cases if needed for other types like rectangle, etc.
                }

                if (geoJson) {
                    store2.dispatch('drawings/saveDrawing', geoJson);
                    //alert(geoJson);
                }
            });
        }
        store2.dispatch('drawings/loadDrawings');
        drawSavedDrawings();
        dm.setDefaultControlVisible(true);
    } else {
        pDatadrawed.setMap(null);
        if (dm != null) {
            dm.setDefaultControlVisible(false);
            dm.clearAllGraphics();
            dm.setDrawingMode("NONE")
        }
    }
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

//The ColorCode() will give the code every time.
function ColorCode() {
    var makingColorCode = '0123456789ABCDEF';
    var finalCode = '#';
    for (var counter = 0; counter < 6; counter++) {
        finalCode = finalCode + makingColorCode[Math.floor(Math.random() * 16)];
    }
    return finalCode;
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

// Toogle the visibility of a layer group.
// A layer group can represent one or several layers.
// For example, layer group 'layer13' corresponds to 'layer13_1', 'layer13_2', etc
async function toggleLayer(layerGroup) {
    var el = document.getElementById(layerGroup);
    let map = pMap.value;
    let myLayers = layers.get(layerGroup);

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

    if (layerGroup === 'layer22') return [];
    //自訂圖層 await addLayer(layerId, "/GeoJson/GetAdslGps");

    if (layerGroup === 'layer23') return single(byFile("GPS05.json"));
    if (layerGroup === 'layer26') return single(byFile("a河川區域線.geojson"));
    if (layerGroup === 'layer25') return single(byFile("b用地範圍線.geojson"));
    if (layerGroup === 'layer24') return single(byFile("c治理計畫線.geojson"));
    return [];
}

let layers = new LayerMap();

// Returns null if data is not available.
async function getLayers(layerId, funcName) {
    if (layerId == "layer12") {
        let infos = { userId: userId.value, eventTime: "" };
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

        LineHighlight.enable(layerItems);
        InfoWindowUtil.enable(layerItems);
    }
    catch (e) {
        console.log(e);
    }

    getLocation();
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

function updateWnd(x: number, y: number, title?: string): void {
    let map = pMap.value;
    MapUtil.setCenter(map, y, x);

    if (title) {
        pTGMarker.value.setTitle(title);
    }

    markerPoint.value = new TGOS.TGPoint(x, y);
    const markerImg = new TGOS.TGImage(
        'https://api.tgos.tw/TGOS_API/images/marker.png',
        new TGOS.TGSize(50, 40),
        new TGOS.TGPoint(0, 0),
        new TGOS.TGPoint(20, 50),
    );
    pTGMarker.value.setTitle(title);
    alert(title);
    pTGMarker.value.setPosition(markerPoint.value);
    pTGMarker.value.setIcon(markerImg);
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
    width: 250px;
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
