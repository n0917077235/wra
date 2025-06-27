import { ActionTree } from 'vuex';

interface Drawing {
    type: string;
    geometry: any;
    properties?: any;
}

interface DrawingsState {
    drawings: Drawing[];
}

const actions: ActionTree<DrawingsState, any> = {
    saveDrawing({ commit, state }, drawing: Drawing) {
        commit('ADD_DRAWING', drawing);
        localStorage.setItem('drawings', JSON.stringify(state.drawings));
    },
    loadDrawings({ commit }) {
        const drawings = JSON.parse(localStorage.getItem('drawings') || '[]');
        commit('SET_DRAWINGS', drawings);
    },
    clearDrawings({ commit }) {
        commit('CLEAR_DRAWINGS');
        localStorage.removeItem('drawings');
    },
};

export default actions;
