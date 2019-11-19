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
using Company.Calculator;
using Company.Common;

namespace Calculator
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var connectionString = GlobalConfig.Instance.GetConnectionString("CalculatorContext");
            DbMigrator<CalculatorContext>.Migrate(connectionString);

            BuildWebHost(args).Run();
        }

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .UseKestrel(options =>
                {
                    options.Listen(IPAddress.Loopback, GlobalConfig.Instance.GetValue<int>("AppPort"));
                })
                .ConfigureCompanyIsLogging()
                .Build();
    }
}
