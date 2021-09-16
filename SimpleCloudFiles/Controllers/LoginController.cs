using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using SimpleCloudFiles.Dtos;
using SimpleCloudFiles.Utils;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text.Unicode;
using System.Threading.Tasks;

namespace SimpleCloudFiles.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class LoginController : ControllerBase
	{
		private readonly CfDbContext _db;
		public LoginController(CfDbContext db)
		{
			_db = db;
		}

		[HttpPost("Login")]
		public async Task<ApiResult> Login(LoginInput input)
		{
			var result = new ApiResult();
			var account = _db.Accounts.FirstOrDefault(a=>a.UserName == input.Account && a.Password == Md5Util.GetMD5(input.Password));
			
			if (account != null)
			{
				result.Code = 1;
				var claimIdentity = new ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme);
				claimIdentity.AddClaim(new Claim("account", account.UserName));
				claimIdentity.AddClaim(new Claim("accountId", account.Id));
				await HttpContext.SignInAsync(new ClaimsPrincipal(claimIdentity));
			}
			else
			{
				result.Code = 0;
			}
			return result;
		}

		[Authorize]
		[HttpGet("Logout")]
		public async Task<ApiResult> Logout()
		{
			await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
			var result = new ApiResult
			{
				Code = 1
			};
			return result;
		}

		[HttpGet("CheckLogin")]
		public ApiResult CheckLogin()
		{
			return new ApiResult
			{
				Code = HttpContext.User.Claims.Where(a => a.Type == "accountId").Any() ? 1 : 0
			};
		}
	}
}
