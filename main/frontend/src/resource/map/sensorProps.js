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
            unit: '',
            sensorType: 'Earthquake',
        },
        {
            layerId: SensorDef.SLOPE,
            dot: 'orange-dot',
            unit: '°',
            sensorType: 'Slope',
        },
        {
            layerId: 'layer10', // 水位
            dot: 'red-dot',
            unit: 'm',
            sensorType: 'WaterLevel',
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