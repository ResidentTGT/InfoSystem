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
using Company.Common.Models.Deals;

namespace Company.Deals
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var connectionString = GlobalConfig.Instance.GetConnectionString("DealsContext");
            DbMigrator<DealsContext>.Migrate(connectionString);

            BuildWebHost(args).Run();
        }

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .UseKestrel(options =>
                {
                    options.Listen(IPAddress.Loopback, GlobalConfig.Instance.GetValue<int>("AppPort"));

                    options.Limits.MaxRequestBodySize = null; //unlimited
                })
                .ConfigureCompanyIsLogging()
                .Build();
    }
}
