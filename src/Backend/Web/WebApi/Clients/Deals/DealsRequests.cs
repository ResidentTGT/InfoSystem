using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Company.Common.Models.Deals;
using Company.Common.Factories;

namespace WebApi.Clients.Deals
{
    public class DealsRequests
    {
        private readonly string _baseUri;
        private readonly IClientProviderFactory<HttpClient> _clientFactory;

        public DealsRequests(IClientProviderFactory<HttpClient> clientFactory, string baseUri)
        {
            _clientFactory = clientFactory;
            _baseUri = baseUri;
        }

        public async Task<HttpResponseMessage> GetAllDeals(
            int? pageSize, int? pageNumber, List<int> brandIds, List<int> seasonsIds,
            List<int> departmentsIds, List<int> managersIds, string contractor,
            float? discountFrom, float? discountTo, int? dealId, DateTime? loadDateFrom,
            DateTime? loadDateTo, DateTime? createDateFrom, DateTime? createDateTo)
        {


            var queryParams = new List<string>();

            if (pageSize.HasValue)
                queryParams.Add($"pageSize={pageSize}");

            if (pageNumber.HasValue)
                queryParams.Add($"pageNumber={pageNumber}");

            if (brandIds.Any())
                queryParams.Add($"brands={string.Join(",", brandIds)}");

            if (seasonsIds.Any())
                queryParams.Add($"seasons={string.Join(",", seasonsIds)}");

            if (departmentsIds.Any())
                queryParams.Add($"departments={string.Join(",", departmentsIds)}");

            if (managersIds.Any())
                queryParams.Add($"managers={string.Join(",", managersIds)}");

            if (!string.IsNullOrWhiteSpace(contractor))
                queryParams.Add($"contractor={contractor}");

            if (discountFrom.HasValue)
                queryParams.Add($"discountFrom={discountFrom}");

            if (discountTo.HasValue)
                queryParams.Add($"discountTo={discountTo}");

            if (dealId.HasValue)
                queryParams.Add($"dealId={dealId}");

            if (loadDateFrom.HasValue)
                queryParams.Add($"loadDateFrom={loadDateFrom:s}");

            if (loadDateTo.HasValue)
                queryParams.Add($"loadDateTo={loadDateTo:s}");

            if (createDateFrom.HasValue)
                queryParams.Add($"createDateFrom={createDateFrom:s}");

            if (createDateTo.HasValue)
                queryParams.Add($"createDateTo={createDateTo:s}");

            return await _clientFactory.GetClientWithHeaders().GetAsync($"{_baseUri}/deals?{string.Join("&", queryParams)}");
        }

        public async Task<HttpResponseMessage> GetDealById(int id)
        {
            return await _clientFactory.GetClientWithHeaders().GetAsync($"{_baseUri}/deals/{id}");
        }

        public async Task<HttpResponseMessage> GetDownloadFile(int id)
        {
            return await _clientFactory.GetClientWithHeaders().GetAsync($"{_baseUri}/deals/files/{id}");
        }

        public async Task<HttpResponseMessage> GetDealsManagerMarginalities(int dealId)
        {
            return await _clientFactory.GetClientWithHeaders().GetAsync($"{_baseUri}/deals/files/{dealId}");
        }

        public async Task<HttpResponseMessage> LoadOrderForm(MultipartFormDataContent content)
        {
            return await _clientFactory.GetClientWithHeaders().PostAsync($"{_baseUri}/deals", content);
        }

        public async Task<HttpResponseMessage> SaveDealDocuments(int dealId, MultipartFormDataContent content)
        {
            return await _clientFactory.GetClientWithHeaders().PostAsync($"{_baseUri}/deals/{dealId}/contract", content);
        }

        public async Task<HttpResponseMessage> CreateHeadDiscountRequest(HeadDiscountRequest headDiscountRequest)
        {
            var content = new StringContent(JsonConvert.SerializeObject(headDiscountRequest), Encoding.UTF8, "application/json");
            return await _clientFactory.GetClientWithHeaders().PostAsync($"{_baseUri}/deals/head-discount-request", content);
        }

        public async Task<HttpResponseMessage> EditDeal(int id, Deal deal)
        {
            var content = new StringContent(JsonConvert.SerializeObject(deal), Encoding.UTF8, "application/json");
            return await _clientFactory.GetClientWithHeaders().PutAsync($"{_baseUri}/deals/{id}", content);
        }

        public async Task<HttpResponseMessage> EditHeadDiscountRequest(int id, HeadDiscountRequest headDiscountRequest)
        {
            var content = new StringContent(JsonConvert.SerializeObject(headDiscountRequest), Encoding.UTF8, "application/json");
            return await _clientFactory.GetClientWithHeaders().PutAsync($"{_baseUri}/deals/head-discount-request/{id}", content);
        }

        public async Task<HttpResponseMessage> DeleteDeals(List<int> ids)
        {
            return await _clientFactory.GetClientWithHeaders().DeleteAsync($"{_baseUri}/deals/?ids={string.Join(", ", ids)}");
        }
    }
}