<template>
    <div id="app" class="app-container">
        <el-tag v-if="title">{{ title }}</el-tag>
        <div id="olmap" class="cctv-map"></div>
        <div id="menuToggle" @click="toggleMenu"><img src="@/assets/image/layers.png"></div>
        <div id="layerControl">
            <div id="baseMaps" class="layer-content">
                <label><input type="checkbox" id="MapType1" :checked="mapType == '1'" @click="changeMapType('1')">
                    衛星圖</label><br>
                <label><input type="checkbox" id="MapType2" :checked="mapType == '2'" @click="changeMapType('2')">
                    電子地圖</label><br>
            </div>
            <hr>
            <div class="layer-group">
                <h4 v-on:click="toggleLayerGroup('monitoringStations')"> {{ monitoringStationsExpanded ? '-' : '+' }}
                    監測站圖層 </h4>
                <div id="monitoringStations" class="layer-group-content" style="display:block">
                    <label><input type="checkbox" id="layer1" @click="toggleLayer('layer1')" checked> 淡水河水門監測站<img
                            alt="淡水河水門監測站" id="imgg1" src="@/assets/image/Station_WaterGate_.png" width="20" height="20"
                            style="float:right; margin-right:5px;"></label><br>
                    <label><input type="checkbox" id="layer2" @click="toggleLayer('layer2')" checked> 員山子分洪監測站<img
                            alt="員山子分洪監測站" id="imgg2" src="@/assets/image/Station_FloodDiversion_.png" width="20"
                            height="20" style="float:right; margin-right:5px;"></label><br>
                    <label><input type="checkbox" id="layer3" @click="toggleLayer('layer3')" checked> 堤防安全監測站<img
                            alt="堤防安全監測站" id="imgg3" src="@/assets/image/Station_BankSafty_.png" width="20" height="20"
                            style="float:right; margin-right:5px;"></label><br>
                    <label><input type="checkbox" id="layer4" @click="toggleLayer('layer4')"> 影像監視站<img id="imgg4"
                            alt="影像監視站" src="@/assets/image/Station_CCTV_.png" width="20" height="20"
                            style="float:right; margin-right:5px;"></label><br>
                </div>
            </div>
            <hr>
            <div class="layer-group">
                <h4>
                    <label v-on:click="toggleLayerGroup('SenserMaps')"> {{ SenserMapsExpanded ? '-' : '+' }} 感測器圖層
                    </label>
                    <label><input type="checkbox" id="showValues" @click="toggleShowValues()" /> 顯示數值</label>
                </h4>
                <div id="SenserMaps" class="layer-group-content">
                    <label><input type="checkbox" id="layer5" @click="toggleLayer('layer5')"> 沉陷計<img
                            src="@/assets/image/pink-dot_.png" width="20" height="20"
                            style="float:right; margin-right:5px;"></label><br>
                    <label><input type="checkbox" id="layer6" @click="toggleLayer('layer6')"> 高灘地水位計<img
                            src="@/assets/image/yellow-dot_.png" width="20" height="20"
                            style="float:right; margin-right:5px;"></label><br>
                    <label><input type="checkbox" id="layer7" @click="toggleLayer('layer7')"> 裂縫計<img
                            src="@/assets/image/purple-dot_.png" width="20" height="20"
                            style="float:right; margin-right:5px;"></label><br>
                    <label><input type="checkbox" id="layer8" @click="toggleLayer('layer8')"> 地震儀<img
                            src="@/assets/image/green-dot_.png" width="20" height="20"
                            style="float:right; margin-right:5px;"></label><br>
                    <label><input type="checkbox" id="layer9" @click="toggleLayer('layer9')"> 傾斜計<img
                            src="@/assets/image/orange-dot_.png" width="20" height="20"
                            style="float:right; margin-right:5px;"></label><br>
                    <label><input type="checkbox" id="layer10" @click="toggleLayer('layer10')"> 水位計<img
                            src="@/assets/image/red-dot_.png" width="20" height="20"
                            style="float:right; margin-right:5px;"></label><br>
                    <label><input type="checkbox" id="layer11" @click="toggleLayer('layer11')"> 閘門開度計<img
                            src="@/assets/image/reddooropen.png" width="20" height="20"
                            style="float:right; margin-right:5px;"></label><br>
                    <label><input type="checkbox" id="layer12" @click="toggleLayer('layer12')"> 等震度圖<img
                            src="@/assets/image/intensityblack.png" width="20" height="20"
                            style="float:right; margin-right:5px;"></label><br>
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

            <!-- 可新增更多圖層組 -->
        </div>
        <div id="alarmbox3" class="modal">
            <div class="modal-content" style="width: 800px; height: 500px; overflow-y: auto; border: 1px solid #ccc;">
                <el-dialog v-model="showAlarmMsg3" title="Alarm Message"
                    style="width: 800px; height: 500px; overflow-y: auto; border: 1px solid #ccc;">
                    <span class="closeBtn3">&times;</span>
                    <canvas id="AlarmCanvas3" width="800" height="1680"></canvas>
                </el-dialog>
            </div>
        </div>
    </div>
