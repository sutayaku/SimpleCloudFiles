<template>
    <uploader :options="options" 
        :file-status-text="fileStatusText"
        :autoStart="false"
        @file-added="onFileAdded"
        @complete="onComplete"
        class="uploader-example">
        <uploader-unsupport></uploader-unsupport>
        <uploader-drop>
            <p>拖文件到这里上传 或</p>
            <uploader-btn>选择文件</uploader-btn>
        </uploader-drop>
        <uploader-list></uploader-list>
    </uploader>
</template>

<script>
// import api from '../api'
import SparkMD5 from 'spark-md5'
var statusTextMap = {
    success: '上传成功',
    error: '上传出错了',
    uploading: '上传中...',
    paused: '暂停',
    waiting: '等待中...',
    cmd5: '计算md5...'
};

var my = {};
const cfu = {
    data () {
        return {
            fileStatusText: function (status) {
                return statusTextMap[status];
            },
            options: {
                target: '/api/File/Upload',
                withCredentials: true,
                chunkSize: 2097152,
                // 服务器分片校验函数，秒传及断点续传基础
                checkChunkUploadedByResponse (chunk, message) {
                    let rsp = JSON.parse(message);
                    if (rsp.code == 1) {
                        return rsp.data.exists;
                    } 
                    return false;
                },
                processParams (params) {
                    params.dirId = my.$store.state.dirId;
                    return params;
                }
            }
        }
    },
    created() {
        my = this;
    },
    methods: {
       onFileAdded(file) {
            // 计算MD5
            this.computeMD5(file);
        },
        computeMD5(file) {
            var self = this;
            var HASH_LENGTH = 1024;
            var MIN_LENGTH = 5120;
            var blobSlice = File.prototype.slice || File.prototype.mozSlice || File.prototype.webkitSlice;
            var spark = new SparkMD5.ArrayBuffer();
            var blobs = [];

            file.dirId = this.$store.state.dirId;
            file.cmd5 = true;

            if (file.size < MIN_LENGTH) {
                blobs.push(blobSlice.call(file.file, 0, file.size - 1));
            } else {
                blobs.push(blobSlice.call(file.file, 0, HASH_LENGTH));
                var offset = Math.floor(file.size / 2) - 512;
                blobs.push(blobSlice.call(file.file, offset, offset + HASH_LENGTH));
                offset = file.size - HASH_LENGTH - 1;
                blobs.push(blobSlice.call(file.file, offset, offset + HASH_LENGTH));
            }
            var current = 0;
            var fileReader = new FileReader();
            fileReader.onload = function(e) {
                spark.append(e.target.result);
                current++;
                if (current < blobs.length) {
                    loadNext();
                } else {
                    var md5 = spark.end();
                    spark.destroy(); //释放缓存
                    file.uniqueIdentifier = md5; //将文件md5赋值给文件唯一标识
                    file.cmd5 = false; //取消计算md5状态
                    file.resume(); //开始上传
                }
            };

            fileReader.onError = function() {
                self.$message.error("出现了错误！停止上传！");
                file.cancel();
            };

            function loadNext() {
                fileReader.readAsArrayBuffer(blobs[current]);
            }

            loadNext();
        },
        onComplete() {
            this.$emit('complete');
        }
    }
};
export default cfu;
</script>

<style>
  .uploader-example {
    width: 880px;
    padding: 15px;
    margin: 40px auto 0;
    font-size: 12px;
    box-shadow: 0 0 10px rgba(0, 0, 0, .4);
  }
  .uploader-example .uploader-btn {
    margin-right: 4px;
  }
  .uploader-example .uploader-list {
    max-height: 440px;
    overflow: auto;
    overflow-x: hidden;
    overflow-y: auto;
  }
</style>