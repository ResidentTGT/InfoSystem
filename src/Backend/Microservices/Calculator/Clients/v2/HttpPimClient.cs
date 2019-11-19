using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Company.Common.Models.Pim;
using NLog;

namespace Company.Calculator.Clients.v2
{
    public class HttpPimClient
    {
        private static readonly HttpClient client = new HttpClient();
        private readonly Logger _logger;

        public HttpPimClient()
        {
            _logger = LogManager.GetCurrentClassLogger();
        }

        public async Task<HttpResponseMessage> PostProduct(Product product, HttpContext context)
        {
            _logger.Log(LogLevel.Debug, "Starting the post product method...");

            _logger.Log(LogLevel.Trace, "Adding user data to headers...");
            AddUserDataToHeaders(context);
            _logger.Log(LogLevel.Trace, "User data to headers are added");

            _logger.Log(LogLevel.Trace, "Serializing product...");
            var content = new StringContent(JsonConvert.SerializeObject(product), Encoding.UTF8, "application/json");
            _logger.Log(LogLevel.Trace, "Serializing product is finished");

            _logger.Log(LogLevel.Debug, "Post product method is over.");
            return await client.PutAsync($"/v2/products/{product.Id}", content);
        }

        public ListValue GetListValue(int id, HttpContext context)
        {
            _logger.Log(LogLevel.Debug, "Starting the get list value method...");

            _logger.Log(LogLevel.Trace, "Adding user data to headers...");
            AddUserDataToHeaders(context);
            _logger.Log(LogLevel.Trace, "User data to headers are added");

            _logger.Log(LogLevel.Debug, "Get list value method is over.");
            return JsonConvert.DeserializeObject<ListValue>(client.GetAsync($"/v2/attributes/types/list-values/{id}").Result.Content.ReadAsStringAsync().Result);
        }

        public async Task<List<List>> GetLists(List<int> ids, HttpContext context)
        {
            _logger.Log(LogLevel.Debug, "Starting the get lists method...");

            _logger.Log(LogLevel.Trace, "Adding user data to headers...");
            AddUserDataToHeaders(context);
            _logger.Log(LogLevel.Trace, "User data to headers are added");

            _logger.Log(LogLevel.Trace, "Joining IDs to string...");
            var idsAsString = string.Join(",", ids);
            _logger.Log(LogLevel.Trace, "IDs are joined to string");

            _logger.Log(LogLevel.Debug, "Get lists method is over.");
            return JsonConvert.DeserializeObject<List<List>>(await client.GetAsync($"/v2/attributes/types/listCmpyIds?ids={idsAsString}").Result.Content.ReadAsStringAsync());
        }
        public async Task<List<AttributeCategory>> GetAttributesCategories(List<int> categoriesIds, List<int> attributesIds, HttpContext context)
        {
            AddUserDataToHeaders(context);
            var categoriesIdsAsString = string.Join(",", categoriesIds);
            var attributesIdssAsString = string.Join(",", attributesIds);
            return JsonConvert.DeserializeObject<List<AttributeCategory>>(await client.GetAsync($"/v2/AttributesCategories/byCategoriesAndAttributesIds?categoriesIds={categoriesIdsAsString}&attributesIds={attributesIdssAsString}").Result.Content.ReadAsStringAsync());
        }

        public async Task<List<Product>> GetProductCmpyIds(List<int> productsIds, HttpContext context)
        {
            _logger.Log(LogLevel.Debug, "Starting the get products by IDs method...");

            _logger.Log(LogLevel.Trace, "Adding user data to headers...");
            AddUserDataToHeaders(context);
            _logger.Log(LogLevel.Trace, "User data to headers are added");

            _logger.Log(LogLevel.Trace, "Serializing product IDs...");
            var content = new StringContent(JsonConvert.SerializeObject(productsIds), Encoding.UTF8, "application/json");
            _logger.Log(LogLevel.Trace, "Serializing product IDs is finished");

            _logger.Log(LogLevel.Debug, "Get products by IDs method is over.");
            return JsonConvert.DeserializeObject<List<Product>>(await client.PostAsync($"/v2/products/ids", content).Result.Content.ReadAsStringAsync());
        }

        public async Task<HttpResponseMessage> EditProductProperties(List<AttributeValue> attributeValues, HttpContext context)
        {
            _logger.Log(LogLevel.Debug, "Starting the edit product properties method...");

            _logger.Log(LogLevel.Trace, "Adding user data to headers...");
            AddUserDataToHeaders(context);
            _logger.Log(LogLevel.Trace, "User data to headers are added");

            _logger.Log(LogLevel.Trace, "Serializing attribute values...");
            var content = new StringContent(JsonConvert.SerializeObject(attributeValues), Encoding.UTF8, "application/json");
            _logger.Log(LogLevel.Trace, "Serializing product IDs is finished");

            _logger.Log(LogLevel.Debug, "Edit product properties method is over.");
            return await client.PutAsync($"/v2/products/properties", content);
        }

        private void AddUserDataToHeaders(HttpContext context)
        {
            var userId = Common.Helpers.Headers.GetHeaderValue(context.Request.Headers, "UserId");
            Common.Helpers.Headers.SetHeader(client.DefaultRequestHeaders, "UserId", userId);
            Common.Helpers.Headers.SetHeader(client.DefaultRequestHeaders, "ClientIp", context.Connection.RemoteIpAddress.ToString());
        }
    }
}