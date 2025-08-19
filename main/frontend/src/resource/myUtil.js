export default class MyUtil {
    // Returns null if not found
    static getUrlParam(key) {
        return new URL(window.location.href).searchParams.get(key);
    }

    static getRandomId() {
        // Must start with a letter
        return 'i_' + MyUtil.getRandomString();
    }

    static getRandomString() {
        return Math.random().toString(36).substring(2, 15) +
            Math.random().toString(36).substring(2, 15);
    }

}