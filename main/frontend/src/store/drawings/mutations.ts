import { MutationTree } from 'vuex';

interface Drawing {
    type: string;
    geometry: any;
    properties?: any;
}

interface DrawingsState {
    drawings: Drawing[];
}

const mutations: MutationTree<DrawingsState> = {
    SET_DRAWINGS(state, drawings: Drawing[]) {
        state.drawings = drawings;
    },
    ADD_DRAWING(state, drawing: Drawing) {
        state.drawings.push(drawing);
    },
    CLEAR_DRAWINGS(state) {
        state.drawings = [];
    }
};

export default mutations;
