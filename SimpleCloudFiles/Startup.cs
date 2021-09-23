using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.IO;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using System.Threading.Tasks;
using static SimpleCloudFiles.Exts.DateTimeConvertExt;

namespace SimpleCloudFiles
{
	public class Startup
	{
		public Startup(IConfiguration configuration)
		{
			Configuration = configuration;
		}

		public IConfiguration Configuration { get; }

		// This method gets called by the runtime. Use this method to add services to the container.
		public void ConfigureServices(IServiceCollection services)
		{
			#region 返回数据配置
			services.AddControllersWithViews()
				.AddJsonOptions(options =>
				{
					options.JsonSerializerOptions.Converters.Add(new NullToEmptyStringResolver());
					////自定义返回时间格式
					options.JsonSerializerOptions.Converters.Add(new DateTimeConverter());
					options.JsonSerializerOptions.Converters.Add(new DateTimeNullableConverter()); //可空的
					options.JsonSerializerOptions.Encoder = JavaScriptEncoder.Create(UnicodeRanges.All);
					/*
					 * 序列号json后，如果字段是（UserName）系列化后会小写userName
					 */
					options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
				});
			#endregion
			services.AddDbContext<CfDbContext>(o => o.UseSqlite(Configuration.GetConnectionString("Db")));

			#region 认证配置
			var cookie = CookieAuthenticationDefaults.AuthenticationScheme;
			services.AddAuthentication(options =>
			{
				options.DefaultScheme = cookie;
				/*
                 * 当需要用户登录时，我们将使用cookie协议
                 * 将会执行AddCookie()的处理程序
                 */
				options.DefaultChallengeScheme = cookie;
			})
			.AddCookie(options => {
				options.ExpireTimeSpan = TimeSpan.FromDays(Convert.ToInt32(Configuration["AccountExpires"]));
				options.Events = new CookieAuthenticationEvents
				{
					//当跳转到登录的时候触发
					OnRedirectToLogin = context =>
					{
						context.Response.StatusCode = 401;
						return Task.CompletedTask;
					}
				};
			});
			// 配置 Cookie 策略
			services.Configure<CookiePolicyOptions>(o =>
			{
				// 默认用户同意非必要的 Cookie
				o.CheckConsentNeeded = context => true;
				// 定义 SameSite 策略，Cookies允许与顶级导航一起发送
				o.MinimumSameSitePolicy = SameSiteMode.Lax;
			});
			#endregion

			services.AddSwaggerGen();

			CfCfg.SourceFileRoot = Path.Combine(Directory.GetCurrentDirectory(), "Files", "Source");
			CfCfg.SaveFileRoot = Path.Combine(Directory.GetCurrentDirectory(), "Files", "Root");
			
			services.AddCors(opt =>
			{
				opt.AddPolicy("AllowSameDomain", builder =>
				{
					builder
					.SetIsOriginAllowed(_ => true)
					.AllowAnyMethod()
					.AllowAnyHeader()
					.AllowAnyOrigin();
				});
			});
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{
			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
				app.UseSwagger();

				// Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.),
				// specifying the Swagger JSON endpoint.
				app.UseSwaggerUI(c =>
				{
					c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
				});

				
			}
			else
			{
				app.UseExceptionHandler("/Home/Error");
			}

			var fileServerOptions = new FileServerOptions();
			fileServerOptions.DefaultFilesOptions.DefaultFileNames.Clear();
			fileServerOptions.DefaultFilesOptions.DefaultFileNames.Add("index.html");
			app.UseFileServer(fileServerOptions);

			app.UseStaticFiles();

			app.UseCors("AllowSameDomain");

			app.UseRouting();

			app.UseAuthentication(); //先添加认证中间件
			app.UseAuthorization(); //在添加授权中间件

			app.UseEndpoints(endpoints =>
			{
				endpoints.MapControllerRoute(
					name: "default",
					pattern: "{controller=Home}/{action=Index}/{id?}");
			});
		}

	}
}
