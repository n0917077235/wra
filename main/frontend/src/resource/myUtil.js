export default class MyUtil {
    // Returns null if not found
    static getUrlParam(key) {
        return new URL(window.location.href).searchParams.get(key);
    }
}