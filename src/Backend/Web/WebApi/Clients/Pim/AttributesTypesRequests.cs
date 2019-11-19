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
    public class AttributesTypesRequests
    {
        private readonly IClientProviderFactory<HttpClient> _clientFactory;
        private readonly string _baseUri;

        public AttributesTypesRequests(IClientProviderFactory<HttpClient> clientFactory, string baseUri)
        {
            _clientFactory = clientFactory;
            _baseUri = baseUri;
        }

        public async Task<HttpResponseMessage> GetLists(bool withCategories)
        {
            return await _clientFactory.GetClientWithHeaders().GetAsync($"{_baseUri}/attributes/types/lists");
        }

        public async Task<HttpResponseMessage> GetListById(int id)
        {
            return await _clientFactory.GetClientWithHeaders().GetAsync($"{_baseUri}/attributes/types/lists/{id}");
        }

        public async Task<HttpResponseMessage> GetListValueById(int id)
        {
            return await _clientFactory.GetClientWithHeaders().GetAsync($"{_baseUri}/attributes/types/list-values/{id}");
        }

        public async Task<HttpResponseMessage> GetListCmpyIds(List<int> ids)
        {
            return await _clientFactory.GetClientWithHeaders().GetAsync($"{_baseUri}/attributes/types/listCmpyIds?ids={string.Join(", ", ids)}");
        }

        public async Task<HttpResponseMessage> CreateList(List list)
        {
            var content = new StringContent(JsonConvert.SerializeObject(list), Encoding.UTF8, "application/json");
            return await _clientFactory.GetClientWithHeaders().PostAsync($"{_baseUri}/attributes/types/lists", content);
        }

        public async Task<HttpResponseMessage> CreateListValue(ListValue listValue)
        {
            var content = new StringContent(JsonConvert.SerializeObject(listValue), Encoding.UTF8, "application/json");
            return await _clientFactory.GetClientWithHeaders().PostAsync($"{_baseUri}/attributes/types/listvalues", content);
        }

        public async Task<HttpResponseMessage> EditList(int id, List list)
        {
            var content = new StringContent(JsonConvert.SerializeObject(list), Encoding.UTF8, "application/json");
            return await _clientFactory.GetClientWithHeaders().PutAsync($"{_baseUri}/attributes/types/lists/{id}", content);
        }

        public async Task<HttpResponseMessage> EditListValue(int id, ListValue listValue)
        {
            var content = new StringContent(JsonConvert.SerializeObject(listValue), Encoding.UTF8, "application/json");
            return await _clientFactory.GetClientWithHeaders().PutAsync($"{_baseUri}/attributes/types/listvalues/{id}", content);
        }

        public async Task<HttpResponseMessage> DeleteList(int id)
        {
            return await _clientFactory.GetClientWithHeaders().DeleteAsync($"{_baseUri}/attributes/types/lists/{id}");
        }

        public async Task<HttpResponseMessage> DeleteListValue(int id)
        {
            return await _clientFactory.GetClientWithHeaders().DeleteAsync($"{_baseUri}/attributes/types/listvalues/{id}");
        }
    }
}