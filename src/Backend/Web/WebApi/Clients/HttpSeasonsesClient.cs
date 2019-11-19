using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Net.Http;
using WebApi.Clients.Seasons;
using Company.Common.Factories;
using WebApi.Options;

namespace WebApi.Clients
{
    public class HttpSeasonsesClient : IHttpSeasonsClient
    {
        public LogisticsRequests Logistics { get; }
        public PoliciesRequests Policies { get; }

        public HttpSeasonsesClient(IClientProviderFactory<HttpClient> clientFactory, IOptions<BaseMicroservicesUrlsOptions> baseUrlsOptions)
        {
            var baseUri = baseUrlsOptions.Value.Seasons;

            Logistics = new LogisticsRequests(clientFactory, baseUri);
            Policies = new PoliciesRequests(clientFactory, baseUri);
        }
    }
}