<template>
    <div id="app" class="app-container">
        <el-tag v-if="title">{{ title }}</el-tag>
        <div id="TGMap" onresize="resizeMap()"  class="cctv-map" v-if="RenderComponent">
        </div>
        <div id="menuToggle" @click="toggleMenu"><img src="http://211.23.16.134:8088/old/images/layers.png"></div>
        <div id="layerControl">
            <div id="baseMaps" class="layer-content">
                <label><input type="checkbox" id="MapType1" :checked="mapType == '1'" @click="changeMapType('1')"> 衛星圖</label><br>
                <label><input type="checkbox" id="MapType2" :checked="mapType == '2'" @click="changeMapType('2')"> 電子地圖</label><br>
            </div>
            <hr>
            <div class="layer-group">
                <h4 v-on:click="toggleLayerGroup('monitoringStations')"> {{ monitoringStationsExpanded ? '-' : '+' }} 監測站圖層 </h4>
                <div id="monitoringStations" class="layer-group-content" style="display:block">
                    <label><input type="checkbox" id="layer1" @click="toggleLayer('layer1')" checked> 淡水河水門監測站<img alt="淡水河水門監測站" id="imgg1" src="http://211.23.16.134:8088/old/images/Station_WaterGate.png" width="20" height="20" style="float:right; margin-right:5px;"></label><br>
                    <label><input type="checkbox" id="layer2" @click="toggleLayer('layer2')" checked> 員山子分洪監測站<img alt="員山子分洪監測站" id="imgg2" src="http://211.23.16.134:8088/old/images/Station_FloodDiversion.png" width="20" height="20" style="float:right; margin-right:5px;"></label><br>
                    <label><input type="checkbox" id="layer3" @click="toggleLayer('layer3')" checked> 堤防安全監測站<img alt="堤防安全監測站" id="imgg3" src="http://211.23.16.134:8088/old/images/Station_BankSafty.png" width="20" height="20" style="float:right; margin-right:5px;"></label><br>
                    <label><input type="checkbox" id="layer4" @click="toggleLayer('layer4')"> 影像監視站<img id="imgg4" alt="影像監視站" src="http://211.23.16.134:8088/old/images/Station_CCTV.png" width="20" height="20" style="float:right; margin-right:5px;"></label><br>
                </div>
            </div>
            <hr>
            <div class="layer-group">
                <h4 > <label v-on:click="toggleLayerGroup('SenserMaps')"> {{ SenserMapsExpanded ? '-' : '+' }} 感測器圖層  </label>
                    <label><input type="checkbox" id="showValues" @click="toggleShowValues()" /> 顯示數值</label>
                </h4> 
                <div id="SenserMaps" class="layer-group-content">
                    <label><input type="checkbox" id="layer5" @click="toggleLayer('layer5')"> 沉陷計<img src="http://maps.google.com/mapfiles/ms/icons/pink-dot.png" width="20" height="20" style="float:right; margin-right:5px;"></label><br>
                    <label><input type="checkbox" id="layer6" @click="toggleLayer('layer6')"> 高灘地水位計<img  src="http://maps.google.com/mapfiles/ms/icons/yellow-dot.png" width="20" height="20" style="float:right; margin-right:5px;"></label><br>
                    <label><input type="checkbox" id="layer7" @click="toggleLayer('layer7')"> 裂縫計<img src="http://maps.google.com/mapfiles/ms/icons/purple-dot.png" width="20" height="20" style="float:right; margin-right:5px;"></label><br>
                    <label><input type="checkbox" id="layer8" @click="toggleLayer('layer8')"> 地震儀<img src="http://maps.google.com/mapfiles/ms/icons/green-dot.png" width="20" height="20" style="float:right; margin-right:5px;"></label><br>
                    <label><input type="checkbox" id="layer9" @click="toggleLayer('layer9')"> 傾斜計<img  src="http://maps.google.com/mapfiles/ms/icons/orange-dot.png" width="20" height="20" style="float:right; margin-right:5px;"></label><br>
                    <label><input type="checkbox" id="layer10" @click="toggleLayer('layer10')"> 水位計<img  src="http://maps.google.com/mapfiles/ms/icons/red-dot.png" width="20" height="20" style="float:right; margin-right:5px;"></label><br>
                    <label><input type="checkbox" id="layer11" @click="toggleLayer('layer11')"> 閘門開度計<img  src="@/assets/image/reddooropen.png" width="20" height="20" style="float:right; margin-right:5px;"></label><br>
                    <label><input type="checkbox" id="layer12" @click="toggleLayer('layer12')"> 等震度圖<img src="@/assets/image/intensity.png" width="20" height="20" style="float:right; margin-right:5px;"></label><br>
                </div>
            </div>
            <hr>
            <div class="layer-group">
                <h4 v-on:click="toggleLayerGroup('NetworkMaps')"> {{ NetworkMapsExpanded ? '-' : '+' }} 資訊傳遞鏈路</h4>
                <div id="NetworkMaps" class="layer-group-content">
                    <label><input type="checkbox" id="layer13" @click="toggleLayer('layer13')"> 光纖線路</label><br>
                    <label><input type="checkbox" id="layer14" @click="toggleLayer('layer14')"> ADSL<img  src="http://211.23.16.134:8088/old/images/ADSL.png" width="20" height="20" style="float:right; margin-right:5px;"></label><br>
                    <label><input type="checkbox" id="layer15" @click="toggleLayer('layer15')"> 4G<img  src="http://211.23.16.134:8088/old/images/4G.png" width="20" height="20" style="float:right; margin-right:5px;"></label><br>
                    <label><input type="checkbox" id="layer16" @click="toggleLayer('layer16')"> WiFi</label><br>
                </div>
            </div>
            <hr>
            <div class="layer-group">
                <h4 v-on:click="toggleLayerGroup('RiverThreeMaps')"> {{ RiverThreeMapsExpanded ? '-' : '+' }} 河川/排水三線</h4>
                <div id="RiverThreeMaps" class="layer-group-content">
                    <label><input type="checkbox" id="layer24" @click="toggleLayer('layer24')"> 治理計畫線</label><br>
                    <label><input type="checkbox" id="layer25" @click="toggleLayer('layer25')"> 用地範圍線</label><br>
                    <label><input type="checkbox" id="layer26" @click="toggleLayer('layer26')"> 河川區域線/排水設施範圍線</label><br>
                </div>
            </div>
            <hr>
            <div class="layer-group">
                <h4 v-on:click="toggleLayerGroup('SafeManageMaps')"> {{ SafeManageMapsExpanded ? '-' : '+' }} 堤防護岸管理圖層</h4>
                <div id="SafeManageMaps" class="layer-group-content">
                    <label><input type="checkbox" id="layer17" @click="toggleLayer('layer17')"> 109年已施作透地雷達</label><br>
                    <label><input type="checkbox" id="layer18" @click="toggleLayer('layer18')"> 河川排水水道</label><br>
                    <label><input type="checkbox" id="layer19" @click="toggleLayer('layer19')"> 堤防管理里程(里程樁點)<img src="http://maps.google.com/mapfiles/ms/icons/green-dot.png" width="20" height="20" style="float:right; margin-right:5px;"></label><br>
                    <label><input type="checkbox" id="layer20" @click="toggleLayer('layer20')"> 堤防管理里程(堤防護岸線)</label><br>
                </div>
            </div>
            <hr>
            <div id="baseMaps" class="layer-content">
                <label><input type="checkbox" id="layer21" @click="toggleLayer('layer21')"> 雙北橫移門即時啟閉<img  src='@/assets/image/blackdooropen.png' width="20" height="20" style="float:right; margin-right:5px;"></label><br>
                <label><input type="checkbox" id="layer22" @click="drawmap()"> 自訂圖層   </label><button id="cleardrawed" @click="cleardrawed()" >  清除 </button> <br>
                <label><input type="checkbox" id="layer23" @click="toggleLayer('layer23')"> 堤防護岸</label><br>
            </div>

            <!-- 可新增更多圖層組 -->
        </div>
    </div>
