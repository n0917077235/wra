interface Drawing {
    type: string;
    geometry: any; // 具體類型可根據需要進一步詳細定義
    properties?: any;
}

interface DrawingsState {
    drawings: Drawing[];
}

const state: DrawingsState = {
    drawings: JSON.parse(localStorage.getItem('drawings') || '[]')
};


export default state;
