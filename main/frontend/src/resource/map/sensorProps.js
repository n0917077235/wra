import SensorDef from '../sensorDef';

export default class SensorProps {
    static all = [
        {
            layerId: 'layer5', // 沉陷計
            dot: 'pink-dot',
            unit: 'mm',
            sensorType: 'Sink',
        },
        {
            layerId: 'layer6', // 高水位
            dot: 'yellow-dot',
            unit: 'm',
            sensorType: 'WaterLevel',
        },
        {
            layerId: 'layer7', // 裂縫
            dot: 'purple-dot',
            unit: 'mm',
            sensorType: 'Crack',
        },
        {
            layerId: 'layer8',
            dot: 'green-dot',
            unit: 'cm/s²',
            sensorType: 'Earthquake',
        },
        {
            layerId: SensorDef.SLOPE,
            dot: 'orange-dot',
            unit: '°',
            sensorType: 'Slope',
        },
        {
            layerId: SensorDef.LEVEL,
            dot: 'red-dot',
            unit: 'm',
            sensorType: 'WaterLevel',
        },
        {
            layerId: SensorDef.PLANNING_LEVEL,
            dot: 'cyan-dot',
            unit: 'm',
            sensorType: 'PlanningLevel',
        },
    ];

    static find(layerId) {
        return SensorProps.all.find(x => x.layerId === layerId);
    }

    static getIconUrls(item) {
        let dot = item.dot;
        return [
            require(`@/assets/image/${dot}_.png`),
            require(`@/assets/image/mapDots/${dot}_y.png`),
            require(`@/assets/image/mapDots/${dot}_r.png`),
        ];
    }
}