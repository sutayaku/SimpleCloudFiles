using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SimpleCloudFiles.Dtos;
using SimpleCloudFiles.Utils;
using System.Threading.Tasks;

namespace SimpleCloudFiles.Controllers
{
	[Authorize]
	[Route("api/[controller]")]
	[ApiController]
	public class AccountController : BaseController
	{
		private CfDbContext _db;
		public AccountController(CfDbContext db)
		{
			_db = db;
		}

		/// <summary>
		/// 修改账户或密码
		/// </summary>
		/// <param name="input"></param>
		/// <returns></returns>
		[HttpPost("Update")]
		public async Task<ApiResult> Update(LoginInput input)
		{
			var result = new ApiResult();
			var entity = await _db.Accounts.FirstOrDefaultAsync(a => a.Id == AccountId);
			var pwd = Md5Util.GetMD5("123456");
			
			if (entity.UserName != input.Account)
			{
				var hasSame = await _db.Accounts.AnyAsync(a => a.UserName == input.Account);
				if (hasSame)
				{
					result.Msg = "已有相同账户";
					return result;
				}
			}

			entity.UserName = input.Account.Trim();
			entity.Password = Md5Util.GetMD5(input.Password.Trim());
			_db.Entry(entity).State = EntityState.Modified;
			await _db.SaveChangesAsync();
			//登出
			await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
			result.Code = 1;
			return result;
		}

		/// <summary>
		/// 获取账户信息
		/// </summary>
		/// <returns></returns>
		[HttpGet("Info")]
		public async Task<ApiResult> Info()
		{
			var account = await _db.Accounts.FirstOrDefaultAsync(a => a.Id == AccountId);
			var info = new AccountInfo
			{
				UserName = account.UserName,
				IsInit = !(account.UserName == "admin" && account.Password == Md5Util.GetMD5("123456"))
			};

			return new ApiResult { Code = 1, Data = info };

		}
	}
	public class AccountInfo
	{
		public string UserName { get; set; }
		public bool IsInit { get; set; }
	}
}
