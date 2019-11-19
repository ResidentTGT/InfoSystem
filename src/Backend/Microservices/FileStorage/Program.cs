using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Company.Common;
using System.Net;
using Company.Common.Models.FileStorage;

namespace Company.FileStorage
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var connectionString = GlobalConfig.Instance.GetConnectionString("FileStorageContext");
            DbMigrator<FileStorageContext>.Migrate(connectionString);

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