</template>
<script lang="ts" setup>

import { apiGetGps, apiGetIsoseismal } from '@/resource/geojson';
import MapUtil from '@/resource/map/mapUtil';
import LineHighlight from '@/resource/map/lineHighlight';
import { apiGetSensorMoreDataByStationName } from '@/resource/sensor';
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

const store2 = useStore();
const userId = computed<string>(() => store2.state.user.userId);
const CameraArea = computed(() => store2.state.image.currentCameraArea);
const pMap = ref();
const markerPoint = ref();
const pTGMarker = ref();
var pTGMarker2;
var pTGLine = ref();
const el = document.getElementsByClassName('el');
const monitoringStationsExpanded = ref(true);
const SenserMapsExpanded = ref(false);
const NetworkMapsExpanded = ref(false);
const SafeManageMapsExpanded = ref(false);
const RiverThreeMapsExpanded = ref(false);
// 1131111控制警告對話框的顯示
const showAlarmMsg2 = ref(false);
const alarmrtn2 = ref("");
// 1131205 canvas evnetlisten
const canvasevent = ref(false);

onMounted((): void => {
    //if (typeof TGOS !== 'undefined') {
    store2.dispatch('drawings/loadDrawings'); // 使用命名空間調用 action
    //} else {
    //    alert('TGOS API 未加載');
    //}
    init();
    toggleLayer("layer1");
    toggleLayer("layer2");
    toggleLayer("layer3");
    toggleLayer("layer18");
    toggleLayer("layer19");
    //updateWnd('121.44678969866742', '24.99384208911512');
    document.getElementsByClassName("closeBtn3")[0]?.addEventListener('click', function () {
        const modal = document.getElementById("alarmbox3");
        if (modal) {
            modal.style.display = "none";
        }
        const canvas = document.getElementById('AlarmCanvas3');

        if (canvas) {
            const originalWidth = canvas.width;  // 儲存原始寬度
            const originalHeight = canvas.height; // 儲存原始高度
            canvas.width = originalWidth;        // 重新設定寬度（自動清空）
            canvas.height = originalHeight;      // 重新設定高度（自動清空）
        }
    });
});
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
const products = ref(null);
var lists: string[];
//manu收合
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
            pMap.value.setMapTypeId('ROADMAP'); // 確保 'ROADMAP' 是正確的 ID
        }
        else if (tp == '2') {
            // 切換為電子地圖
            pMap.value.setMapTypeId('TGOSMAP'); // 確保 'TGOSIMAGE' 是正確的 ID
        }
    }
};

