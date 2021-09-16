using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleCloudFiles
{
	public class Program
	{
        public static void Main(string[] args)
        {
			try
			{
				CreateHostBuilder(args).Build().Run();
			}
			catch (Exception e)
			{
				var sb = new StringBuilder();
				sb.AppendLine("³ÌÐòÍË³ö");
				sb.AppendLine(e.Message);
				if (e.InnerException != null)
				{
					sb.AppendLine(e.InnerException.ToString());
				}
				Log.Fatal(sb.ToString());
				throw;
			}
		}

		public static IHostBuilder CreateHostBuilder(string[] args)
		{
			var config = new ConfigurationBuilder()
				.SetBasePath(Directory.GetCurrentDirectory())
				.AddJsonFile("./appsettings.json")
				.AddCommandLine(args)
				.Build();

			Log.Logger = new LoggerConfiguration()
				.ReadFrom.Configuration(config)
				.CreateLogger();

			return Host.CreateDefaultBuilder(args)
				.ConfigureWebHostDefaults(webBuilder =>
				{
					webBuilder
					.UseConfiguration(config)
					.UseSerilog()
					.UseStartup<Startup>();
				});
		}
	}
}
