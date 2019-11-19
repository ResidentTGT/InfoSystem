using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Company.Common.Factories;
using WebApi.Options;

namespace WebApi.Clients.FileStorage
{
    public class FileStorageRequests
    {
        private readonly string _baseUri;
        private readonly IClientProviderFactory<HttpClient> _clientFactory;

        public FileStorageRequests(IClientProviderFactory<HttpClient> clientFactory, string baseUri)
        {
            _clientFactory = clientFactory;
            _baseUri = baseUri;
        }

        public async Task<HttpResponseMessage> Save(MultipartFormDataContent file)
        {
            return await _clientFactory.GetClientWithHeaders().PostAsync($"{_baseUri}/files/", file);
        }

        public async Task<HttpResponseMessage> GetById(int id)
        {
            return await _clientFactory.GetClientWithHeaders().GetAsync($"{_baseUri}/files/{id}");
        }

        public async Task<HttpResponseMessage> GetMetadata(List<int> ids)
        {
            return await _clientFactory.GetClientWithHeaders().GetAsync($"{_baseUri}/files/metadata?ids={string.Join(",", ids)}");
        }
    }
}