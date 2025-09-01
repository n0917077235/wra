import MapUtil from "./mapUtil";

export default class GateImages {
    static images = [
        'gateclose.png',
        'gateopen.png',
        'gateclose_y.png',
        'gateopen_y.png',
        'gateclose_r.png',
        'gateopen_r.png',
    ];

    static getImageIndex(value, feature, sensorInfo) {
        let status = MapUtil._getSensorStatus(feature, sensorInfo);
        let open = value <= 5 ? 0 : 1;
        let s = status === null ? 0 : status;
        return s * 2 + open;
    }

    static getImageUrls(){
        return GateImages.images.map(x => require(`@/assets/image/gates/${x}`));
    }
}