<template>
    <el-container style="height:100%;">
        <el-header style="margin-top:10px;height:30px;">
            <el-row>
                <el-col :span="12">
                    <h2><span><i class="el-icon-folder-opened"></i>&nbsp;{{dirName}}</span></h2>
                </el-col>
                <el-col :span="12">
                    <el-menu default-active="1" class="el-menu-demo" mode="horizontal" style="float:right;">
                        <el-submenu index="1">
                            <template slot="title">
                                <i class="el-icon-s-custom"></i>
                                <span>{{accountInfo.userName}}，欢迎</span>
                            </template>
                            <el-menu-item index="1-1" @click="showEditAccount">
                                <i class="el-icon-edit"></i>
                                <span>修改账户与密码</span>
                            </el-menu-item>
                            <el-menu-item index="1-3" @click="onLogout">
                                <i class="el-icon-s-promotion"></i>
                                <span>登出</span>
                            </el-menu-item>
                        </el-submenu>
                    </el-menu>
                </el-col>
            </el-row>
        </el-header>
        <el-main>
            <el-button-group>
                <el-button type="primary" icon="el-icon-s-home" @click="toHome">首页</el-button>
                <el-button type="primary" icon="el-icon-caret-top" @click="toPerv">上一层</el-button>
                <el-button type="primary" icon="el-icon-upload" @click="showUpload">上传</el-button>
                <el-button type="primary" icon="el-icon-folder-add" @click="showAddDirDialog">新建</el-button>
            </el-button-group>
            <el-table :data="tableData" v-loading="loading" style="width:100%">
                <el-table-column label="名称">
                    <template slot-scope="s">
                        <i class="el-icon-document" v-if="s.row.type == 'file'"></i>
                        <i class="el-icon-folder" v-if="s.row.type == 'dir'"></i>
                        {{s.row.name}}
                    </template>
                </el-table-column>
                <el-table-column prop="fileSize" label="大小"></el-table-column>
                <el-table-column prop="createTime" label="日期"></el-table-column>
                <el-table-column label="操作">
                    <template slot-scope="s">
                        <el-button v-if="s.row.type=='file'" size="mini" type="success" @click="onDownload(s.row)">下载</el-button>
                         <el-button v-if="s.row.type=='dir'" size="mini" type="primary" @click="onInDir(s.row)">进入</el-button>
                        <el-button @click="onDelete(s.row)" size="mini" type="danger">
                            删除
                        </el-button>
                    </template>
                </el-table-column>
            </el-table>
            <el-dialog title="上传" :visible.sync="showUploadDialog"
                :close-on-click-modal="false"
                :close-on-press-escape="false">
                <CFUploader @complete="onUploadComplete"></CFUploader>
            </el-dialog>
            <el-dialog title="创建目录" :visible.sync="showCreateDirDialog" width="300px"
                :close-on-click-modal="false"
                :close-on-press-escape="false">
                <el-form :model="form">
                    <el-form-item label="目录名称">
                        <el-input v-model="form.name" autocomplete="off"></el-input>
                    </el-form-item>
                </el-form>
                <div slot="footer" class="dialog-footer">
                    <el-button @click="showCreateDirDialog = false">取 消</el-button>
                    <el-button type="primary" @click="onAddDir">确 定</el-button>
                </div>
            </el-dialog>

            <el-dialog
                title="账户与密码修改"
                :visible.sync="showEditAccountDialog"
                :close-on-click-modal="false"
                :close-on-press-escape="false"
                >
                <el-form :model="accountForm">
                    <el-form-item label="账户">
                    <el-input v-model="accountForm.account"></el-input>
                    </el-form-item>
                    <el-form-item label="密码">
                    <el-input type="" v-model="accountForm.password" show-password></el-input>
                    </el-form-item>
                </el-form>
                <template #footer>
                    <span class="dialog-footer">
                    <el-button type="primary" @click="onEditAccount">修改</el-button>
                    </span>
                </template>
            </el-dialog>
        </el-main>
    </el-container>
</template>
<script>
import CFUploader from './CFUploader.vue'
import axios from 'axios'
import api from '../api'

