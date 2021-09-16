using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

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
		public string UserName { get; set; }
		public string Password {  get; set; }
		public DateTime CreateTime { get; set; }
	}
}
