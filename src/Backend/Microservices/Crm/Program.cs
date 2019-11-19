using System.Net;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Company.Common;

namespace Company.Crm
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var connectionString = GlobalConfig.Instance.GetConnectionString("CrmContext");
            DbMigrator<CrmContext>.Migrate(connectionString);
           
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
