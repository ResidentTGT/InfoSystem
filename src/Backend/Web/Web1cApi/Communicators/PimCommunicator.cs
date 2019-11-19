using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Company.Common.Models.Pim;
using Company.Common.Requests.Pim;
using Web1cApi.Dto;
using Web1cApi.Exceptions;
using Web1cApi.Extensions;
using Web1cApi.Helpers;
using Web1cApi.Options;

namespace Web1cApi.Communicators
{
    public class PimCommunicator : IPimCommunicator
    {
        private readonly ITransformModelHelper _transformModelHelper;
        private readonly HttpClient _client;
        private readonly string _baseUri;

        public PimCommunicator(IOptions<BaseMicroservicesUrlsOptions> baseUrlsOptions, ITransformModelHelper transformModelHelper)
        {
            _client = new HttpClient(new HttpExceptionHandler(new HttpClientHandler()));
            _baseUri = baseUrlsOptions.Value.Pim;
            _transformModelHelper = transformModelHelper;
        }

        public async Task<ProductDto> GetProduct(int id)
        {
            var response = await _client.GetAsync($"{_baseUri}/TradeManagement/{id}");
            return await response.HttpResponseToDto<Product, ProductDto>(p => new ProductDto(p));
        }


        public async Task<List<Product1cDto>> GetProductCmpySkuWithAttributesIds(List<int> ids, List<string> skus)
        {
            var query = skus.Any() ? $"skus={string.Join(",", skus)}&" : "";
            query += ids.Any() ? $"attributesIds={string.Join(",", ids)}" : "";

            var response = await _client.GetAsync($"{_baseUri}/TradeManagement?{query}");
            return await response.HttpResponseToDto<List<TradeManagementResponseDto>, List<Product1cDto>>(ltmr => _transformModelHelper.PopulateProduct1cDto(ltmr, ListValuesToDictionary(ltmr)));
        }

        public async Task<List<Product1cDto>> GetProductCmpySkuWithAttributesIdsPost(ProductCmpySkusAndAttributesIdsRequest request)
        {
            var content = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");
            var response = await _client.PostAsync($"{_baseUri}/TradeManagement", content);
            return await response.HttpResponseToDto<List<TradeManagementResponseDto>, List<Product1cDto>>(ltmr => _transformModelHelper.PopulateProduct1cDto(ltmr, ListValuesToDictionary(ltmr)));
        }

        public async Task<List<ListValue>> GetListValueCmpyIds(List<int> ids)
        {
            var response = await _client.GetAsync($"{_baseUri}/attributes/types/listValueCmpyIds?ids={string.Join(", ", ids)}");
            return await response.HttpResponseToDto<List<ListValue>, List<ListValue>>((llv) => llv);
        }

        public async Task<string> GetProductSkuCmpyGUID(string guidN, string guidX)
        {
            var response = await _client.GetAsync($"{_baseUri}/TradeManagement/sku?guidn={guidN}&guidX={guidX}");
            return await response.HttpResponseToDto<string, string>((llv) => llv);
        }

        private Dictionary<int, string> ListValuesToDictionary(List<TradeManagementResponseDto> ltmr)
        {
            if (!ltmr.Any())
                return new Dictionary<int, string>();

            var listValesIds = new List<int>();
            ltmr.ForEach(tmr =>
                listValesIds.AddRange(
                    tmr.Product.AttributeValues
                        .Where(av => av.Attribute.Type == AttributeType.List && av.ListValueId != null)
                        .Select(av => av.ListValueId.Value))
            );

            List<ListValue> listValues = null;

            if (listValesIds.Any())
                listValues = GetListValueCmpyIds(listValesIds).Result;

            return listValues?.ToDictionary(lv => lv.Id, lv => lv.Value) ?? new Dictionary<int, string>();
        }
    }
}