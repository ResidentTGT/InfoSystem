using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Company.Common.Factories;

namespace WebApi.Clients.Crm
{
    public class PartnersRequests
    {
        private readonly string _baseUri;
        private readonly IClientProviderFactory<HttpClient> _clientFactory;

        public PartnersRequests(IClientProviderFactory<HttpClient> clientFactory, string baseUri)
        {
            _clientFactory = clientFactory;
            _baseUri = baseUri;
        }

        public async Task<HttpResponseMessage> GetTeamPartnerCmpyUserId(int userId)
        {
            return await _clientFactory.GetClientWithHeaders().GetAsync($"{_baseUri}/partners/{userId}");
        }
    }
}
