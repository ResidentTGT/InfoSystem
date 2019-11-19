using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Company.Common.Factories;

namespace WebApi.Clients.Pim
{
    public class ExportsRequests
    {
        private readonly IClientProviderFactory<HttpClient> _clientFactory;
        private readonly string _baseUri;

        public ExportsRequests(IClientProviderFactory<HttpClient> clientFactory, string baseUri)
        {
            _clientFactory = clientFactory;
            _baseUri = baseUri;
        }

        public async Task<HttpResponseMessage> ExportGtin(List<int> productsIds, bool withoutCategory, List<int> categoriesIds, string searchString)
        {
            var queryParams = new List<string>
            {
                $"withoutCategory={withoutCategory}"
            };

            if (searchString != null)
                queryParams.Add($"searchString={searchString}");

            if (categoriesIds.Any())
                queryParams.Add($"categories={string.Join(",", categoriesIds)}");


            var content = new StringContent(JsonConvert.SerializeObject(productsIds), Encoding.UTF8, "application/json");
            return await _clientFactory.GetClientWithHeaders().PostAsync($"{_baseUri}/Exports/gtin?{string.Join("&", queryParams)}", content);
        }

        public async Task<HttpResponseMessage> CreateExportFile(List<int> productsIds, int? templateCategoryId, bool withoutCategory, List<int> categoriesIds, string searchString)
        {
            var queryParams = new List<string>
            {
                $"withoutCategory={withoutCategory}"
            };

            if (templateCategoryId.HasValue)
                queryParams.Add($"templateCategoryId={templateCategoryId}");

            if (searchString!=null)
                queryParams.Add($"searchString={searchString}");

            if (categoriesIds.Any())
                queryParams.Add($"categories={string.Join(",", categoriesIds)}");


            var content = new StringContent(JsonConvert.SerializeObject(productsIds), Encoding.UTF8, "application/json");
            return await _clientFactory.GetClientWithHeaders().PostAsync($"{_baseUri}/Exports?{string.Join("&", queryParams)}", content);
        }
    }
}
