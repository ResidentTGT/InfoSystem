using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace Company.Common
{
    public static class CommonConfigExtensions
    {
        public static IWebHostBuilder ConfigureCompanyIsLogging(this IWebHostBuilder hostBuilder)
        {
            return hostBuilder.ConfigureLogging((hostingContext, logging) =>
            {               
                logging.AddConfiguration(hostingContext.Configuration.GetSection("Logging"));
                logging.AddConsole();
                logging.AddDebug();
            });
        }

    }
}
