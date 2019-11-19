using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Company.Deals.Client.v2;
using Company.Deals.Interfaces.v2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Company.Deals
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddHttpClientServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Interesting but is it good?
            //services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            //add http client services
            services.AddTransient<IPimMSCommunicator, HttpPimMSCommunicator>(httpCommunicator => new HttpPimMSCommunicator(new System.Net.Http.HttpClient(), configuration));


            return services;
        }
    }
}
