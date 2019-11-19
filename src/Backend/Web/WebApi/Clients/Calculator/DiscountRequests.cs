using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Company.Common.Enums;
using Company.Common.Factories;
using Company.Common.Models.Deals;

namespace WebApi.Clients.Calculator
{
    public class DiscountRequests
    {
        private readonly string _baseUri;
        private readonly IClientProviderFactory<HttpClient> _clientFactory;

        public DiscountRequests(IClientProviderFactory<HttpClient> clientFactory, string baseUri)
        {
            _clientFactory = clientFactory;
            _baseUri = baseUri;
        }

        public async Task<HttpResponseMessage> GetCalcParams(int dealId)
        {
            return await _clientFactory.GetClientWithHeaders().GetAsync($"{_baseUri}/Discount/marginalities?dealId={dealId}");
        }

        public async Task<HttpResponseMessage> GetMaxDiscounts(int dealId, ContractType contractType, OrderType orderType, float headDiscount, float ceoDiscount, float installment, float prepayment, bool considerMarginality)
        {
            return await _clientFactory.GetClientWithHeaders()
                .GetAsync($"{_baseUri}/Discount/?dealId={dealId}&" +
                          $"contractType={contractType}&" +
                          $"orderType={orderType}&" +
                          $"headDiscount={headDiscount}&" +
                          $"ceoDiscount={ceoDiscount}&" +
                          $"installment={installment}&" +
                          $"prepayment={prepayment}&" +
                          $"considerMarginality={considerMarginality}");
        }

        public async Task<HttpResponseMessage> SaveDeal(int id, Deal deal)
        {
            var content = new StringContent(JsonConvert.SerializeObject(deal), Encoding.UTF8, "application/json");
            return await _clientFactory.GetClientWithHeaders().PutAsync($"{_baseUri}/Discount/deals/{id}", content);
        }
    }
}