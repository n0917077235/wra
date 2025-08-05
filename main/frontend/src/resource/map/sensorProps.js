import LayerDef from '../LayerDef';
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
        },
        {
            layerId: 'layer6', // 高水位
            dot: 'yellow-dot',
            getIcons: null,
            unit: 'm',
            sensorType: 'WaterLevel',
            geojsonUrl: '/GeoJson/GetSensorGps?sensorType=waterlevel2',
        },
        {
            layerId: 'layer7', // 裂縫
            dot: 'purple-dot',
            getIcons: null,
            unit: 'mm',
            sensorType: 'Crack',
            geojsonUrl: '/GeoJson/GetSensorGps?sensorType=crack'
        },
        {
            layerId: LayerDef.EARTHQUAKE,
            dot: 'green-dot',
            getIcons: null,
            unit: 'cm/s²',
            sensorType: 'Earthquake',
            geojsonUrl: '/GeoJson/GetSensorGps?sensorType=earthquake',
        },
        {
            layerId: LayerDef.SLOPE,
            dot: 'orange-dot',
            getIcons: null,
            unit: '°',
            sensorType: 'Slope',
            geojsonUrl: '/GeoJson/GetSensorGps?sensorType=slope',
        },
        {
            layerId: LayerDef.LEVEL,
            dot: 'red-dot',
            getIcons: null,
            unit: 'm',
            sensorType: 'WaterLevel',
            geojsonUrl: '/GeoJson/GetSensorGps?sensorType=waterlevel',
        },
        {
            layerId: LayerDef.PLANNING_LEVEL,
            dot: 'cyan-dot',
            getIcons: null,
            unit: 'm',
            sensorType: 'PlanningLevel',
            geojsonUrl: '/GeoJson/GetSensorGps?sensorType=planninglevel',
        },
        {
            layerId: LayerDef.GATE,
            dot: null,
            getIcons: GateImages.getImageUrls,
            unit: '%',
            sensorType: 'Gate',
            geojsonUrl: '/GeoJson/GetSensorGps?sensorType=gate',
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