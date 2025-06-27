// store/user/getters.ts
import { State } from './state';

export const getters = {
    isAuthenticated: (state: State) => !!state.token, // 假設 token 存在即為已授權
};
