export default class SensorItem {
    name;
    lastDataTime;
    valueText;
    areaID;
    sensorType;
    sensorId;

    getSensorPageUrl() {
        let a = encodeURIComponent(this.areaID);
        let s = encodeURIComponent(this.sensorType);
        let sid = encodeURIComponent(this.sensorId);
        return `/search/sensor?areaID=${a}&sensorType=${s}&sensorId=${sid}`;
    }
};