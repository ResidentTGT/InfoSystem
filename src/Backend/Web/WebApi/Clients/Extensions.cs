using Microsoft.Extensions.DependencyInjection;

namespace WebApi.Clients
{
    public static class Extensions
    {
        public static void AddMicroServicesHttpClients(this IServiceCollection services)
        {
            services.AddScoped<IHttpFileStorageClient, HttpFileStorageClient>();
            services.AddScoped<IHttpPimClient, HttpPimClient>();
            services.AddScoped<IHttpCalculatorClient, HttpCalculatorClient>();
            services.AddScoped<IHttpDealsClient, HttpDealsClient>();
            services.AddScoped<IHttpSeasonsClient, HttpSeasonsesClient>();
            services.AddScoped<IHttpCrmClient, HttpCrmClient>();
        }
    }
}