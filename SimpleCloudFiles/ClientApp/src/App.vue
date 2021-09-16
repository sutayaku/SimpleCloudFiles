<template>
    <router-view></router-view>
</template>

<script>
import axios from 'axios'

export default {
  name: 'App',
  components: {
    
  },
  methods: {
    login (){
      var url = '/api/login/login';
      var data = {account:'admin', password: '2021'};
      axios.post(url, data)
      .then((rsp)=>{
        console.log(rsp);
      })
      .catch((err)=>{
        console.log(err);
      })
    },
    getData(){
      var url = '/api/space/test';
      axios.get(url)
      .then((rsp)=>{
        console.log(rsp);
      }).catch((err)=>{
        console.log(err);
      })
    }
  }
}
</script>

<style>
#app {
  font-family: Avenir, Helvetica, Arial, sans-serif;
  -webkit-font-smoothing: antialiased;
  -moz-osx-font-smoothing: grayscale;
  text-align: center;
  color: #2c3e50;
  margin-top: 60px;
}
</style>
