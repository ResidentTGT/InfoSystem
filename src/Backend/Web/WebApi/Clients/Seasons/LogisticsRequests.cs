using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Company.Common;
using Company.Common.Models.Seasons;
using Company.Common.Factories;

namespace WebApi.Clients.Seasons
{
    public class LogisticsRequests
    {
        private readonly IClientProviderFactory<HttpClient> _clientFactory;
        private readonly string _baseUri;

        public LogisticsRequests(IClientProviderFactory<HttpClient> clientFactory, string baseUri)
        {
            _clientFactory = clientFactory;
            _baseUri = baseUri;
        }

        public async Task<HttpResponseMessage> GetLogisticById(int id)
        {
            return await _clientFactory.GetClientWithHeaders().GetAsync($"{_baseUri}/Logistics/{id}");
        }

        public async Task<HttpResponseMessage> GetBySeasonAndBrandIds(int seasonId, int brandId)
        {
            return await _clientFactory.GetClientWithHeaders().GetAsync($"{_baseUri}/Logistics/?seasonId={seasonId}&brandId={brandId}");
        }

        public async Task<HttpResponseMessage> CreateLogistic(Logistic logistic)
        {
            var content = new StringContent(JsonConvert.SerializeObject(logistic), Encoding.UTF8, "application/json");
            return await _clientFactory.GetClientWithHeaders().PostAsync($"{_baseUri}/Logistics/", content);
        }

        public async Task<HttpResponseMessage> EditLogistic(int id, Logistic logistic)
        {
            var content = new StringContent(JsonConvert.SerializeObject(logistic), Encoding.UTF8, "application/json");
            return await _clientFactory.GetClientWithHeaders().PutAsync($"{_baseUri}/Logistics/{id}", content);
        }

        public async Task<HttpResponseMessage> DeleteLogistic(int id)
        {
            return await _clientFactory.GetClientWithHeaders().DeleteAsync($"{_baseUri}/Logistics/{id}");
        }
    }
}