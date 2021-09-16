using System;
using System.ComponentModel.DataAnnotations;

namespace SimpleCloudFiles.Models
{
	public class Dir
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
        /// 父级Id
        /// </summary>
        public string DirId { get; set; }
        /// <summary>
        /// 目录名称
        /// </summary>
        public string Name {  get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateTime { get; set; }
    }
}
