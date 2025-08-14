import LayerProps from './layerProps';
import LayerDef from '../layerDef';
import SensorProps from './sensorProps';
import GateImages from './gateImages';
import InfoWindowUtil from './infoWindowUtil';
import StationProps from './stationProps';
import MapDrawing from './mapDrawing';

export default class MapUtil {
    static _eqIntensities = ['1', '2', '3', '4', '5.1', '5.9', '6.1', '6.9', '7'];

    static _lineProps = {
        'layer13': {
            color: "#663300",
        },
        'layer16': {
            color: "#660000",
        },
        'layer17': {
            color: "#009900",
        },
        'layer18': {
            color: "#666600",
        },
        'layer24': {
            color: "#FFFF00",
            zIndex: 2,
            strokew: 3,
        },
        'layer25': {
            color: "#CC0000",
            zIndex: 1,
            strokew: 6,
        },
        'layer26': {
            color: "#00CC00",
            zIndex: 0,
            strokew: 9,
        },
    };

    static _roundTo(num, decimal) {
        let y = Math.pow(10, decimal);
        return Math.round((num + Number.EPSILON) * y) / y;
    }

    static _getNumber(feature, key) {
        return MapUtil._roundTo(Number(feature.get(key)), 2);
    }

    static _getValue(layerId, values, unit) {
        let item = SensorProps.find(layerId);
        if (item === undefined) return '';
        return InfoWindowUtil.createValueText(item.sensorType, values, unit);
    }

    static _getFeatureProps(layerProps, feature, sensorInfo) {
        let unit = layerProps.unit;
        let layerId = layerProps.layerId;
        let lastValue1 = MapUtil._getNumber(feature, "lastValue1");
        let lastValue2 = MapUtil._getNumber(feature, "lastValue2");
        let value = MapUtil._getValue(layerId, [lastValue1, lastValue2], unit);
        let imageIndex = MapUtil._findSensorImageIndex(layerId, lastValue1, feature, sensorInfo);
        return { imageIndex, value };
    }

    // Returns 0 if not found
    static _findSensorImageIndex(layerId, lastValue1, feature, sensorInfo) {
        if (layerId == LayerDef.GATE) {
            return GateImages.getImageIndex(lastValue1, feature, sensorInfo);
        }

        if (layerId == LayerDef.SLIDING_GATE) return lastValue1 <= 5 ? 0 : 1;

        if (layerId == 'layer12') {
            let intensity = feature.get('intensity');
            let index = MapUtil._eqIntensities.indexOf(intensity);
            return index >= 0 ? index : 0;
        }

        let item = SensorProps.find(layerId);
        if (item === undefined) return 0;
        return MapUtil._getSensorStatus(feature, sensorInfo) ?? 0;
    }

    // Returns: null | 0 | 1 | 2
    static _getSensorStatus(feature, sensorInfo) {
        let m = sensorInfo.find(x => x.sensorId === feature.get('id'));
        if (m === undefined) return null;
        let status = m.status;
        if (status === '停用' || status === '缺測') return 1;
        if (status === '警戒') return 2;
        return 0;
    }

    static _selectIcon(layerProps, fp) {
        let index = fp.imageIndex;

        return {
            image: layerProps.images[index],
            url: layerProps.urls[index],
        };
    }

    static _getIcon(layerProps, fp) {
        if (layerProps.urls.length === 0) return undefined;
        let { image, url } = MapUtil._selectIcon(layerProps, fp);
        let scale = 35.0 / Math.max(image.width, image.height);

        return new ol.style.Icon({
            scale,
            anchor: [0.5, 1],
            src: url
        });
    }

    static _getFeatureItem(layerProps, feature, sensorInfo) {
        let fp = MapUtil._getFeatureProps(layerProps, feature, sensorInfo);
        let icon = MapUtil._getIcon(layerProps, fp);
        return { icon, fp };
    }

