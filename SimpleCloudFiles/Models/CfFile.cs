using System;
using System.ComponentModel.DataAnnotations;

namespace SimpleCloudFiles.Models
{
	/// <summary>
	/// 文件数据
	/// </summary>
	public class CfFile
    {
        /// <summary>
        /// Id
        /// </summary>
        [Key]
        public string Id { get; set; }
        /// <summary>
        /// 账户Id
        /// </summary>
        public string AccountId { get; set; }
        /// <summary>
        /// 目录Id
        /// </summary>
        public string DirId { get; set; }
        /// <summary>
        /// 由客户端传入的文件内容MD5
        /// </summary>
        public string SourceMd5 { get; set; }
        /// <summary>
        /// 文件内容MD5
        /// </summary>
        public string Md5 { get; set; }
        /// <summary>
        /// 文件大小
        /// </summary>
        public long Size { get; set; }
        /// <summary>
        /// 文件扩展名
        /// </summary>
        public string Ext { get; set; }
        /// <summary>
        /// 文件名
        /// </summary>
        public string FileName { get; set; }
        /// <summary>
        /// 添加时间
        /// </summary>
        public DateTime CreateTime { get; set; }
    }
}
