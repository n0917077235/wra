import LineHighlight from "./lineHighlight";
import { apiGetSensorMoreDataByStationName } from '@/resource/sensor';
import SensorItem from "./sensorItem";

export default class InfoWindowUtil {
    static _supportedlayers = ['layer1', 'layer2', 'layer3', 'layer4'];

    // layerItems: { layerProps, map, popup }
    static enable(layerItems) {
        let layerId = layerItems.layerProps.layerId;
        if (!InfoWindowUtil._supportedlayers.includes(layerId)) return;
        let map = layerItems.map;
        map.on('click', e => InfoWindowUtil._handle(e, layerItems));
    }

    static _handle(event, layerItems) {
        let map = layerItems.map;
        let first = null;
        let layerFilter = layer => ol.util.getUid(layer) === ol.util.getUid(layerItems.vectorLayer);

        map.forEachFeatureAtPixel(event.pixel, f => {
            if (first === null) first = f;
        }, { layerFilter });

        if (first === null) return;
        InfoWindowUtil._setProps(layerItems, first, event)
    }

    static async _setProps(layerItems, f, event) {
        let n = LineHighlight.getFeatureName(f);
        if (!n) return;
        let c = await InfoWindowUtil._getContent(layerItems, n);

        if (n) {
            let coordinate = event.coordinate;
            let popup = layerItems.popup;
            popup.content = c;
            popup.overlay.setPosition(coordinate);
        }
    }

    static async _getContent(layerItems, n) {
        let name = n.name;

        if (layerItems.layerProps.layerId === 'layer4') {
            //  cctv
            return {
                mode: 'camera',
                name,
                url: n.url,
            }
        }

        let data = await apiGetSensorMoreDataByStationName(name);

        // string | undefined
        let areaName = data.map(x => x.areaName).find(x => x !== undefined);

        return {
            mode: 'sensor',
            name,     // string | null | undefined
            areaName, // string | undefined
            sensors: InfoWindowUtil._getSensors(data),
            cameras: InfoWindowUtil._getCctvs(data),
        };
    }

    static _getSensors(data) {
        return data.map(x => {
            let name = x.sensorNameA;
            let lastDataTime = x.lastDataTime;
            if (!name || !lastDataTime) return null;
            let s = new SensorItem();
            s.name = name;
            s.lastDataTime = lastDataTime;
            s.value1 = x.value1;
            s.areaID = x.areaID;
            s.sensorType = x.sensorType;
            s.sensorId = x.sensorId;
            return s;
        }).filter(x => x !== null);
    }

    static _getCctvs(data) {
        return data.map(x => {
            let name = x.sensorNameA;
            let url = x.stream;
            if (!name || !url) return null;
            return { name, url };
        }).filter(x => x !== null);
    }
}