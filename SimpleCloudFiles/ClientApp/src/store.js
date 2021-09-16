import Vue from 'vue'
import Vuex from 'vuex'

Vue.use(Vuex)

const store = new Vuex.Store({
    state () {
        return {
            dirId: ''
        }
    },
    mutations: {
        changeDirId (state, dirId) {
            state.dirId = dirId
        }
    }
});

export default store;