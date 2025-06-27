// store/modules/drawings.js

const state = {
  drawings: []
};

const mutations = {
  SET_DRAWINGS(state: { drawings: any; }, drawings: any) {
    state.drawings = drawings;
  },
  ADD_DRAWING(state: { drawings: any[]; }, drawing: any) {
    state.drawings.push(drawing);
    },
    CLEAR_DRAWINGS(state: { drawings: any[]; }) {
        state.drawings = [];
    }
};

const actions = {
  saveDrawing({ commit }: any, drawing: any) {
    commit('ADD_DRAWING', drawing);
    localStorage.setItem('drawings', JSON.stringify(state.drawings));
  },
    loadDrawings({ commit }: any): void {
        if (localStorage != null) {
            const ss = localStorage.getItem('drawings');
            if (ss != null) {
                const drawings = JSON.parse(ss) || [];
                commit('SET_DRAWINGS', drawings);
            }
        }
    },
    clearDrawings({ commit }: any): void{
        commit('CLEAR_DRAWINGS');
        localStorage.removeItem('drawings');
    },
};

export default {
    namespaced: true, // 確保使用命名空間
  state,
  mutations,
  actions
};