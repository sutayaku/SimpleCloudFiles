namespace SimpleCloudFiles.Dtos
{
    public class UploadInput
    {
        /// <summary>
        /// 文件名
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 目录Id
        /// </summary>
        public string DirId { get; set; }
        /// <summary>
        /// 扩展名
        /// </summary>
        public string Ext { get; set; }
        /// <summary>
        /// 文件hash
        /// </summary>
        public string Md5 { get; set; }
        /// <summary>
        /// 文件大小
        /// </summary>
        public long Size { get; set; }
        /// <summary>
        /// 当前块index
        /// </summary>
        public uint Chunk { get; set; }
        /// <summary>
        /// 一共划分成多少块
        /// </summary>
        public uint Chunks { get; set; }
    }
}
