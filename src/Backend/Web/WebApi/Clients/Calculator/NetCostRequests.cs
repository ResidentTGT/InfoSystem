using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Company.Common.Models.Pim;
using Company.Common.Factories;

namespace WebApi.Clients.Calculator
{
    public class NetCostRequests
    {
        private readonly string _baseUri;
        private readonly IClientProviderFactory<HttpClient> _clientFactory;

        public NetCostRequests(IClientProviderFactory<HttpClient> clientFactory, string baseUri)
        {
            _clientFactory = clientFactory;
            _baseUri = baseUri;
        }
        public async Task<HttpResponseMessage> EditNetCost(int id, bool recalculate, Product product, int brandId, int seasonId)
        {
            var content = new StringContent(JsonConvert.SerializeObject(product), Encoding.UTF8, "application/json");
            return await _clientFactory.GetClientWithHeaders().PutAsync($"{_baseUri}/NetCost/{id}?recalculate={recalculate}&brandId={brandId}&seasonId={seasonId}", content);
        }

        public async Task<HttpResponseMessage> EditMultipleNetCost(bool recalculate, List<AttributeValue> attributeValues, int brandId, int seasonId)
        {
            var content = new StringContent(JsonConvert.SerializeObject(attributeValues), Encoding.UTF8, "application/json");
            return await _clientFactory.GetClientWithHeaders().PutAsync($"{_baseUri}/NetCost/properties?recalculate={recalculate}&brandId={brandId}&seasonId={seasonId}", content);
        }
    }
}