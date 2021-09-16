import Vue from 'vue'
import VueRouter from 'vue-router'
import Login from  './components/Login.vue'
import Home from './components/Home.vue'

Vue.use(VueRouter)

const routes = [
    {path: '/', component: Home},
    {path: '/login', component: Login}
]

const router = new VueRouter({
    routes
});

export default router;