using System.IO;
using System.Net;
using IdentityDatabase;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Company.Common;

namespace Company.Users
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var connectionString = GlobalConfig.GetConfigByName("dbsettings.json").GetConnectionString("IdentityContext");
            DbMigrator<IdentityContext>.Migrate(connectionString);

            BuildWebHost(args).Run();
        }

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .UseKestrel(options =>
                {
                    options.Listen(IPAddress.Loopback, GlobalConfig.Instance.GetValue<int>("AppPort"));
                })
                .Build();
    }
}
