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
			#region ������������
			services.AddControllersWithViews()
				.AddJsonOptions(options =>
				{
					options.JsonSerializerOptions.Converters.Add(new NullToEmptyStringResolver());
					////�Զ��巵��ʱ���ʽ
					options.JsonSerializerOptions.Converters.Add(new DateTimeConverter());
					options.JsonSerializerOptions.Converters.Add(new DateTimeNullableConverter()); //�ɿյ�
					options.JsonSerializerOptions.Encoder = JavaScriptEncoder.Create(UnicodeRanges.All);
					/*
					 * ���к�json������ֶ��ǣ�UserName��ϵ�л����СдuserName
					 */
					options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
				});
			#endregion
			services.AddDbContext<CfDbContext>(o => o.UseSqlite(Configuration.GetConnectionString("Db")));

			#region ��֤����
			var cookie = CookieAuthenticationDefaults.AuthenticationScheme;
			services.AddAuthentication(options =>
			{
				options.DefaultScheme = cookie;
				/*
                 * ����Ҫ�û���¼ʱ�����ǽ�ʹ��cookieЭ��
                 * ����ִ��AddCookie()�Ĵ������
                 */
				options.DefaultChallengeScheme = cookie;
			})
			.AddCookie(options => {
				options.ExpireTimeSpan = TimeSpan.FromDays(Convert.ToInt32(Configuration["AccountExpires"]));
				options.Events = new CookieAuthenticationEvents
				{
					//����ת����¼��ʱ�򴥷�
					OnRedirectToLogin = context =>
					{
						context.Response.StatusCode = 401;
						return Task.CompletedTask;
					}
				};
			});
			// ���� Cookie ����
			services.Configure<CookiePolicyOptions>(o =>
			{
				// Ĭ���û�ͬ��Ǳ�Ҫ�� Cookie
				o.CheckConsentNeeded = context => true;
				// ���� SameSite ���ԣ�Cookies�����붥������һ����
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

			app.UseAuthentication(); //�������֤�м��
			app.UseAuthorization(); //�������Ȩ�м��

			app.UseEndpoints(endpoints =>
			{
				endpoints.MapControllerRoute(
					name: "default",
					pattern: "{controller=Home}/{action=Index}/{id?}");
			});
		}

	}
}
