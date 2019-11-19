using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Company.Common;
using Company.Common.Factories;

namespace WebApi.Clients.Pim
{
    public class ImportsRequests
    {
        private readonly IClientProviderFactory<HttpClient> _clientFactory;
        private readonly string _baseUri;

        public ImportsRequests(IClientProviderFactory<HttpClient> clientFactory, string baseUri)
        {
            _clientFactory = clientFactory;
            _baseUri = baseUri;
        }

        public async Task<HttpResponseMessage> Get(int id)
        {
            return await _clientFactory.GetClientWithHeaders().GetAsync($"{_baseUri}/imports/{id}/error");
        }

        public async Task<HttpResponseMessage> Get()
        {
            return await _clientFactory.GetClientWithHeaders().GetAsync($"{_baseUri}/imports/");
        }

        public async Task<HttpResponseMessage> Create(MultipartFormDataContent file, List<int> necessaryAttributes)
        {
            var param = necessaryAttributes != null && necessaryAttributes.Count > 0 ? string.Join(", ", necessaryAttributes) : "";
            return await _clientFactory.GetClientWithHeaders().PostAsync($"{_baseUri}/imports?necessaryAttributes={param}", file);
        }

        public async Task<HttpResponseMessage> CreateOldImport(MultipartFormDataContent file)
        {
            return await _clientFactory.GetClientWithHeaders().PostAsync($"{_baseUri}/imports/old", file);
        }

        public async Task<HttpResponseMessage> ImportGtin(MultipartFormDataContent file)
        {
            return await _clientFactory.GetClientWithHeaders().PostAsync($"{_baseUri}/imports/gtin", file);
        }
    }
}