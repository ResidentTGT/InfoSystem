using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Company.Common;
using Company.Common.Models.Pim;
using Company.Common.Factories;

namespace WebApi.Clients.Pim
{
    public class AttributesCategoriesRequests
    {
        private readonly IClientProviderFactory<HttpClient> _clientFactory;
        private readonly string _baseUri;

        public AttributesCategoriesRequests(IClientProviderFactory<HttpClient> clientFactory, string baseUri)
        {
            _clientFactory = clientFactory;
            _baseUri = baseUri;
        }

        public async Task<HttpResponseMessage> GetById(int categoryId)
        {
            return await _clientFactory.GetClientWithHeaders().GetAsync($"{_baseUri}/attributescategories?categoryId={categoryId}");
        }

        public async Task<HttpResponseMessage> Edit(int categoryId, List<AttributeCategory> attributeCategories)
        {
            var content = new StringContent(JsonConvert.SerializeObject(attributeCategories), Encoding.UTF8, "application/json");
            return await _clientFactory.GetClientWithHeaders().PutAsync($"{_baseUri}/attributescategories/{categoryId}", content);
        }
    }
}