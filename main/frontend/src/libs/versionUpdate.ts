import axios, { AxiosResponse } from 'axios';

const isNewVersion = (): void => {
    const url = `//${window.location.host}/version.json?t=${new Date().getTime()}`;

    axios.get(url).then((res: AxiosResponse) => {
        if (res.status === 200) {
            const vueVersion = res.data.version || '1.0.0';
            const localVueVersion = localStorage.getItem('vueVersion');
            localStorage.setItem('vueVersion', vueVersion);

            if (localVueVersion && localVueVersion !== vueVersion) {
                alert('檢測發現新版本，請確認後更新。');
                window.location.reload();
                return;
            }
        }
    });
};
export default {
    isNewVersion,
};