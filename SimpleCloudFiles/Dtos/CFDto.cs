using System.Collections.Generic;

namespace SimpleCloudFiles.Dtos
{
	public class CFDto
	{
		public string DirId { get; set; } = string.Empty;
		public string DirName { get; set; }
		public string PrevDirId { get; set; } = string.Empty;
		public List<CFItem> Datas { get; set; } = new List<CFItem>();
	}
}
