import LayerDef from '../layerDef';
import GateImages from './gateImages';

export default class SensorProps {
    static all = [
        {
            layerId: 'layer5', // 沉陷計
            dot: 'pink-dot',
            getIcons: null,
            unit: 'mm',
            sensorType: 'Sink',
            geojsonUrl: '/GeoJson/GetSensorGps?sensorType=sink',
            valueCount: 1,
        },
        {
            layerId: 'layer6', // 高水位
            dot: 'yellow-dot',
            getIcons: null,
            unit: 'm',
            sensorType: 'WaterLevel',
            geojsonUrl: '/GeoJson/GetSensorGps?sensorType=waterlevel2',
            valueCount: 1,
        },
        {
            layerId: 'layer7', // 裂縫
            dot: 'purple-dot',
            getIcons: null,
            unit: 'mm',
            sensorType: 'Crack',
            geojsonUrl: '/GeoJson/GetSensorGps?sensorType=crack',
            valueCount: 1,
        },
        {
            layerId: LayerDef.EARTHQUAKE,
            dot: 'green-dot',
            getIcons: null,
            unit: 'cm/s²',
            sensorType: 'Earthquake',
            geojsonUrl: '/GeoJson/GetSensorGps?sensorType=earthquake',
            valueCount: 1,
        },
        {
            layerId: LayerDef.SLOPE,
            dot: 'orange-dot',
            getIcons: null,
            unit: '°',
            sensorType: 'Slope',
            geojsonUrl: '/GeoJson/GetSensorGps?sensorType=slope',
            valueCount: 2,
        },
        {
            layerId: LayerDef.LEVEL,
            dot: 'red-dot',
            getIcons: null,
            unit: 'm',
            sensorType: 'WaterLevel',
            geojsonUrl: '/GeoJson/GetSensorGps?sensorType=waterlevel',
            valueCount: 1,
        },
        {
            layerId: LayerDef.PLANNING_LEVEL,
            dot: 'cyan-dot',
            getIcons: null,
            unit: 'm',
            sensorType: 'PlanningLevel',
            geojsonUrl: '/GeoJson/GetSensorGps?sensorType=planninglevel',
            valueCount: 1,
        },
        {
            layerId: LayerDef.GATE,
            dot: null,
            getIcons: GateImages.getImageUrls,
            unit: '%',
            sensorType: 'Gate',
            geojsonUrl: '/GeoJson/GetSensorGps?sensorType=gate',
            valueCount: 1,
        },
        {
            layerId: 'layer21',
            dot: null,
            getIcons: () => [
                require('@/assets/image/blackdoorclose.png'),
                require('@/assets/image/blackdooropen.png')
            ],
            unit: '%',
            sensorType: 'Gate',
            geojsonUrl: '/GeoJson/GetTaipeiGateGps',
            valueCount: 1,
        },
    ];

    static getAllLayers() {
        return SensorProps.all.map(x => x.layerId);
    }

    static find(layerId) {
        return SensorProps.all.find(x => x.layerId === layerId);
    }

    static getIconUrls(item) {
        if (item.getIcons !== null) return item.getIcons();

        let dot = item.dot;

        return [
            require(`@/assets/image/${dot}_.png`),
            require(`@/assets/image/mapDots/${dot}_y.png`),
            require(`@/assets/image/mapDots/${dot}_r.png`),
        ];
    }
}