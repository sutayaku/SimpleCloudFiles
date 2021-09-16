import Vue from 'vue'
import App from './App.vue'
import router from './router'
import store from './store'
import axios from 'axios'
import uploader from 'vue-simple-uploader'
import ElementUI from 'element-ui'
import 'element-ui/lib/theme-chalk/index.css'

Vue.config.productionTip = false

axios.defaults.headers.post['Content-Type'] = 'application/json';
axios.defaults.withCredentials=true;
axios.interceptors.response.use(
  (response) => {
      return response
  }, 
  (error)=>{
    if (error.response.status == 401) {
        router.push('/Login');
    }
    return Promise.reject(error);
  }
)

Vue.use(ElementUI)
Vue.use(uploader)
Vue.use(axios)

new Vue({
  router,
  store,
  render: h => h(App),
}).$mount('#app')
