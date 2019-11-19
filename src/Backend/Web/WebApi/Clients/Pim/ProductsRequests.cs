using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Company.Common;
using Company.Common.Models.Pim;
using Company.Common.Factories;

namespace WebApi.Clients.Pim
{
    public class ProductsRequests
    {
        private readonly IClientProviderFactory<HttpClient> _clientFactory;
        private readonly string _baseUri;

        public ProductsRequests(IClientProviderFactory<HttpClient> clientFactory, string baseUri)
        {
            _clientFactory = clientFactory;
            _baseUri = baseUri;
        }

        public async Task<HttpResponseMessage> GetById(int id, bool withParents = false)
        {
            return await _clientFactory.GetClientWithHeaders().GetAsync($"{_baseUri}/products/{id}?withParents={withParents}");
        }

        public async Task<HttpResponseMessage> GetByParams(int pageSize, int pageNumber, bool withoutCategory, string sku, string name, int? importId)
        {
            return await _clientFactory.GetClientWithHeaders().GetAsync($"{_baseUri}/products/?pageSize={pageSize}" +
                                          $"&pageNumber={pageNumber}" +
                                          $"&withoutCategory={withoutCategory}" +
                                          $"&sku={sku}" +
                                          $"&name={name}" +
                                          $"&importId={importId}");
        }

        public async Task<HttpResponseMessage> GetByBrandAndSeason(int seasonId, int brandId)
        {
            return await _clientFactory.GetClientWithHeaders().GetAsync($"{_baseUri}/products/calculator?seasonId={seasonId}&brandId={brandId}");
        }

        public async Task<HttpResponseMessage> GetProductCmpySku(List<string> skus)
        {
            var content = new StringContent(JsonConvert.SerializeObject(skus), Encoding.UTF8, "application/json");
            return await _clientFactory.GetClientWithHeaders().PostAsync($"{_baseUri}/products/deals", content);
        }

        public async Task<HttpResponseMessage> GetProductCmpyIds(List<int> ids)
        {
            return await _clientFactory.GetClientWithHeaders().GetAsync($"{_baseUri}/products/ids?ids={string.Join(", ", ids)}");
        }

        public async Task<HttpResponseMessage> GetProductCmpyIdsPost(List<int> ids)
        {
            var content = new StringContent(JsonConvert.SerializeObject(ids), Encoding.UTF8, "application/json");
            return await _clientFactory.GetClientWithHeaders().PostAsync($"{_baseUri}/products/ids", content);
        }

        public async Task<HttpResponseMessage> UpdateSearchString()
        {
            return await _clientFactory.GetClientWithHeaders().PutAsync($"{_baseUri}/products/update-search", null);
        }

        public async Task<HttpResponseMessage> UpdateSearchStringArray()
        {
            return await _clientFactory.GetClientWithHeaders().GetAsync($"{_baseUri}/products/update-search");
        }

        public async Task<HttpResponseMessage> Search(int? pageSize, int? pageNumber, bool withoutCategory, List<int> categories, List<int> attributesIds, List<string> searchStr, string sortByPropertyName, bool sortByAsc, 
                                                      string ageMonthRange = "-", string ageYearRange = "-", string priceRange = "-", string currency = "EUR")
        {
            var query = $"{_baseUri}/products/search";
            var param = new List<string>();
            if (pageSize != null)
                param.Add($"pageSize={pageSize}");
            if (pageNumber != null)
                param.Add($"pageNumber={pageNumber}");
            if (withoutCategory)
                param.Add($"withoutCategory={withoutCategory}");
            if (categories.Count != 0)
                param.Add($"categories={String.Join(",", categories)}");
            if (attributesIds.Any())
                param.Add($"attributesIds={String.Join(",", attributesIds)}");
            if (searchStr.Count != 0)
            {
                var s = searchStr.Select(str => Uri.EscapeDataString(str)).ToList();
                param.Add($"searchString={String.Join(",", s)}");
            }
            if (sortByPropertyName != null)
                param.Add($"sortByPropertyName={sortByPropertyName}");
            if (sortByAsc)
                param.Add($"sortByAsc={sortByAsc}");

            param.Add($"ageMonthRange={ageMonthRange}");
            param.Add($"ageYearRange={ageYearRange}");
            param.Add($"priceRange={priceRange}");
            param.Add($"currency={currency}");

            return await _clientFactory.GetClientWithHeaders().GetAsync($"{query}?{String.Join('&', param)}");
        }


        public async Task<HttpResponseMessage> Create(Product product)
        {
            var content = new StringContent(JsonConvert.SerializeObject(product), Encoding.UTF8, "application/json");
            return await _clientFactory.GetClientWithHeaders().PostAsync($"{_baseUri}/products/", content);
        }

        public async Task<HttpResponseMessage> CreateAttributeValues(List<AttributeValue> attributeValues)
        {
            var content = new StringContent(JsonConvert.SerializeObject(attributeValues), Encoding.UTF8, "application/json");
            return await _clientFactory.GetClientWithHeaders().PutAsync($"{_baseUri}/products/properties", content);
        }

        public async Task<HttpResponseMessage> Edit(int id, Product product)
        {
            var content = new StringContent(JsonConvert.SerializeObject(product), Encoding.UTF8, "application/json");
            return await _clientFactory.GetClientWithHeaders().PutAsync($"{_baseUri}/products/{id}", content);
        }

        public async Task<HttpResponseMessage> DeleteById(int id)
        {
            return await _clientFactory.GetClientWithHeaders().DeleteAsync($"{_baseUri}/products/{id}");
        }

        public async Task<HttpResponseMessage> DeleteByIds(List<int> ids)
        {
            return await _clientFactory.GetClientWithHeaders().DeleteAsync($"{_baseUri}/products/?ids={string.Join(", ", ids)}");
        }
    }
}