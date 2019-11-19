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
    public class CategoriesRequests
    {
        private readonly IClientProviderFactory<HttpClient> _clientFactory;
        private readonly string _baseUri;

        public CategoriesRequests(IClientProviderFactory<HttpClient> clientFactory, string baseUri)
        {
            _clientFactory = clientFactory;
            _baseUri = baseUri;
        }

        public async Task<HttpResponseMessage> Get(int id)
        {
            return await _clientFactory.GetClientWithHeaders().GetAsync($"{_baseUri}/categories/{id}");
        }

        public async Task<HttpResponseMessage> Get()
        {
            return await _clientFactory.GetClientWithHeaders().GetAsync($"{_baseUri}/categories/");
        }

        public async Task<HttpResponseMessage> GetAttributeCmpyCategoriesIdsFromCalculator(List<int> ids)
        {
            return await _clientFactory.GetClientWithHeaders().GetAsync($"{_baseUri}/categories/attributes?ids={string.Join(", ", ids)}");
        }

        public async Task<HttpResponseMessage> GetAttributes(int id)
        {
            return await _clientFactory.GetClientWithHeaders().GetAsync($"{_baseUri}/categories/{id}/attributes");
        }

        public async Task<HttpResponseMessage> Create(Category category)
        {
            var content = new StringContent(JsonConvert.SerializeObject(category), Encoding.UTF8, "application/json");
            return await _clientFactory.GetClientWithHeaders().PostAsync($"{_baseUri}/categories/", content);
        }

        public async Task<HttpResponseMessage> Edit(int id, Category category)
        {
            var content = new StringContent(JsonConvert.SerializeObject(category), Encoding.UTF8, "application/json");
            return await _clientFactory.GetClientWithHeaders().PutAsync($"{_baseUri}/categories/{id}", content);
        }


        public async Task<HttpResponseMessage> Delete(int id)
        {
            return await _clientFactory.GetClientWithHeaders().DeleteAsync($"{_baseUri}/categories/{id}");
        }
    }
}