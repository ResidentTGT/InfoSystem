using Microsoft.AspNetCore.Http;
using Company.Common.Models.Identity;
using System.Net.Http;
using Company.Common.Helpers;

namespace Company.Common.Factories
{
    public class HttpClientProviderFactory : IClientProviderFactory<HttpClient>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public HttpClientProviderFactory(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public HttpClient GetClientWithHeaders()
        {
            var client = new HttpClient();
            client.Timeout = new System.TimeSpan(0, 10, 0);

            if (_httpContextAccessor.HttpContext.Items["User"] is User user)
            {
                Headers.SetHeader(client.DefaultRequestHeaders, "UserId", user.Id.ToString());
            }

            Headers.SetHeader(client.DefaultRequestHeaders, "ClientIp", _httpContextAccessor.HttpContext.Connection.RemoteIpAddress.ToString());

            return client;
        }
    }
}
