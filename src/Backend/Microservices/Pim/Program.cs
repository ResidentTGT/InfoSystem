using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Company.Common;
using Company.Common.Models.Pim;

namespace Company.Pim
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var connectionString = GlobalConfig.Instance.GetConnectionString("PimContext");
            DbMigrator<PimContext>.Migrate(connectionString);

            BuildWebHost(args).Run();
        }

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration(options =>
                {
                    options.AddJsonFile("templates.json", optional: false, reloadOnChange: false);
                })
                .UseStartup<Startup>()
                .UseKestrel(options =>
                {
                    options.Listen(IPAddress.Loopback, GlobalConfig.Instance.GetValue<int>("AppPort"));

                    options.Limits.MaxRequestBodySize = null; //unlimited
                    options.Limits.KeepAliveTimeout = TimeSpan.FromMinutes(10);
                    
                })
                .ConfigureCompanyIsLogging()
                .Build();
    }
}
