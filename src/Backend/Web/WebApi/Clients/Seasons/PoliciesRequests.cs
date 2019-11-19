using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Company.Common;
using Company.Common.Models.Seasons;
using Company.Common.Factories;

namespace WebApi.Clients.Seasons
{
    public class PoliciesRequests
    {
        private readonly IClientProviderFactory<HttpClient> _clientFactory;
        private readonly string _baseUri;

        public PoliciesRequests(IClientProviderFactory<HttpClient> clientFactory, string baseUri)
        {
            _clientFactory = clientFactory;
            _baseUri = baseUri;
        }

        public async Task<HttpResponseMessage> GetPolicyBySeasonId(int seasonId)
        {
            return await _clientFactory.GetClientWithHeaders().GetAsync($"{_baseUri}/Policies/?seasonId={seasonId}");
        }

        public async Task<HttpResponseMessage> CreatePolicy(DiscountPolicy discountPolicy)
        {
            var content = new StringContent(JsonConvert.SerializeObject(discountPolicy), Encoding.UTF8, "application/json");
            return await _clientFactory.GetClientWithHeaders().PostAsync($"{_baseUri}/Policies", content);
        }

        public async Task<HttpResponseMessage> EditPolicy(int id, DiscountPolicy discountPolicy)
        {
            var content = new StringContent(JsonConvert.SerializeObject(discountPolicy), Encoding.UTF8, "application/json");
            return await _clientFactory.GetClientWithHeaders().PutAsync($"{_baseUri}/Policies/{id}", content);
        }

        public async Task<HttpResponseMessage> GetExchangeRateCmpySeasonListValueId(int seasonListValueId)
        {
            return await _clientFactory.GetClientWithHeaders().GetAsync($"{_baseUri}/Policies/exchange-rates?seasonId={seasonListValueId}");
        }
    }
}