using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SimpleCloudFiles.Dtos;
using SimpleCloudFiles.Models;
using SimpleCloudFiles.Utils;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleCloudFiles.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	[Authorize]
	public class SpaceController : BaseController
	{
		private readonly CfDbContext _db;
		public SpaceController(CfDbContext db)
		{
			_db = db;
		}

		/// <summary>
		/// 获取目录下文件与目录列表
		/// </summary>
		/// <param name="dirId"></param>
		/// <returns></returns>
		[HttpGet("GetList")]
		public async Task<ApiResult> GetList(string dirId = "")
		{
			var dto = new CFDto();

			if (string.IsNullOrWhiteSpace(dirId))
			{
				var dir = await _db.Dirs.FirstOrDefaultAsync(
					a => string.IsNullOrEmpty(a.DirId)
					&& a.AccountId == AccountId);
				dirId = dir.Id;
				dto.DirId = dirId;
				dto.DirName = "首页";
			}
			else
			{
				var dir = await _db.Dirs.FirstOrDefaultAsync(
					a => a.Id == dirId
					&& a.AccountId == AccountId);
				if (dir == null)
				{
					return new ApiResult
					{
						Msg = "无效的目录"
					};
				}
				dto.PrevDirId = dir.DirId;
				dto.DirId = dirId;
				dto.DirName = dir.Name;
			}

			var files = await _db.CfFiles.Where(a => a.DirId == dirId)
				.Select(a=>new CFItem { 
					Id = a.Id,
					Name = a.FileName,
					Type = "file",
					CreateTime = a.CreateTime,
					Size = a.Size
				})
				.OrderBy(a=>a.Name)
				.ToListAsync();
			var dirs = await _db.Dirs.Where(a => a.DirId == dirId)
				.Select(a => new CFItem
				{
					Id = a.Id,
					Name = a.Name,
					Type = "dir",
					CreateTime = a.CreateTime
				})
				.OrderBy(a=>a.Name)
				.ToListAsync();


			dto.Datas.AddRange(dirs);
			dto.Datas.AddRange(files);
			var result = new ApiResult() { 
				Code = 1,
				Data = dto
			};
			return result;
		}

		/// <summary>
		/// 创建虚拟目录
		/// </summary>
		/// <param name="dto"></param>
		/// <returns></returns>
		[HttpPost("CreateDir")]
		public async Task<ApiResult> CreateDir(CreateDirDto dto)
		{
			if (string.IsNullOrWhiteSpace(dto.Name))
			{
				return new ApiResult { Msg = "不可设空名称" };
			}
			var dir = new Dir
			{
				Id = Guid.NewGuid().ToString("N"),
				AccountId = AccountId,
				DirId = dto.ParentId,
				CreateTime = DateTime.Now,
				Name = dto.Name
			};
			await _db.Dirs.AddAsync(dir);
			await _db.SaveChangesAsync();
			return new ApiResult() { Code = 1 };
		}

		/// <summary>
		/// 删除虚拟目录或文件
		/// </summary>
		/// <param name="id"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		[HttpDelete("Delete/{type}/{id}")]
		public async Task<ApiResult> Delete(string id, string type)
		{
			var result = new ApiResult();
			type = type.ToLower();
			if (type == "dir")
			{
				if (!await DeleteDir(id))
				{
					result.Msg = "无效的id";
					return result;
				}
			}
			else
			{
				var file = await _db.CfFiles.FirstOrDefaultAsync(a => a.Id == id);
				if(file == null)
				{
					result.Msg = "无效的id";
					return result;
				}

				await DeleteFile(file);

				_db.CfFiles.Remove(file);
				_db.SaveChanges();
			}
			result.Code = 1;
			return result;
		}

		private async Task<bool> DeleteDir(string id)
		{
			var dir = await _db.Dirs.FirstOrDefaultAsync(a => a.Id == id);
			if (dir == null)
			{
				return false;
			}
			//检测目录下是否有文件，有则删除
			var files = await _db.CfFiles.Where(a => a.DirId == id).ToListAsync();
			if (files.Any())
			{
				foreach(var file in files)
				{
					await DeleteFile(file);
				}

				_db.CfFiles.RemoveRange(files);
				await _db.SaveChangesAsync();
			}
			//检测目录下是否有目录，有则递归删除
			var dirs = await _db.Dirs.Where(a => a.DirId == id).ToListAsync();
			if (dirs.Any())
			{
				foreach(var dir2 in dirs)
				{
					await DeleteDir(dir2.Id);
				}
			}
			_db.Dirs.Remove(dir);
			await _db.SaveChangesAsync();
			return true;
		}

		private async Task DeleteFile(CfFile file)
		{
			//检测是否有其他引用，有则不删除文件
			var hasOther = await _db.CfFiles.AnyAsync(a => a.Md5 == file.Md5 && a.Id != file.Id);
			if (!hasOther)
			{
				var filePath = FileOperation.GetFileSavePath(file.Md5, file.Ext, file.Size);

				if (System.IO.File.Exists(filePath))
				{
					System.IO.File.Delete(filePath);
				}
			}
		}
	}
}
