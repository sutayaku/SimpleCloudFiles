using System;
using System.ComponentModel.DataAnnotations;

namespace SimpleCloudFiles.Models
{
	/// <summary>
	/// 账户
	/// </summary>
	public class Account
	{
		/// <summary>
		/// Id
		/// </summary>
		[Key]
		public string Id { get; set; }
		/// <summary>
		/// 用户名
		/// </summary>
		public string UserName { get; set; }
		/// <summary>
		/// 密码
		/// </summary>
		public string Password {  get; set; }
		/// <summary>
		/// 创建时间
		/// </summary>
		public DateTime CreateTime { get; set; }
	}
}
