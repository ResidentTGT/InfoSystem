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
    public class AttributesRequests
    {
        private readonly IClientProviderFactory<HttpClient> _clientFactory;
        private readonly string _baseUri;

        public AttributesRequests(IClientProviderFactory<HttpClient> clientFactory, string baseUri)
        {
            _clientFactory = clientFactory;
            _baseUri = baseUri;
        }

        public async Task<HttpResponseMessage> Get(bool withCategories, bool withPermissions)
        {
            return await _clientFactory.GetClientWithHeaders().GetAsync($"{_baseUri}/attributes/?withCategories={withCategories}&withPermissions={withPermissions}");
        }

        public async Task<HttpResponseMessage> GetGroups()
        {
            return await _clientFactory.GetClientWithHeaders().GetAsync($"{_baseUri}/attributes/groups");
        }

        public async Task<HttpResponseMessage> GetById(int id)
        {
            return await _clientFactory.GetClientWithHeaders().GetAsync($"{_baseUri}/attributes/{id}");
        }

        public async Task<HttpResponseMessage> GetGroupById(int id)
        {
            return await _clientFactory.GetClientWithHeaders().GetAsync($"{_baseUri}/attributes/groups/{id}");
        }

        public async Task<HttpResponseMessage> Create(Attribute attribute)
        {
            var content = new StringContent(JsonConvert.SerializeObject(attribute), Encoding.UTF8, "application/json");
            return await _clientFactory.GetClientWithHeaders().PostAsync($"{_baseUri}/attributes/", content);
        }

        public async Task<HttpResponseMessage> CreateGroup(AttributeGroup attributeGroup)
        {
            var content = new StringContent(JsonConvert.SerializeObject(attributeGroup), Encoding.UTF8, "application/json");
            return await _clientFactory.GetClientWithHeaders().PostAsync($"{_baseUri}/attributes/groups", content);
        }

        public async Task<HttpResponseMessage> Edit(int id, Attribute attribute)
        {
            var content = new StringContent(JsonConvert.SerializeObject(attribute), Encoding.UTF8, "application/json");
            return await _clientFactory.GetClientWithHeaders().PutAsync($"{_baseUri}/attributes/{id}", content);
        }

        public async Task<HttpResponseMessage> EditGroup(int id, AttributeGroup attributeGroup)
        {
            var content = new StringContent(JsonConvert.SerializeObject(attributeGroup), Encoding.UTF8, "application/json");
            return await _clientFactory.GetClientWithHeaders().PutAsync($"{_baseUri}/attributes/groups/{id}", content);
        }

        public async Task<HttpResponseMessage> DeleteById(int id)
        {
            return await _clientFactory.GetClientWithHeaders().DeleteAsync($"{_baseUri}/attributes/{id}");
        }

        public async Task<HttpResponseMessage> DeleteByIds(List<int> ids)
        {
            return await _clientFactory.GetClientWithHeaders().DeleteAsync($"{_baseUri}/attributes/?ids={string.Join(", ", ids)}");
        }

        public async Task<HttpResponseMessage> DeleteGroupById(int id)
        {
            return await _clientFactory.GetClientWithHeaders().DeleteAsync($"{_baseUri}/attributes/groups/{id}");
        }

        public async Task<HttpResponseMessage> DeleteGroupByIds(List<int> ids)
        {
            return await _clientFactory.GetClientWithHeaders().DeleteAsync($"{_baseUri}/attributes/groups?ids={string.Join(", ", ids)}");
        }

        public async Task<HttpResponseMessage> GetFilteredAttributes(int categoryId)
        {
            return await _clientFactory.GetClientWithHeaders().GetAsync($"{_baseUri}/attributes/filters?categoryId={categoryId}");
        }
    }
}