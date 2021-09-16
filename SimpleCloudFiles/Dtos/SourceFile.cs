namespace SimpleCloudFiles.Dtos
{
    public class SourceFile
    {
        /// <summary>
        /// 源文件名
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 源文件扩展名
        /// </summary>
        public string Ext { get; set; }
        /// <summary>
        /// 源文件文件md5
        /// </summary>
        public string Md5 { get; set; }
        /// <summary>
        /// 源文件文件大小
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
        /// <summary>
        /// 当前块数据
        /// </summary>
        public byte[] ChunkData { get; set; }
    }
}