</template>
<script lang="ts" setup>
    /* eslint-disable */
    var ElementQueries = require('css-element-queries/src/ElementQueries');
    // attaches to DOMLoadContent and does anything for you
    ElementQueries.listen();

    declare namespace TGOS {
        class TGGraphic {
            getProperty(name: string): any;
            setProperty(name: string, value: any): void;
            getGeometry(): TGLineString;
            infowindow?: TGInfoWindow;
        }
        class TGDrawing {
            setMap(map: TGOnlineMap): void;
            setOptions(options: any): void;
            setDefaultControlVisible(visible: boolean): void;
            clearAllGraphics(): void;
            setDrawingMode(mode: string): void;
        }

        class TGMarker {
            constructor(map: TGOnlineMap, position: TGPoint, title: string, icon: TGImage);
            setClickable(clickable: boolean): void;
            setPosition(position: TGPoint): void;
            setTitle(title: string): void;
            setIcon(icon: TGImage): void;
        }

        class TGOnlineMap {
            constructor(element: HTMLElement, coordSys: any, options?: any);
            setCenter(point: TGPoint): void;
            setZoom(zoomLevel: number): void;
            setMapTypeId(typeId: string): void;
        }

        class TGPoint {
            x: number;
            y: number;
            constructor(x: number, y: number);
        }

        class TGSize {
            constructor(width: number, height: number);
        }

        class TGCoordSys {
            static EPSG3857: any;
        }

        class TGImage {
            constructor(url: string, size: TGSize, origin: TGPoint, anchor: TGPoint);
        }

        class TGEvent {
            static addListener(instance: any, eventName: string, handler: Function): void;
            static removeListener(instance: any, eventName: string): void;
        }

        class TGInfoWindow {
            constructor(content: string, position: TGPoint, options?: any);
            open(map: TGOnlineMap, position?: TGPoint): void;
            close(): void;
        }

        class TGLine {
            constructor(map: TGOnlineMap, path: any, options: any);
            setStrokeWeight(n: number): void;
            setStrokeColor(c: string): void;
        }

        class TGLineString {
            constructor(points: TGPoint[]);
        }

        class TGData {
            constructor(options: any);
            addGeoJson(geoJson: object, options: any): TGGraphic[];
            forEachFeature(callback: (feature: any) => void): void;
            setMap(map: TGOnlineMap | null): void;
            getMap(): TGOnlineMap;
            overrideStyle(feature: any, style: any): void;
            getFeatures(): any[];
            getGraphicById(id: string): TGGraphic | undefined;
        }

        class TGMarkerCluster {
            constructor(map: TGOnlineMap, markers: TGMarker[], options: any);
            setMaxZoom(zoomLevel: number): void;
            setVisible(visible: boolean): void;
            setSearchBounds(bounds: number): void;
            redrawAll(force: boolean): void;
        }
    }
    import { inject,computed, nextTick, onMounted, ref, watch, onUnmounted } from 'vue';
    import { useStore } from 'vuex';
    import { apiGetGeoJsonFileNmae, apiGetGeoJsonDataByFileName } from '@/resource/geojson'
    import { apiGetTansuiGps, getGeoJsonLayer, getGetStationGps, apiGetGps } from '@/resource/geojson'
    import { useRoute } from 'vue-router';
    const route = useRoute();
    const isCollapse = inject('isCollapse');

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

    const CameraArea = computed(() => store2.state.image.currentCameraArea);
    const pMap = ref();
    const markerPoint = ref();
    const pTGMarker = ref();
    var pTGMarker2 ;
    var pTGLine = ref();
    const el = document.getElementsByClassName('el');
    const monitoringStationsExpanded = ref(true);
    const SenserMapsExpanded = ref(false);
    const NetworkMapsExpanded = ref(false);
    const SafeManageMapsExpanded = ref(false);
    const RiverThreeMapsExpanded = ref(false);
    var RenderComponent = ref(true);

    onMounted((): void => {
        if (typeof TGOS !== 'undefined') {
            store2.dispatch('drawings/loadDrawings'); // 使用命名空間調用 action
        } else {
            alert('TGOS API 未加載');
        }
        init();
        window.pMap = pMap.value;
        toggleLayer("layer1");
        toggleLayer("layer2");
        toggleLayer("layer3");
        toggleLayer("layer18");
        toggleLayer("layer19");
        window.addEventListener("resize", resizeMap);
        //resizeMap();
        //nextTick(() => {
            //resizeMap();
        //});
        //updateWnd('121.44678969866742', '24.99384208911512');
    });

    onUnmounted(() => {
        // Cleanup event listener when the component is unmounted
        //window.removeEventListener('resize', resizeMap);
    });

    function resizeMap() {
        //alert("hi");
        if (pMap.value) {
            RenderComponent = false;
            nextTick(() => {
                RenderComponent = true;
            });
           // updateWnd(pMap.value.getCenter().x, pMap.value.getCenter().y, "123");
        }
    }
    function cleardrawed() {
        const savedDrawings = store2.state.drawings.drawings;
        if (savedDrawings) {
            store2.dispatch('drawings/clearDrawings');
        }
        if(dm !=null) dm.clearAllGraphics();
        pDatadrawed.setMap(null);
        //drawSavedDrawings();
    }
    var pDatadrawed = new TGOS.TGData({ map: pMap.value });
    function drawSavedDrawings() {
        pDatadrawed.setMap(null);
        pDatadrawed = new TGOS.TGData({ map: pMap.value });
        const savedDrawings = store2.state.drawings.drawings;
        if (savedDrawings) {
            savedDrawings.forEach(drawing => {
                try {
                    var graphics = pDatadrawed.addGeoJson(drawing, { idPropertyName: "GEOJSON" });
                    graphics.forEach((g) => {
                        //if (g == "undefined") alert(g);
                        g.setProperty("strokeColor", "#FF0000");
                        if (g['geometry']["type"].toLowerCase() == "linestring") {
                            var style1 = {
                                strokeColor: "#CC0000",
                                strokeWeight: 4,
                            };


                            //if (type === "TGLineString") alert(titleset);
                            pDatadrawed.overrideStyle(g, style1);
                        }
                    }
                    )
                } catch (e) {
                    //alert(e);
                }
                //dm.setGeoJsonProperties((r) => { alert(r); },JSON.stringify(drawing));
                //alert(JSON.stringify(drawing));
            });
        }
        //alert(savedDrawings.length);
        //const pData = new TGOS.TGData({ map: pMap.value });
        //savedDrawings.forEach(drawing => {
        //    let graphics= pData.addGeoJson(drawing, { idPropertyName: "GEOJSON" });
            //alert(dm.graphic);
            //alert(drawing);
        //});
        //pData.setMap(pMap.value);
    }
    watch(
        () => isCollapse?.value,
        () => {
            if (window.TGOS && window.pMap) {
                resizeMap();
            }
        },
    );
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
//visiablemarker & 2
/* 
    async function visibleMarker(): Promise<void> {
        const data: string = await apiGetGeoJsonFileNmae();
        //GeoJson列表
        lists = data.toString().split(",", 1000);
        for (let i = 0; i < lists.length; i++) {
            try {
                if (i == 2) continue;
                alert(lists[i]);
                const data2: string = await apiGetGeoJsonDataByFileName(lists[i]);
                //alert(data2);
                const jj: JSON = JSON.parse(JSON.stringify(data2));
                //newPoint(x, y, n);
                jj['features'].forEach(fea => {
                    
                   
                    if (fea['geometry']['type'].toLowerCase().valueOf() == ("point").valueOf()) {
                        var x = fea['geometry']['coordinates'][0];
                        var y = fea['geometry']['coordinates'][1];
                        var n = fea['properties']['NAME'];
                        //alert(fea['geometry']['type'].toLowerCase());
                        //alert(x + "," + y + "," + n);
                        newPoint(x, y, n);
                    } else {
                        var ar = new Array<any>();
                        fea['geometry']['coordinates'].forEach(xy => {
                            ar.push(new TGOS.TGPoint(xy[0], xy[1]));
                        })
                        newLine(ar, n);
                    }
                })
            } catch (e) { alert(e.toString()); continue;}
        };
    }
    async function visibleMarker2(): Promise<void> {
        const data: string = await apiGetTansuiGps();
        const jj: JSON = JSON.parse(JSON.stringify(data));
        jj['features'].forEach(fea => {
            if (fea['geometry']['type'].toLowerCase().valueOf() == ("point").valueOf()) {
                var x = fea['geometry']['coordinates'][0];
                var y = fea['geometry']['coordinates'][1];
                var n = fea['properties']['NAME'];
                //alert(fea['geometry']['type'].toLowerCase());
                //alert(x + "," + y + "," + n);
                newPoint(x, y, n);
            } else {
                var ar = new Array < any > ();
                fea['geometry']['coordinates'].forEach(xy => {
                    ar.push(new TGOS.TGPoint(xy[0], xy[1]));
                })
                newLine(ar, n);
            }
        })
    }
 */   
    async function init(): Promise<void> {
        let dmap = document.getElementById('TGMap');
        if (!dmap) return;
        pMap.value = new TGOS.TGOnlineMap(
            dmap,
            TGOS.TGCoordSys.EPSG3857,
            {
                mapTypeControl: false,
                maxZoom: 24,
                minZoom: 7,
            }
        );
        pMap.value.setCenter(new TGOS.TGPoint(121.44678969866742, 24.99384208911512));
        /*
        markerPoint.value = new TGOS.TGPoint(121.44678969866742, 24.99384208911512);
        const markerImg = new TGOS.TGImage(
            'https://api.tgos.tw/TGOS_API/images/marker.png',
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
        */
        pMap.value.setZoom(12);
        getLocation();
        //drawmap();
        // 添加圖層
        
    }
    function getLocation() {
        if (navigator.geolocation) {
           // navigator.geolocation.getCurrentPosition(showPosition);
            navigator.geolocation.getCurrentPosition(showPosition, handleError, {
                enableHighAccuracy: true,
                timeout: 10000,
                maximumAge: 0
            });
        } else {
            alert( "Geolocation is not supported by this browser.");
        }
    }
    function handleError(error: any): void {
        switch (error.code) {
            case error.PERMISSION_DENIED:
                alert("User denied the request for Geolocation.");
                break;
            case error.POSITION_UNAVAILABLE:
                alert("Location information is unavailable.");
                retryGetLocation();
                break;
            case error.TIMEOUT:
                alert("The request to get user location timed out.");
                retryGetLocation();
                break;
            case error.UNKNOWN_ERROR:
                alert("An unknown error occurred.");
                retryGetLocation();
                break;
        }
    }
    function retryGetLocation(): void {
        setTimeout(() => {
            getLocation();
        }, 5000); // 5秒後重試
    }
    function showPosition(position:any) {
        //alert( "你的位置 Latitude: " + position.coords.latitude +
        //    "<br>Longitude: " + position.coords.longitude);
        var mey = position.coords.latitude;
        var mex = position.coords.longitude;
        newPoint(mex, mey, "me");
        pMap.value.setCenter(new TGOS.TGPoint(mex, mey));
        //return new Array({mex, mey});
        //updateWnd(position.coords.latitude, position.coords.longitude);
    }
    var dm:TGOS.TGDrawing;
    function drawmap(): void {
        var el = document.getElementById('layer22') as HTMLInputElement;
        if (el.checked) {
            if (dm == null) {
                dm = new TGOS.TGDrawing();
                dm.setMap(pMap.value);
                dm.setOptions({
                    drawingControl: true,  //顯示繪圖管理器
                    drawingControlOptions: {
                        position: "bottom_left"
                        //設定繪圖管理器顯示於右下角
                    },
                    markerOptions: {  //設定繪製標記的樣式
                        dragable: true,
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
                        case 'polyline':
                            //alert("510");
                            const path = e.overlay.getPath().map(point => [point.x, point.y]);
                            geoJson = {
                                type: 'Feature',
                                geometry: {
                                    type: 'LineString',
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
    
    var ls = Array<TGOS.TGMarker>();
    var ls2 = new Map<string,TGOS.TGInfoWindow>();
    var ls3 = new Map<string, TGOS.TGGraphic>();
    var ls4 = new Map<string, string>();
    function newPoint(x: string, y: string, title: string): void {
        var markerlink = 'https://api.tgos.tw/TGOS_API/images/marker.png';
        if (title == "me") {
            markerlink = 'https://th.bing.com/th/id/OIP.XC-FL3LVGpFPSxhXpEAhpwAAAA?w=167&h=180&c=7&r=0&o=5&dpr=1.5&pid=1.7';
        } 
        const markerImg = new TGOS.TGImage(
            markerlink,
            new TGOS.TGSize(35, 35),
            new TGOS.TGPoint(0, 0),
            new TGOS.TGPoint(20, 50),
        );
        const markerPoint = new TGOS.TGPoint(x, y);
        pTGMarker2 = new TGOS.TGMarker(
            pMap.value,
            markerPoint,
            title,
            markerImg,
        );
        pTGMarker2.setClickable(true);
        var infoContent = title;

        var infoWindowOptions = { pixelOffset: new TGOS.TGSize(5, 5) };
        var infowindow = new TGOS.TGInfoWindow(infoContent, markerPoint, infoWindowOptions);
        //ls2.set(title, infowindow);
        //TGOS.TGEvent.addListener(pTGMarker2, 'click', function () {
        //    ls2.forEach(
        //        function (v, k) {
        //            v.close();
        //        }
        //    )
        //    infowindow.open(pMap.value, markerPoint);
        //});
        ls.push(pTGMarker2);

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
    function toggleLayerGroup(groupId:any) {
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
    var mCluster = ref();
    const markers = Array<TGOS.TGMarker>();
    // 切換圖層
    async function toggleLayer(layerId: string) {
        var el = document.getElementById(layerId) as HTMLInputElement;
        var sv = document.getElementById('showValues') as HTMLInputElement;
        ls2.forEach(function (v, k) { v.close(); });
        if (layers.has(layerId)) {
            var tdata = layers.get(layerId) as TGOS.TGData;
            //alert(el.checked);
            //已有資料，切換顯示
            if (el.checked) tdata.setMap(pMap.value);
            else {
                
                tdata.setMap(null);
            }
        } else if (layerId == 'layer13' && layers.has(layerId + '_1')) {
            for (var i = 1; i < 6; i++) {
                var tdata = layers.get(layerId+'_'+i) as TGOS.TGData;
                if (el.checked) tdata.setMap(pMap.value);
                else tdata.setMap(null);    
            }
        }
        else if(el.checked) {//無資料則新增
            //var data = ref();
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
                case 'layer24':
                    await addLayer(layerId, "/GeoJson/GetGeoJsonDataByFileName?fileName=a河川區域線.geojson");
                    break;
                case 'layer25':
                    await addLayer(layerId, "/GeoJson/GetGeoJsonDataByFileName?fileName=b用地範圍線.geojson");
                    break;
                case 'layer26':
                    await addLayer(layerId, "/GeoJson/GetGeoJsonDataByFileName?fileName=c治理計畫線.geojson");
                    break;
            }
        }
        if (infows.has(layerId)) {
            var infowmap1 = infows.get(layerId) as Map<TGOS.TGPoint, TGOS.TGInfoWindow>;
            if (el.checked && sv.checked) {
                infowmap1.forEach((i, p) => {
                    i.open(pMap.value, p);
                    //alert(p);
                });
            } else {
                infowmap1.forEach((i, p) => {
                    i.close();
                    //alert(p);
                });
            }
        }
    }
    var zi = 0;
    // 獲取並添加圖層
    var layers = new Map(); 
    const addLayer = async (layerId: string, funcName: string) => {
        //const layer = TGOS.TGLayer()
        //建立TGData 並綁定到地圖上
        const pData = new TGOS.TGData({ map: pMap.value });
        //建立Map存放TGInfoWindow 給特定點顯示數值用 
        var infowmap = new Map<TGOS.TGPoint, TGOS.TGInfoWindow>();
        const data = await apiGetGps(funcName);
        const jj: object = JSON.parse(JSON.stringify(data));
        //alert(JSON.stringify(data));
        try {
            var unit = "";
            var id;
            var graphic;
            var markerImg;
            var strokew = 3;
            var strokecolor = "#BB0000";
            var url = "https://api.tgos.tw/TGOS_API/images/marker.png";
            var url2 = "https://api.tgos.tw/TGOS_API/images/marker.png"; //特殊時換圖 開門
            switch(layerId) { //指定圖示  //GeoJson/GetTansuiGps
                case 'layer1':
                    url = "http://211.23.16.134:8088/old/images/Station_WaterGate.png";
                    break;
                case 'layer2':
                    url = "http://211.23.16.134:8088/old/images/Station_FloodDiversion.png";
                    break;
                case 'layer3':
                    url = "http://211.23.16.134:8088/old/images/Station_BankSafty.png";
                    break;
                case 'layer4':
                    url = "http://211.23.16.134:8088/old/images/Station_CCTV.png";
                    break;
                case 'layer14':
                    url = "http://211.23.16.134:8088/old/images/ADSL.png";
                    break;
                case 'layer15':
                    url = "http://211.23.16.134:8088/old/images/4G.png";
                    break;
                case 'layer18': //河川排水水道
                    strokecolor ="#666600";
                    break;
                case 'layer17': //109
                    strokecolor = "#009900"
                    break;
                case 'layer19': //堤防管理里程
                    url = "http://maps.google.com/mapfiles/ms/icons/green-dot.png";
                    break;
                case 'layer13':
                    strokecolor = "#663300"
                    break;
                case 'layer16':
                    strokecolor = "#660000"
                    break;
                case 'layer21':
                    unit=" %"
                    url = require('@/assets/image/blackdoorclose.png');
                    url2 = require('@/assets/image/blackdooropen.png');
                    break;
                case 'layer5'://沉陷計
                    url = "http://maps.google.com/mapfiles/ms/icons/pink-dot.png";
                    unit = " mm";
                    break;
                case 'layer6': //高水位
                    url = "http://maps.google.com/mapfiles/ms/icons/yellow-dot.png";
                    unit = " M";
                    break;
                case 'layer7': //裂縫
                    url = "http://maps.google.com/mapfiles/ms/icons/purple-dot.png";
                    unit = " mm";
                    break;
                case 'layer8':
                    url = "http://maps.google.com/mapfiles/ms/icons/green-dot.png";
                    break;
                case 'layer9': //傾斜
                    url = "http://maps.google.com/mapfiles/ms/icons/orange-dot.png";
                    unit = " °";
                    break;
                case 'layer10'://水位
                    url = "http://maps.google.com/mapfiles/ms/icons/red-dot.png";
                    unit = " M";
                    break;
                case 'layer11':
                    url = require('@/assets/image/reddoorclose.png');
                    url2 = require('@/assets/image/reddooropen.png');
                    unit = " %";
                    break;
                case 'layer2':
                    url = "http://211.23.16.134:8088/old/images/Station_FloodDiversion.png";
                    break;
                case 'layer24':
                    strokecolor = "#FFFF00"
                    zi = 2;
                    strokew = 3;
                    break;
                case 'layer25':
                    strokecolor = "#CC0000"
                    zi = 1;
                    strokew = 6;
                    break;
                case 'layer26':
                    strokecolor = "#00CC00"
                    zi = 0;
                    strokew = 9;
                    break;

                //url = "";
            }
            //alert(pStation);
            graphic = pData.addGeoJson(jj, { idPropertyName: "GEOJSON" }) as Array<TGOS.TGGraphic>;
            //換圖片"./images/Station_CCTV.png"
            //alert(graphic.length);
            const roundTo = function (num, decimal) { return Math.round((num + Number.EPSILON) * Math.pow(10, decimal)) / Math.pow(10, decimal); }
            for (var i = 0; i < graphic.length; i++) {
                zi = zi + 1; //設定zindex

                id = layerId + "_" + graphic[i].getProperty("id");
                var x = graphic[i]['geometry']['x'];
                var y = graphic[i]['geometry']['y'];
                var name = graphic[i].getProperty("name");
                if (name == null) {
                    name = graphic[i].getProperty("NAME");
                    //alert(name);
                }
                var type = graphic[i]['geometry']["type"];
                if (type == "TGLineString")
                    id = layerId + "_" + name+"_"+i;
                var tgpoint =new TGOS.TGPoint(0,0);
                var lastValue = roundTo(Number(graphic[i].getProperty("lastValue")), 2);
                var lastValue1 = roundTo(Number(graphic[i].getProperty("lastValue1")),2);
                var lastValue2 = roundTo(Number(graphic[i].getProperty("lastValue2")),2);
                
                //if (graphic[i].getProperty("unit")) unit = graphic[i].getProperty("unit");
                var value = ""; //infowindow顯示用
                var names;
                var titleset = name; //hover顯示
                graphic[i].setProperty('id', id);
                if (name != null) {
                    names = name.split(';');
                    titleset = names[0];
                } 
                //alert(type + " " + titleset);
                if (layerId == 'layer21') {
                    if (lastValue.toString() == "0") {
                        //alert("000");
                        url = require('@/assets/image/blackdoorclose.png');
                    } else url = require('@/assets/image/blackdooropen.png');;
                    value = lastValue.toString()+unit;
                } else if (layerId == 'layer5') {
                    value = lastValue1 +  unit;//titleset = titleset + " lastValue1:" + lastValue1;
                } else if (layerId == 'layer6') {
                    value = lastValue1 +  unit;//titleset = titleset + " lastValue1:" + lastValue1;
                } else if (layerId == 'layer7') {
                    value = lastValue1 +  unit;//titleset = titleset + " lastValue1:" + lastValue1;
                } else if (layerId == 'layer10') { //水位計
                    value = lastValue1 +  unit; //titleset = titleset + " lastValue1:" + lastValue1;
                } else if (layerId == 'layer9') {
                    if (lastValue1 >= 0) value = lastValue1 +  unit;//titleset = titleset + " lastValue1:" + lastValue1;
                    if (lastValue2 >= 0) value = "," + lastValue2 +  unit;//titleset = titleset + " lastValue2:" + lastValue2;
                } else if (layerId == 'layer11') {
                    if (lastValue1 != 0) {
                        if (lastValue1 == -888) value = " 無此設備 ";//titleset = titleset + " 沒設備 "
                        else if (lastValue1 == -999) value = " 異常 "; //titleset = titleset + " 異常 "
                        else {
                            if (lastValue1 > 100) lastValue1 = 100;
                            else if (lastValue1 < 0) lastValue1 = 0;
                            //titleset = titleset + " lastValue1:" + lastValue1;
                            value = lastValue1 +  unit;
                        };
                    } else value = "_";
                    if (lastValue1 > 0) url = require('@/assets/image/reddooropen.png');
                    else url = require('@/assets/image/reddoorclose.png');
                } else if (layerId == 'layer24' ) {
                    zi = 2;
                } else if (layerId == 'layer25') {
                    zi = 1;
                } else if (layerId == 'layer26') {
                    zi = 0;
                }
                markerImg = new TGOS.TGImage(url, new TGOS.TGSize(35, 35),
                    new TGOS.TGPoint(0, 0), new TGOS.TGPoint(10, 33));
                var style1 = {
                    strokeColor: strokecolor,
                    strokeWeight: strokew,
                    title: titleset,
                    icon: markerImg,
                    clickable: true,
                    zIndex: zi,
                };
                var style2 = {
                    strokeColor: strokecolor,
                    strokeWeight: strokew+2,
                    title: titleset,
                    clickable: true,
                    zIndex: zi,
                }
               
                //if (type === "TGLineString") alert(titleset);
                pData.overrideStyle(graphic[i], style1);
                //線段事件處理
                if (type === "TGLineString") {
                    //alert(type);
                    //alert(titleset);
                    var infowindow = new TGOS.TGInfoWindow(titleset, tgpoint, { pixelOffset: new TGOS.TGSize(0, 0) });
                    //graphic[i].setProperty('zIndex', zi);
                    //alert(graphic[i].getProperty('zIndex'));

                    if (!ls2.has(titleset)) ls2.set(titleset, infowindow);
                    if (!ls3.has(titleset)) ls3.set(titleset, graphic[i]);
                    if (!ls4.has(zi.toString())) ls4.set(zi.toString(), titleset);
                    TGOS.TGEvent.addListener(graphic[i], 'mouseover', function (e) {
                        //alert(e.target);
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
                        //if (! ls3.includes(e.target)) ls3.push(e.target);
                        e.target.setStrokeWeight(e.target.getStrokeWeight()+2.5);
                        //儲存高亮前,再進行亮度上升40%
                        if(!lastcol.has(e.target))
                            lastcol.set(e.target, e.target.getStrokeColor());
                        e.target.setStrokeColor(LightenDarkenColor(e.target.getStrokeColor(), 40));
                        //pData.overrideStyle(e.target, style2);
                        var tgpoint = e.point;
                        
                        //alert(e.target.getId());
                        ls2.forEach(
                            function (v, k) {
                                //if (k == titleset) { alert(k); };
                                //alert(k);
                                if (v instanceof TGOS.TGInfoWindow)
                                    v.close();

                            });
                        ls3.forEach(
                            function (v, k) {
                                var ok = v['geometry'].getPath();
                                var ok1 = e.target.getPath().getPath();
                                if (ok == ok1) {
                                    var tgw = new TGOS.TGInfoWindow(k, e.point, { pixelOffset: new TGOS.TGSize(0, 0) });
                                    tgw.open(pMap.value, e.point);
                                    ls2.set(k, tgw);
                                }
                            }
                        )
                    });
                    TGOS.TGEvent.addListener(graphic[i], 'click', function (e) {
                        //alert(e.target);
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
                        //if (! ls3.includes(e.target)) ls3.push(e.target);
                        e.target.setStrokeWeight(e.target.getStrokeWeight() + 2.5);
                        //儲存高亮前,再進行亮度上升40%
                        if (!lastcol.has(e.target))
                            lastcol.set(e.target, e.target.getStrokeColor());
                        e.target.setStrokeColor(LightenDarkenColor(e.target.getStrokeColor(), 40));
                        //pData.overrideStyle(e.target, style2);
                        var tgpoint = e.point;

                        //alert(e.target.getId());
                        ls2.forEach(
                            function (v, k) {
                                //if (k == titleset) { alert(k); };
                                //alert(k);
                                if (v instanceof TGOS.TGInfoWindow)
                                    v.close();

                            });
                        ls3.forEach(
                            function (v, k) {
                                var ok = v['geometry'].getPath();
                                var ok1 = e.target.getPath().getPath();
                                if (ok == ok1) {
                                    var tgw = new TGOS.TGInfoWindow(k, e.point, { pixelOffset: new TGOS.TGSize(0, 0) });
                                    tgw.open(pMap.value, e.point);
                                    ls2.set(k, tgw);
                                }
                            }
                        )
                    });
                }
                //處理訊息視窗
                if (sensorLayers.includes(layerId) && x != null && y != null) {
                    tgpoint = new TGOS.TGPoint(x, y);
                    var infoContent = value;
                    var infoWindowOptions = { pixelOffset: new TGOS.TGSize(5, 5), zIndex:1001,opacity:0.8};
                    var infowindow = new TGOS.TGInfoWindow(infoContent, tgpoint, infoWindowOptions);
                    if(!infowmap.has(tgpoint))
                        infowmap.set(tgpoint, infowindow);
                }
            }
            //type  TGPoint TGLinString
            layers.set(layerId, pData);
            infows.set(layerId,infowmap);
            pData.setMap(pMap.value);  //設定呈現幾何圖層物件的地圖物件
            var infowmap1 = infows.get(layerId) as Map<TGOS.TGPoint,TGOS.TGInfoWindow>;
            infowmap1.forEach((i,p) => {
                i.open(pMap.value, p);
               //alert(p);
            });

        }
        catch (e) {
            alert(e);
        }
        //const geoJsonLayer = new TGOS.TGGeoJSON(pMap.value, data, { id: layerId });
        //layers[layerId] = geoJsonLayer;
        //geoJsonLayer.setVisible(false); // 初始狀態設置為不可見
    };

    //線段的訊息
    var infows2 = new Map<string, Map<TGOS.TGPoint, TGOS.TGInfoWindow>>(); // 指定infowindows Map()

    //感測器圖層顯示數值?
    var infows = new Map<string, Map<TGOS.TGPoint, TGOS.TGInfoWindow>>(); // 指定infowindows Map()
    let sensorLayers = ['layer5', 'layer6', 'layer7', 'layer9', 'layer10', 'layer11', 'layer21'];

    const showValues = ref(false);
    function toggleShowValues(): void {
        showValues.value = !showValues.value;
        //alert(showValues.value);
        infows.forEach((infowmap, layerId) => {
            var infowmap1 = infows.get(layerId) as Map<TGOS.TGPoint, TGOS.TGInfoWindow>;
            var el = document.getElementById(layerId) as HTMLInputElement;
            infowmap1.forEach((i, p) => {
                if (showValues.value && el.checked) i.open(pMap.value, p);
                else i.close();
            });
            
        });
    }

    function newLine(TGPs: Array<any>, title?: undefined): void {
        pTGLine.value = new TGOS.TGLine(
            pMap.value,
            new TGOS.TGLineString(TGPs), {
                strokeColor: ColorCode(),
            strokeWeight: 4,
            clickable: true}
        );
    }
    function updateWnd(x: number, y: number, title?: string): void {
        //alert("updatewnd");
        //pMap.value.setCenter(new TGOS.TGPoint(x, y));
        //if (title) {
        //    pTGMarker.value.setTitle(title);
        //}
        
                /*
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
        pTGMarker.value.setIcon(markerImg);*/
    }
    //原始顏色
    var lastcol = new Map<TGOS.TGLine, string>();
    //亮度增加
    function LightenDarkenColor(col: string, amt: number) {
        var usePound = false;
       //alert(col);
        if (col.charAt(0) == '#') {
            col = col.slice(1);
            usePound = true;
        }
        var num = parseInt(col, 16);
        var r = (num >> 16) + amt;
        if (r > 255) r = 255;
        else if (r < 0) r = 0;
        var b = ((num >> 8) & 0x00FF) + amt;
        if (b > 255) b = 255;
        else if (b < 0) b = 0;
        var g = (num & 0x0000FF) + amt;
        if (g > 255) g = 255;
        else if (g < 0) g = 0;
        return (usePound ? "#" : "") + (g | (b << 8) | (r << 16)).toString(16);
    }
    /* eslint-disable */
</script>

<style lang="scss" scoped>
    .cctv-map {
        height: calc(100vh - 80px);
        width: 100%;
        position:relative;
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
        max-height: calc(100vh - 155px); /* 限制選單高度，防止溢出視窗 */
        overflow-y: auto; /* 當內容超出最大高度時，顯示滾動條 */
        z-index: 1000; /* 確保選單在頂層顯示 */
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
        background-color: #4cff00; /* 按鈕的背景顏色 */
        border-radius: 50%; /* 圓形按鈕 */
        cursor: pointer;
        z-index: 1001; /* 確保圖示在選單上方 */
    }

</style>
