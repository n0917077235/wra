import LineHighlight from "./lineHighlight";
import { apiGetSensorMoreDataByStationName } from '@/resource/sensor';

export default class InfoWindowUtil {
    static _supportedlayers = ['layer1', 'layer2', 'layer3'];

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
        let name = LineHighlight.getFeatureName(f);
        let data = await apiGetSensorMoreDataByStationName(name);

        // string | undefined
        let areaName = data.map(x => x.areaName).find(x => x !== undefined);

        if (name) {
            let coordinate = event.coordinate;
            let popup = layerItems.popup;

            popup.content = {
                mode: 'sensor',
                name,     // string | null | undefined
                areaName, // string | undefined
                sensors: InfoWindowUtil._getSensors(data),
                cameras: InfoWindowUtil._getCctvs(data),
            };

            popup.overlay.setPosition(coordinate);
        }
    }

    static _getSensors(data) {
        return data.map(x => {
            let name = x.sensorNameA;
            let lastDataTime = x.lastDataTime;
            if (!name || !lastDataTime) return null;

            return {
                name,
                lastDataTime,
                value1: x.value1,
            };
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