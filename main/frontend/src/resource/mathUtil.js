export default class MathUtil {
    static bound(value, min, max) {
        return Math.max(Math.min(value, max), min);
    }
}
