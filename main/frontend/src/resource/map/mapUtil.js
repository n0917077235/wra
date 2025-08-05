import LayerProps from './layerProps';
import LayerDef from '../LayerDef';
import SensorProps from './sensorProps';
import GateImages from './gateImages';
import InfoWindowUtil from './infoWindowUtil';

export default class MapUtil {
    static _eqIntensities = ['1', '2', '3', '4', '5.1', '5.9', '6.1', '6.9', '7'];

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

    // Returns null if not applicable
    static _findSensorImageIndex(layerId, lastValue1, feature, sensorInfo) {
        if (layerId == LayerDef.GATE) {
            return GateImages.getImageIndex(lastValue1, feature, sensorInfo);
        }

        if (layerId == 'layer21') {
            return lastValue1 <= 5 ? 0 : 1;
        }

        if (layerId == 'layer12') {
            let intensity = feature.get('intensity');
            let index = MapUtil._eqIntensities.indexOf(intensity);
            return index >= 0 ? index : 0;
        }

        let item = SensorProps.find(layerId);
        if (item === undefined) return 0;
        return MapUtil._getSensorStatus(feature, sensorInfo);
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

        let item = SensorProps.find(layerId);

        if (item !== undefined) {
            urls.push(...SensorProps.getIconUrls(item));
            if (item.unit) unit = ' ' + item.unit;
        } else {
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
