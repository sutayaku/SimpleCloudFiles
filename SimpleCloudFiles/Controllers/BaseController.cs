using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace SimpleCloudFiles.Controllers
{
	public class BaseController : ControllerBase
	{
        private string _accountId = string.Empty;

        protected string AccountId
        {
            get
            {
                if (_accountId == string.Empty)
                {
                    _accountId = HttpContext.User.Claims.Where(a => a.Type == "accountId").Select(a => a.Value).FirstOrDefault();
                }

                return _accountId;
            }
        }
    }
}
