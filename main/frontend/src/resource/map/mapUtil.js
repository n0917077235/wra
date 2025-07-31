import LayerProps from './layerProps';
import MathUtil from '../mathUtil';
import SensorDef from '../sensorDef';
import SensorProps from './sensorProps';

export default class MapUtil {
    static _eqIntensities = ['1', '2', '3', '4', '5.1', '5.9', '6.1', '6.9', '7'];
    static sensorLayers = ['layer5', 'layer6', 'layer7', SensorDef.SLOPE,
        'layer10', 'layer11', 'layer21'];

    static _roundTo(num, decimal) {
        let y = Math.pow(10, decimal);
        return Math.round((num + Number.EPSILON) * y) / y;
    }

    static _getNumber(feature, key) {
        return MapUtil._roundTo(Number(feature.get(key)), 2);
    }

    static _getFeatureProps(layerProps, feature, sensorInfo) {
        let unit = layerProps.unit;
        let layerId = layerProps.layerId;
        let lastValue = MapUtil._getNumber(feature, "lastValue");
        let lastValue1 = MapUtil._getNumber(feature, "lastValue1");
        let lastValue2 = MapUtil._getNumber(feature, "lastValue2");
        let value = "";
        let imageIndex = 0;

        if (layerId == 'layer5') {
            value = lastValue1 + unit;
        } else if (layerId == 'layer6') {
            value = lastValue1 + unit;
        } else if (layerId == 'layer7') {
            value = lastValue1 + unit;
        } else if (layerId == 'layer10') {
            // 水位計
            value = lastValue1 + unit;
        } else if (layerId == SensorDef.SLOPE) {
            value = lastValue1 + unit + ", " + lastValue2 + unit;
        } else if (layerId == 'layer11') {
            // Gate opening
            value = MapUtil._mapGateOpening(lastValue1, unit);
            imageIndex = lastValue1 <= 5 ? 0 : 1;
        } else if (layerId == 'layer12') {
            let intensity = feature.get('intensity');
            let index = MapUtil._eqIntensities.indexOf(intensity);
            if (index >= 0) imageIndex = index;
        } else if (layerId == 'layer21') {
            imageIndex = lastValue1 <= 5 ? 0 : 1;
            value = lastValue1.toString() + unit;
        }

        let s = MapUtil._findSensorImageIndex(layerId, feature, sensorInfo);
        if (s !== null) imageIndex = s;
        return { imageIndex, value };
    }

    // Returns null if not applicable
    static _findSensorImageIndex(layerId, feature, sensorInfo) {
        let item = SensorProps.find(layerId);
        if (item === undefined) return null;
        let m = sensorInfo.find(x => x.sensorId === feature.get('id'));
        if (m === undefined) return null;
        let status = m.status;
        if (status === '停用' || status === '缺測') return 1;
        if (status === '警戒') return 2;
        return 0;
    }

    static _mapGateOpening(val, unit) {
        if (val == -888) return " 無此設備 ";
        if (val == -999) return " 異常 ";
        let v = MathUtil.bound(val, 0, 100);
        return `${v}${unit}`;
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
        if (!MapUtil.sensorLayers.includes(layerProps.layerId)) return undefined;
        return fp.value;
    }

    static styleFunction(layerProps, getShowValue, sensorInfo) {
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
        let strokew = 3;
        let strokecolor = "#BB0000";
        let urls = [];
        let zi = 50;

        switch (layerId) {
            case 'layer1':
                urls.push(require('@/assets/image/Station_WaterGate_.png'));
                break;
            case 'layer2':
                urls.push(require('@/assets/image/Station_FloodDiversion_.png'));
                break;
            case 'layer3':
                urls.push(require('@/assets/image/Station_BankSafty_.png'));
                break;
            case 'layer4':
                urls.push(require('@/assets/image/Station_CCTV_.png'));
                break;
            case 'layer5':
            case 'layer6':
            case 'layer7':
            case 'layer8':
            case SensorDef.SLOPE:
            case 'layer10':
                let item = SensorProps.find(layerId);
                urls.push(...SensorProps.getIconUrls(item));
                if (item.unit) unit = ' ' + item.unit;
                break;
            case 'layer11':
                urls.push(require('@/assets/image/reddoorclose.png'));
                urls.push(require('@/assets/image/reddooropen.png'));
                unit = " %";
                break;
            case 'layer12':
                let imgs = MapUtil._eqIntensities.map(x => require(`@/assets/image/intensity${x}.png`));
                urls.push(...imgs);
                break;
            case 'layer13':
                strokecolor = "#663300"
                break;
            case 'layer14':
                urls.push(require('@/assets/image/ADSL.png'));
                break;
            case 'layer15':
                urls.push(require('@/assets/image/4G_.png'));
                break;
            case 'layer16':
                strokecolor = "#660000"
                break;
            case 'layer17': //109
                strokecolor = "#009900"
                break;
            case 'layer18': //河川排水水道
                strokecolor = "#666600";
                break;
            case 'layer19': //堤防管理里程
                urls.push(require('@/assets/image/green-dot_.png'));
                break;
            case 'layer21':
                unit = " %"
                urls.push(require('@/assets/image/blackdoorclose.png'));
                urls.push(require('@/assets/image/blackdooropen.png'));
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
        }

        let p = new LayerProps();
        p.layerId = layerId;
        p.unit = unit;
        p.strokew = strokew;
        p.strokecolor = strokecolor;
        p.zIndex = zi;
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
