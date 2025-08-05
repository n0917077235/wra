import MapUtil from "./mapUtil";

export default class GateImages {
    static images = [
        'reddoorclose.png',
        'reddooropen.png',
        'reddoorclose_y.png',
        'reddooropen_y.png',
        'reddoorclose_r.png',
        'reddooropen_r.png',
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