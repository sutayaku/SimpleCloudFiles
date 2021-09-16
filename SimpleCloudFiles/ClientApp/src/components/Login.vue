<template>
  <el-container style="height: 100%">
    <el-dialog
      title="登录"
      :visible.sync="dialogFormVisible"
      :close-on-click-modal="false"
      :close-on-press-escape="false"
      :show-close="false"
      :center="true"
    >
      <el-form :model="form">
        <el-form-item label="账户" :label-width="formLabelWidth">
          <el-input v-model="form.account"></el-input>
        </el-form-item>
        <el-form-item label="密码" :label-width="formLabelWidth">
          <el-input type="" v-model="form.password" show-password></el-input>
        </el-form-item>
      </el-form>
      <template #footer>
        <span class="dialog-footer">
          <el-button type="primary" @click="login">登录</el-button>
        </span>
      </template>
    </el-dialog>
  </el-container>
</template>

<script>
import axios from "axios";
import api from "../api";

export default {
  name: "Login",
  data() {
    return {
      form: {
        account: "",
        password: "",
      },
      formLabelWidth: "120px",
      dialogFormVisible: true,
    };
  },
  created(){
    this.checkLogin();
  },
  methods: {
    login() {
      var url = api.login;
      var self = this;
      axios
        .post(url, this.form)
        .then((rsp) => {
          if (rsp.data.code == 1) {
            self.$router.push("/");
          } else {
            self.$message.error("错误的账号或密码");
          }
        })
        .catch((err) => {
          console.log(err);
        });
    },
    checkLogin() {
      var self = this;
      axios
        .get(api.checkLogin)
        .then(rsp => {
          if (rsp.data.code == 1){
            self.$router.push('/');
          }
        }).catch(err => {
          console.log(err);
        });
    },
  },
};
</script>