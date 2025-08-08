export default class MathUtil {
    static bound(value, min, max) {
        return Math.max(Math.min(value, max), min);
    }

    // Format number with at most n decimal places
    // source: https://stackoverflow.com/a/32229831
    static toFixed(value, decimalPlaces) {
        return +parseFloat(value).toFixed(decimalPlaces);
    }

}
