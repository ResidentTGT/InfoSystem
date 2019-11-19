using System.Net.Http;
using Microsoft.Extensions.Options;
using WebApi.Clients.Calculator;
using Company.Common.Factories;
using WebApi.Options;

namespace WebApi.Clients
{
    public class HttpCalculatorClient : IHttpCalculatorClient
    {
        public BwpRrcRequests BwpRrc { get; }
        public DiscountRequests Discount { get; }
        public NetCostRequests NetCost { get; }

        public HttpCalculatorClient(IClientProviderFactory<HttpClient> clientFactory, IOptions<BaseMicroservicesUrlsOptions> baseUrlsOptions)
        {
            var baseUri = baseUrlsOptions.Value.Calculator;

            BwpRrc = new BwpRrcRequests(clientFactory, baseUri);
            Discount = new DiscountRequests(clientFactory, baseUri);
            NetCost = new NetCostRequests(clientFactory, baseUri);
        }
    }
}