using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Company.Common.Enums;
using Company.Common.Factories;
using Company.Common.Models.Pim;

namespace WebApi.Clients.Calculator
{
    public class BwpRrcRequests
    {
        private readonly string _baseUri;
        private readonly IClientProviderFactory<HttpClient> _clientFactory;

        public BwpRrcRequests(IClientProviderFactory<HttpClient> clientFactory, string baseUri)
        {
            _clientFactory = clientFactory;
            _baseUri = baseUri;
        }

        public async Task<HttpResponseMessage> EditMultipleBwpAndRrc(RetailAttribute pin, RetailAttribute editAttr, List<AttributeValue> attributeValues)
        {
            var content = new StringContent(JsonConvert.SerializeObject(attributeValues), Encoding.UTF8, "application/json");
            return await _clientFactory.GetClientWithHeaders().PutAsync($"{_baseUri}/bwp-rrc/properties/?pin={pin}&editAttr={editAttr}", content);
        }

        public async Task<HttpResponseMessage> EditSingleBwpAndRrc(int id, Product product)
        {
            var content = new StringContent(JsonConvert.SerializeObject(product), Encoding.UTF8, "application/json");
            return await _clientFactory.GetClientWithHeaders().PutAsync($"{_baseUri}/bwp-rrc/{id}", content);
        }
    }
}