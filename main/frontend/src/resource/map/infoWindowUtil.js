import LineHighlight from "./lineHighlight";
import { apiGetSensorMoreDataByStationName } from '@/resource/sensor';
import SensorItem from "./sensorItem";
import MathUtil from '../mathUtil';
import SensorProps from "./sensorProps";
import LayerDef from "../layerDef";
import StationProps from "./stationProps";

export default class InfoWindowUtil {
    static _supportedlayers() {
        return StationProps.getAllLayers().concat(SensorProps.getAllLayers());
    }

    // layerItems: { layerProps, map, popup }
    static enable(layerItems) {
        let layerId = layerItems.layerProps.layerId;
        if (!InfoWindowUtil._supportedlayers().includes(layerId)) return;
        let map = layerItems.map;
        map.on('click', e => InfoWindowUtil._handle(e, layerItems));
    }

    static _handle(event, layerItems) {
        let map = layerItems.map;
        let first = null;

        let layerFilter = layer =>
            ol.util.getUid(layer) === ol.util.getUid(layerItems.vectorLayer);

        map.forEachFeatureAtPixel(event.pixel, f => {
            if (first === null) first = f;
        }, { layerFilter });

        if (first === null) return;
        InfoWindowUtil._setProps(layerItems, first, event)
    }

    static async _setProps(layerItems, f, event) {
        let n = LineHighlight.getFeatureName(f);
        if (!n) return;
        let c = await InfoWindowUtil._getContent(layerItems, n, f);

        if (n) {
            let coordinate = event.coordinate;
            let popup = layerItems.popup;
            popup.content = c;
            popup.overlay.setPosition(coordinate);
        }
    }

    static async _getContent(layerItems, n, f) {
        let name = n.name;
        let layerId = layerItems.layerProps.layerId;

        if (SensorProps.getAllLayers().includes(layerId)) {
            return InfoWindowUtil._getSingleSensor(layerItems, name, f);
        }

        if (layerId === LayerDef.CAMERA_STATION) {
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
            let unit = x.unit ? (' ' + x.unit) : x.unit;
            let name = x.sensorNameA;
            let lastDataTime = x.lastDataTime;
            if (!name || !lastDataTime) return null;
            let sType = x.sensorType;
            let s = new SensorItem();
            s.name = name;
            s.lastDataTime = lastDataTime;
            s.areaID = x.areaID;
            s.sensorType = sType;
            s.sensorId = x.sensorId;
            s.valueText = InfoWindowUtil.createValueText(sType, [x.value1, x.value2], unit);
            return s;
        }).filter(x => x !== null);
    }

    static _getSingleSensor(layerItems, name, f) {
        let unit = layerItems.layerProps.unit;
        let s = new SensorItem();
        s.name = name;
        s.lastDataTime = f.get('lastDataTime');
        s.areaID = f.get('areaID');
        s.sensorType = f.get('sensorType');
        s.sensorId = f.get('id');

        let values = [f.get('lastValue1'), f.get('lastValue2')];
        s.valueText = InfoWindowUtil.createValueText(s.sensorType, values, unit);

        return {
            mode: 'sensor',
            name,     // string | null | undefined
            sensors: [s],
            cameras: [],
        };
    }

    static _getValueCount(sensorType) {
        let m = SensorProps.all.find(x => x.sensorType === sensorType);
        if (m === undefined) return 1;
        return m.valueCount;
    }

    static createValueText(sensorType, values, unit) {
        let count = InfoWindowUtil._getValueCount(sensorType);
        let v = values.slice(0, count);

        if (sensorType === 'PlanningLevel') {
            return InfoWindowUtil._mapPlanningLevel(v, unit);
        }

        if (sensorType === 'Gate') {
            return InfoWindowUtil._mapGateOpening(v, unit);
        }

        return v.map(x => MathUtil.toFixed(x, 3) + unit).join(', ');
    }

    static _mapGateOpening(val, unit) {
        if (val == -888) return " 無此設備 ";
        if (val == -999 || val == -998) return " 異常 ";
        let v = MathUtil.bound(val, 0, 100);
        return `${v}${unit}`;
    }

    static _mapPlanningLevel(val, unit) {
        if (val === -1001 || val === -1002) return " 異常 ";
        return `${val}${unit}`;
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