    static _getText(layerProps, fp) {
        if (!SensorProps.getAllLayers().includes(layerProps.layerId)) return undefined;
        return fp.value;
    }

    static styleFunction(layerProps, getShowValue, sensorInfo) {
        if (layerProps.layerId === LayerDef.DRAWING) return MapDrawing.styleFunc;

        return feature => {
            let fi = MapUtil._getFeatureItem(layerProps, feature, sensorInfo);
            let image = fi.icon;
            let text = getShowValue() ? MapUtil._getText(layerProps, fi.fp) : undefined;

            const styles = {
                'Point': new ol.style.Style({
                    image,
                    text: new ol.style.Text({
                        font: '18px Calibri,sans-serif',
                        fill: new ol.style.Fill({ color: '#000' }),
                        stroke: new ol.style.Stroke({
                            color: '#fff', width: 6
                        }),
                        text
                    }),
                }),
                'LineString': new ol.style.Style({
                    stroke: new ol.style.Stroke({
                        color: layerProps.strokecolor,
                        width: layerProps.strokew,
                    }),
                }),
                'Polygon': new ol.style.Style({
                    stroke: new ol.style.Stroke({
                        color: layerProps.strokecolor,
                        lineDash: [4],
                        width: layerProps.strokew,
                    }),
                    fill: new ol.style.Fill({
                        color: 'rgba(0, 0, 255, 0.1)',
                    }),
                }),
                'MultiLineString': new ol.style.Style({
                    stroke: new ol.style.Stroke({
                        color: layerProps.strokecolor,
                        width: layerProps.strokew,
                    }),
                }),
                'MultiPoint': new ol.style.Style({
                    image: image,
                }),
                'MultiPolygon': new ol.style.Style({
                    stroke: new ol.style.Stroke({
                        color: layerProps.strokecolor,
                        width: layerProps.strokew,
                    }),
                    fill: new ol.style.Fill({
                        color: 'rgba(255, 255, 0, 0.1)',
                    }),
                }),
                'Circle': new ol.style.Style({
                    stroke: new ol.style.Stroke({
                        color: layerProps.strokecolor,
                        width: layerProps.strokew,
                    }),
                    fill: new ol.style.Fill({
                        color: 'rgba(255,0,0,0.2)',
                    }),
                }),
            };

            return styles[feature.getGeometry().getType()];
        };
    }

    static async getLayerProps(layerId) {
        let unit = "";
        let urls = [];
        let sensor = SensorProps.find(layerId);
        let station = StationProps.find(layerId);

        if (sensor !== undefined) {
            urls.push(...SensorProps.getIconUrls(sensor));
            if (sensor.unit) unit = ' ' + sensor.unit;
        } else if (station !== undefined) {
            urls.push(station.icon);
        } else if (layerId === 'layer12') {
            let imgs = MapUtil._eqIntensities.map(x => require(`@/assets/image/intensity${x}.png`));
            urls.push(...imgs);
        } else if (layerId === 'layer14') {
            urls.push(require('@/assets/image/ADSL.png'));
        } else if (layerId === 'layer15') {
            urls.push(require('@/assets/image/4G_.png'));
        } else if (layerId === 'layer19') {
            //堤防管理里程
            urls.push(require('@/assets/image/green-dot_.png'));
        }

        let m = MapUtil._lineProps[layerId];
        let p = new LayerProps();
        p.layerId = layerId;
        p.unit = unit;
        p.strokew = m?.strokew ?? 3;
        p.strokecolor = m?.color ?? "#BB0000";
        p.zIndex = m?.zIndex ?? 50;
        p.urls = urls;
        p.images = [];

        for (let url of urls) {
            p.images.push(await MapUtil.getImage(url));
        }

        return p;
    }

    static async getImage(url) {
        const img = new Image();
        img.src = url;
        await img.decode();
        return img;
    }

    static setCenter(map, lat, lon) {
        map.getView().setCenter(ol.proj.fromLonLat([lon, lat]));
    }

}
