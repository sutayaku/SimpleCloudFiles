using System;

namespace SimpleCloudFiles.Dtos
{
	public class CFItem
	{
		public string Id { get; set; }
		public string Name {  get; set; }
		/// <summary>
		/// file or dir
		/// </summary>
		public string Type {  get; set; }
		public long Size { get; set; }
		public DateTime CreateTime { get; set; }
	}
}
