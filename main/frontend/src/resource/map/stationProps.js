import LayerDef from '../layerDef';

export default class StationProps {
    static all = [
        {
            layerId: LayerDef.TANSUI_STATION,
            icon: require('@/assets/image/Station_WaterGate_.png'),
            geojsonUrl: "/GeoJson/GetTansuiGps",
        },
        {
            layerId: LayerDef.YANSANTZI_STATION,
            icon: require('@/assets/image/Station_FloodDiversion_.png'),
            geojsonUrl: "/GeoJson/GetYansantziGps",
        },
        {
            layerId: LayerDef.EMBANKMENT_STATION,
            icon: require('@/assets/image/Station_BankSafty_.png'),
            geojsonUrl: "/GeoJson/GetBankGps",
        },
        {
            layerId: LayerDef.CAMERA_STATION,
            icon: require('@/assets/image/Station_CCTV_.png'),
            geojsonUrl: "/GeoJson/GetCCTVGps",
        },
    ];

    static getAllLayers() {
        return StationProps.all.map(x => x.layerId);
    }

    static find(layerId) {
        return StationProps.all.find(x => x.layerId === layerId);
    }
}