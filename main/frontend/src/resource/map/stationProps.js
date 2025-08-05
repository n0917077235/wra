import LayerDef from '../layerDef';

export default class StationProps {
    static all = [
        {
            layerId: LayerDef.TANSUI_STATION,
            text: '淡水河水門監測站',
            icon: require('@/assets/image/Station_WaterGate_.png'),
            geojsonUrl: "/GeoJson/GetTansuiGps",
            defaultVisible: true,
        },
        {
            layerId: LayerDef.YANSANTZI_STATION,
            text: '員山子分洪監測站',
            icon: require('@/assets/image/Station_FloodDiversion_.png'),
            geojsonUrl: "/GeoJson/GetYansantziGps",
            defaultVisible: true,
        },
        {
            layerId: LayerDef.EMBANKMENT_STATION,
            text: '堤防安全監測站',
            icon: require('@/assets/image/Station_BankSafty_.png'),
            geojsonUrl: "/GeoJson/GetBankGps",
            defaultVisible: true,
        },
        {
            layerId: LayerDef.CAMERA_STATION,
            text: '影像監視站',
            icon: require('@/assets/image/Station_CCTV_.png'),
            geojsonUrl: "/GeoJson/GetCCTVGps",
            defaultVisible: false,
        },
    ];

    static getAllLayers() {
        return StationProps.all.map(x => x.layerId);
    }

    static find(layerId) {
        return StationProps.all.find(x => x.layerId === layerId);
    }
}