async function init(): Promise<void> {
    let elem = document.getElementById('olmap');

    let gmapLayer = new ol.layer.Tile({
        source: new ol.source.XYZ({
            url: 'https://mt1.google.com/vt/lyrs=m&hl=zh-TW&x={x}&y={y}&z={z}'
        }), name: 'roadmap', title: "地圖"
    });

    let layers = [gmapLayer];
    let layerGroupMap = new ol.layer.Group({ title: 'map', layers });

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

var ls2 = new Map<string, TGOS.TGInfoWindow>();
var ls3 = new Map<string, TGOS.TGGraphic>();

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

// 切換圖層
async function toggleLayer(layerId: string) {
    var el = document.getElementById(layerId) as HTMLInputElement;
    var sv = document.getElementById('showValues') as HTMLInputElement;
    ls2.forEach(function (v, k) { v.close(); });
    let map = pMap.value;

    if (layers.has(layerId)) {
        // Layer already exists. Toggle visibility.
        let vectorLayer = layers.get(layerId);
        vectorLayer.setVisible(el.checked);
    } else if (layerId == 'layer13' && layers.has(layerId + '_1')) {
        for (var i = 1; i < 6; i++) {
            var vectorLayer = layers.get(layerId + '_' + i) as TGOS.TGData;
            if (vectorLayer) {
                if (el.checked) vectorLayer.setMap(pMap.value);
                else vectorLayer.setMap(null);
            }
        }
    } else if (el.checked) {
        //無資料則新增
        switch (layerId) {
            case 'layer1':
                await addLayer(layerId, "/GeoJson/GetTansuiGps");
                break;
            case 'layer2':
                await addLayer(layerId, "/GeoJson/GetYansantziGps");
                break;
            case 'layer3':
                await addLayer(layerId, "/GeoJson/GetBankGps");
                break;
            case 'layer4':
                await addLayer(layerId, "/GeoJson/GetCCTVGps");
                break;
            case 'layer5':
                await addLayer(layerId, "/GeoJson/GetSensorGps?sensorType=sink");
                break;
            case 'layer6':
                await addLayer(layerId, "/GeoJson/GetSensorGps?sensorType=waterlevel2");
                break;
            case 'layer7':
                await addLayer(layerId, "/GeoJson/GetSensorGps?sensorType=crack");
                break;
            case 'layer8':
                await addLayer(layerId, "/GeoJson/GetSensorGps?sensorType=earthquake");
                break;
            case 'layer9':
                await addLayer(layerId, "/GeoJson/GetSensorGps?sensorType=slope");
                break;
            case 'layer10':
                await addLayer(layerId, "/GeoJson/GetSensorGps?sensorType=waterlevel");
                break;
            case 'layer11':
                await addLayer(layerId, "/GeoJson/GetSensorGps?sensorType=gate");
                break;
            case 'layer12':
                await addLayer(layerId, "/Earthquake/GetEQEventRangeIntensity")
                // 等震度圖 await addLayer(layerId, "等震度圖");
                break;
            case 'layer13':
                await addLayer(layerId + '_1', "/GeoJson/GetGeoJsonDataByFileName?fileName=三重.json");
                await addLayer(layerId + '_2', "/GeoJson/GetGeoJsonDataByFileName?fileName=基隆.json");
                await addLayer(layerId + '_3', "/GeoJson/GetGeoJsonDataByFileName?fileName=新店.json");
                await addLayer(layerId + '_4', "/GeoJson/GetGeoJsonDataByFileName?fileName=板橋.json");
                await addLayer(layerId + '_5', "/GeoJson/GetGeoJsonDataByFileName?fileName=汐止.json");
                break;
            case 'layer14':
                await addLayer(layerId, "/GeoJson/GetAdslGps");
                break;
            case 'layer15':
                await addLayer(layerId, "/GeoJson/Get4GGps");
                break;
            case 'layer16':
                await addLayer(layerId, "/GeoJson/GetGeoJsonDataByFileName?fileName=GPS08.json");
                break;
            case 'layer17':
                await addLayer(layerId, "/GeoJson/GetGeoJsonDataByFileName?fileName=GPS01.json");
                break;
            case 'layer18':
                await addLayer(layerId, "/GeoJson/GetGeoJsonDataByFileName?fileName=GPS02.json");
                break;
            case 'layer19':
                await addLayer(layerId, "/GeoJson/GetDamPointGps");
                break;
            case 'layer20':
                await addLayer(layerId, "/GeoJson/GetGeoJsonDataByFileName?fileName=GPS04.json");
                break;
            case 'layer21':
                await addLayer(layerId, "/GeoJson/GetTaipeiGateGps");
                break;
            case 'layer22':
                //自訂圖層 await addLayer(layerId, "/GeoJson/GetAdslGps");
                break;
            case 'layer23':
                await addLayer(layerId, "/GeoJson/GetGeoJsonDataByFileName?fileName=GPS05.json");
                break;
            case 'layer26':
                await addLayer(layerId, "/GeoJson/GetGeoJsonDataByFileName?fileName=a河川區域線.geojson");
                break;
            case 'layer25':
                await addLayer(layerId, "/GeoJson/GetGeoJsonDataByFileName?fileName=b用地範圍線.geojson");
                break;
            case 'layer24':
                await addLayer(layerId, "/GeoJson/GetGeoJsonDataByFileName?fileName=c治理計畫線.geojson");
                break;
        }
    }

    if (infows.has(layerId)) {
        var infowmap1 = infows.get(layerId) as Map<TGOS.TGPoint, TGOS.TGInfoWindow>;
        if (el.checked && sv.checked) {
            infowmap1.forEach((i, p) => {
                i.open(pMap.value, p);
            });
        } else {
            infowmap1.forEach((i, p) => {
                i.close();
            });
        }
    }
}

var zi = 0;
// 獲取並添加圖層
var layers = new Map();

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

async function addLayer(layerId, funcName) {
    console.log(`adding layer ${layerId}`);
    let map = pMap.value;

    // 建立Map存放TGInfoWindow 給特定點顯示數值用 
    var infowmap = new Map<TGOS.TGPoint, TGOS.TGInfoWindow>();
    let myLayers = await getLayers(layerId, funcName);

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
            style: MapUtil.styleFunction(layerProps, activeLayer),
        });

        map.addLayer(vectorLayer);
        layers.set(layerId, vectorLayer);
        infows.set(layerId, infowmap);
        LineHighlight.enable(layerProps, map);
        return;

        var graphic;
        const pData = new TGOS.TGData({ map: pMap.value });

        for (var i = 0; i < graphic.length; i++) {
            zi = zi + 1; //設定zindex
            var x = graphic[i]['geometry']['x'];
            var y = graphic[i]['geometry']['y'];
            var name = graphic[i].getProperty("name");
            if (name == null) name = graphic[i].getProperty("NAME");
            var type = graphic[i]['geometry']["type"];
            var tgpoint = new TGOS.TGPoint(0, 0);
            var value = ""; //infowindow顯示用
            var names;
            var titleset = name; //hover顯示
            if (name != null) {
                names = name.split(';');
                titleset = names[0];
            }

            var style1 = {
                title: titleset,
                clickable: true,
                // zIndex: zi,
            };

            pData.overrideStyle(graphic[i], style1);

            //線段事件處理
            if (type === "TGLineString") {
                var infowindow = new TGOS.TGInfoWindow(titleset, tgpoint, { pixelOffset: new TGOS.TGSize(0, 0) });
                if (!ls2.has(titleset)) ls2.set(titleset, infowindow);
                if (!ls3.has(titleset)) ls3.set(titleset, graphic[i]);

                function highlightLine(e) {
                    lastcol.forEach(function (v, k) {
                        if (k.getStrokeWeight() == 5.5) {
                            k.setStrokeWeight(3);
                        } else if (k.getStrokeWeight() == 8.5)
                            k.setStrokeWeight(6);
                        else if (k.getStrokeWeight() == 11.5) {
                            k.setStrokeWeight(9);
                        }
                        k.setStrokeColor(v);
                    });

                    e.target.setStrokeWeight(e.target.getStrokeWeight() + 2.5);
                    //儲存高亮前,再進行亮度上升40%
                    if (!lastcol.has(e.target))
                        lastcol.set(e.target, e.target.getStrokeColor());
                    e.target.setStrokeColor(lightenColor(e.target.getStrokeColor(), 40));
                    var tgpoint = e.point;

                    ls2.forEach((v, k) => {
                        if (v instanceof TGOS.TGInfoWindow) v.close();
                    });

                    ls3.forEach((v, k) => {
                        var ok = v['geometry'].getPath();
                        var ok1 = e.target.getPath().getPath();
                        if (ok != ok1) return;
                        var tgw = new TGOS.TGInfoWindow(k, e.point, { pixelOffset: new TGOS.TGSize(0, 0) });
                        tgw.open(pMap.value, e.point);
                        ls2.set(k, tgw);
                    });
                }

                TGOS.TGEvent.addListener(graphic[i], 'mouseover', e => highlightLine(e));
                TGOS.TGEvent.addListener(graphic[i], 'click', e => highlightLine(e));
            }

            //處理訊息視窗
            if (sensorLayers.includes(layerId) && x != null && y != null) {
                tgpoint = new TGOS.TGPoint(x, y);
                var infoContent = value;
                var infoWindowOptions = { pixelOffset: new TGOS.TGSize(5, 5), zIndex: 1001, opacity: 0.8, maxWidth: 125 };
                var infowindow = new TGOS.TGInfoWindow(infoContent, tgpoint, infoWindowOptions);
                if (!infowmap.has(tgpoint))
                    infowmap.set(tgpoint, infowindow);
            }

            //點擊查詢 
            if (layerId == 'layer1' || layerId == 'layer2' || layerId == 'layer3') {
                TGOS.TGEvent.addListener(graphic[i], 'click', async function (e) {
                    //alert(e.target.getTitle());
                    const infos: string = await apiGetSensorMoreDataByStationName(e.target.getTitle());
                    alarmrtn2.value = infos;
                    setTimeout(() => {
                        const box = document.getElementById('alarmbox3') as HTMLElement;
                        const Canvas = document.getElementById('AlarmCanvas3');

                        const ctx = Canvas?.getContext('2d');
                        if (box && ctx) {
                            //ctx.clearRect(0, 0, Canvas.clientWidth, Canvas.clientHeight);
                            box.style.display = "block";
                            ctx.font = '20px Arial';

                            ctx.fillStyle = 'black';
                            //var l = alarmrtn2.value.length / 30;
                            var lst = infos.toString().split(",");
                            var cou = 0;
                            ctx.fillText(e.target.getTitle(), 20, 50 + cou++ * 30);
                            var iscctv = false;
                            var hadarea = false;
                            while (lst2.value.length > 0) {
                                lst2.value.pop(); // 每次移除最後一個元素
                            }

                            for (i = 0; i < lst.length; i++) {
                                if (!hadarea && !iscctv && lst[i].includes("areaName")) {
                                    var a = lst[i].split(":")[1].replace("\"", "").replace("\"", "");
                                    if (a == "") continue;
                                    ctx.fillText("所屬流域:" + a, 20, 50 + cou++ * 30);
                                    hadarea = true;
                                } else if (lst[i].includes("sensorNameA")) {
                                    // 繪製一條分隔線
                                    ctx.beginPath();          // 開始繪圖
                                    ctx.moveTo(20, 50 + cou * 30); // 起點 (x: 0, y: 畫布高度的中點)
                                    ctx.lineTo(Canvas?.width, 50 + cou++ * 30); // 終點 (x: 畫布寬度, y: 畫布高度的中點)
                                    ctx.stroke();
                                    var a = lst[i].split(":")[1].replace("\"", "").replace("\"", "");
                                    if (a == "") continue;
                                    ctx.fillText("" + a, 20, 50 + cou++ * 30);
                                } else if (!iscctv && lst[i].includes("lastDataTime\"")) {
                                    //ctx.fillText(lst[i], 20, 50 + cou++ * 30);
                                    var a = lst[i].substring(lst[i].indexOf(":")).replace("\"", "").replace("\"", "");
                                    if (a == "") continue;
                                    ctx.fillText("即時監測時間" + a, 20, 50 + cou++ * 30);
                                } else if (!iscctv && lst[i].includes("value1")) {
                                    var a = lst[i].split(":")[1].replace("\"", "").replace("\"", "");
                                    if (a == "") continue;
                                    ctx.fillText("數值:" + a, 20, 50 + cou++ * 30);
                                } else if (lst[i].includes("sensorTypeName\":\"CCTV")) {
                                    iscctv = true;
                                } else if (lst[i].includes("stream") && iscctv) {
                                    var link = lst[i].substring(lst[i].indexOf(":") + 1).replace("\"", "").replace("\"", "");
                                    if (link == "null") continue;
                                    ctx.fillStyle = 'blue';
                                    ctx.fillText("影像:" + link, 20, 50 + cou++ * 30);
                                    ctx.fillStyle = 'black';
                                    lst2.value.unshift(link + "|" + cou * 30);
                                }
                            }

                            if (!canvasevent.value && Canvas) {
                                canvasevent.value = true;
                                //Canvas.removeEventListener('click', handleCanvasClick); // 移除舊的監聽器
                                Canvas.addEventListener('click', handleCanvasClick);    // 添加新的監聽器
                            }

                        }
                    }, 100);
                    // 彈出視窗
                    if (alarmrtn2.value)
                        showAlarmMsg2.value = true;

                })
            }
        }

        layers.set(layerId, pData);
        infows.set(layerId, infowmap);
        if (pData) {
            pData.setMap(pMap.value);  //設定呈現幾何圖層物件的地圖物件
        }
        var infowmap1 = infows.get(layerId) as Map<TGOS.TGPoint, TGOS.TGInfoWindow>;
        infowmap1.forEach((i, p) => {
            i.open(pMap.value, p);
        });
    }
    catch (e) {
        console.log(e);
    }

    getLocation();
}