export default {
    name: 'Home',
    components: {CFUploader},
    data: function () {
        return {
            accountInfo: {
                userName: '',
                isInit: false,
            },
            showUploadDialog: false,
            showCreateDirDialog: false,
            showEditAccountDialog: false,
            loading: false,
            prevDirId: '',
            dirId: '',
            dirName: '首页',
            tableData: [],
            form: {
                name: ''
            },
            accountForm: {
                account: '',
                password: ''
            }
        }
    },
    created: function () {
        this.getAccountInfo();
        this.loadData();
    },
    methods: {
        loadData() {
            var self = this;
            self.loading = true;
            axios.get(`${api.getList}?dirId=${this.dirId}`)
            .then((rsp)=>{
                self.loading = false;
                var datas = rsp.data.data.datas;
                datas.forEach((d)=>{
                    if (d.type == 'file') {
                        d.fileSize = self.byteconvert(d.size);
                    } 
                    else {
                        d.fileSize = '-';
                    }
                });
                self.tableData = datas;
                self.prevDirId = rsp.data.data.prevDirId;
                self.dirId = rsp.data.data.dirId;
                self.dirName = rsp.data.data.dirName;
                self.$store.commit('changeDirId', self.dirId);
            });
        },
        getAccountInfo() {
            var self = this;
            axios.get(api.accountInfo)
            .then(rsp => {
                self.accountInfo.userName = rsp.data.data.userName;
                self.accountInfo.isInit = rsp.data.data.isInit;
            });
        },
        onInDir(row) {
            this.dirId = row.id;
            this.loadData();
        },
        toHome() {
            this.dirId = '';
            this.loadData();
        },
        toPerv() {
            this.dirId = this.prevDirId;
            this.loadData();
        },
        showUpload() {
            this.showUploadDialog = true;
        },
        showAddDirDialog() {
            this.showCreateDirDialog = true;
            this.form.name = '';
        },
        showEditAccount() {
            this.showEditAccountDialog = true;
            this.accountForm.account = '';
            this.accountForm.password = '';
        },
        onAddDir () { 
            if (!this.showCreateDirDialog)
            {
                return;
            }
            if (this.form.name.trim() == ''){
                this.$message.warning('不可输入空值');
                return;
            }
            this.showCreateDirDialog = false;
            var param = {
                parentId: this.dirId,
                name: this.form.name
            };
            var self = this;
            axios.post(api.createDir, param)
            .then((rsp) => {
                if (rsp.data.code == 1) {
                    self.loadData();
                    self.$message.success('创建成功');
                }
            })
        },
        onDelete(row) {
            var self = this;
            var url = api.delete + `/${row.type}/${row.id}`;
            var msg = '确定要删除该文件吗？';
            if (row.type == 'dir'){
                msg = '删除目录夹会删除目录内所有文件及目录，确定删除吗？';
            }
            this.$confirm(msg, '确认信息', {
                distinguishCancelAndClose: true,
                confirmButtonText: '确定',
                cancelButtonText: '取消'
            }).then(() => {
                 axios.delete(url)
                .then((rsp) => {
                    if (rsp.data.code == 1) {
                        self.$message.success("删除成功");
                        self.loadData();
                    } else {
                        self.$message.warning(rsp.data.msg);
                    }
                }).catch((err) => {
                    console.log(err);
                });
            });
        },
        download(row){ 
            var url = api.download + '/' + row.id
            window.open(url);
        },
        onLogout(){
            var self = this;
            this.$confirm('您确定要登出吗？', '确认信息', {
                distinguishCancelAndClose: true,
                confirmButtonText: '确定',
                cancelButtonText: '取消'
                })
                .then(() => {
                    axios.get(api.logout).then(()=>{
                        self.$router.push('/Login');
                    });
                });
        },
        onEditAccount(){ 
            var self = this;
            self.accountForm.account = self.accountForm.account.trim();
            self.accountForm.password = self.accountForm.password.trim();
            if ( self.accountForm.account == '' 
                || self.accountForm.password == ''
                || self.accountForm.account.indexOf(' ') != -1
                || self.accountForm.password.indexOf(' ') != -1) {
                self.$$message.warning('账号和密码不可为空或使用空格！');
                return;
            }

            this.$confirm('修改后会登出系统需要重新登录，是否继续操作？', '确认信息', {
                distinguishCancelAndClose: true,
                confirmButtonText: '确定',
                cancelButtonText: '取消'
                })
                .then(() => {
                    axios.post(api.accountUpdate, self.accountForm)
                    .then((rsp)=>{
                        if (rsp.data.code == 0) {
                            self.$message.warning(rsp.data.msg);
                        } else {
                            self.$message.success("修改成功");
                            self.$router.push('/Login');
                        }
                    });
                });
        },
        onUploadComplete(){
            this.loadData();
        },
        byteconvert (bytes) {
            var i = 0;
            var KBRule = 1024;
            var j = "BKMGT";
            while (bytes > KBRule) {
                bytes=bytes/KBRule;
                i++;
            } 
            return bytes.toFixed(1) + j.charAt(i);
        }
    }
}
</script>