const lst2 = ref(Array<string>());
// 定義事件處理函數
const handleCanvasClick = (event) => {
    const Canvas = document.getElementById('AlarmCanvas3');
    const rect = Canvas?.getBoundingClientRect();
    const x = event.clientX - rect.left;
    const y = event.clientY - rect.top;

    // 檢查點擊是否在連結區域內
    lst2.value.forEach((l) => {
        const link = l.split("|")[0];
        const xy = parseInt(l.split("|")[1], 10);
        if (y >= xy && y <= 30 + xy) {
            window.open(link, '_blank'); // 在新分頁打開連結
        }
    });
};

//感測器圖層顯示數值?
var infows = new Map<string, Map<TGOS.TGPoint, TGOS.TGInfoWindow>>(); // 指定infowindows Map()
let sensorLayers = ['layer5', 'layer6', 'layer7', 'layer9', 'layer10', 'layer11', 'layer21'];
const showValues = ref(false);

function toggleShowValues() {
    showValues.value = !showValues.value;

    infows.forEach((infowmap, layerId) => {
        var infowmap1 = infows.get(layerId) as Map<TGOS.TGPoint, TGOS.TGInfoWindow>;
        var el = document.getElementById(layerId) as HTMLInputElement;
        infowmap1.forEach((i, p) => {
            if (showValues.value && el.checked) i.open(pMap.value, p);
            else i.close();
        });

    });
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

.closeBtn3 {
    position: relative;
    top: 0px;
    /* 距離頂部的距離，可以根據需要調整 */
    left: 0px;
    /* 靠右對齊，距離右邊的距離，可以根據需要調整 */
    font-size: 24px;
    /* 調整按鈕大小 */
    cursor: pointer;
    /* 鼠標懸停時顯示手型 */
}
</style>
