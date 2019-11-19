using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Company.Common;
using Company.Common.Models.Pim;
using Company.Deals.Interfaces.v2;
using NLog;
using System;

namespace Company.Deals.Client.v2
{
    public class HttpPimMSCommunicator : IPimMSCommunicator
    {
        private readonly HttpClient _apiClient;
        private readonly string _pimBaseUrl;
        private readonly Logger _logger;

        public HttpPimMSCommunicator(HttpClient httpClient, IConfiguration configuration)
        {
            _apiClient = httpClient;
            // TODO: Move to settings
            _pimBaseUrl = configuration.GetSection("Urls:PimBaseUrl").Value;
            _logger = LogManager.GetCurrentClassLogger();
        }

        public async Task<List<Product>> GetProductCmpySkus(string[] skus, HttpContext context)
        {
            _logger.Log(LogLevel.Debug, $"Starting the get products by skus '{String.Join(',', skus)}' method...");

            _logger.Log(LogLevel.Trace, "Adding user data to headers...");
            AddUserDataToHeaders(context);
            _logger.Log(LogLevel.Trace, "User data to headers are added");

            _logger.Log(LogLevel.Trace, $"Serializing SKUs '{String.Join(',', skus)}'...");
            var skusContent = new StringContent(JsonConvert.SerializeObject(skus), System.Text.Encoding.UTF8, "application/json");
            _logger.Log(LogLevel.Trace, $"Serializing SKUs '{String.Join(',', skus)}' is finished");

            _logger.Log(LogLevel.Trace, "Getting response...");
            var response = await _apiClient.PostAsync($"{_pimBaseUrl}/products/deals", skusContent);
            _logger.Log(LogLevel.Trace, "Response successfully fetched");

            _logger.Log(LogLevel.Debug, "Get products by skus method is over.");
            return JsonConvert.DeserializeObject<List<Product>>(await response.Content.ReadAsStringAsync());
        }

        public async Task<List<Product>> GetProductCmpyIdsPostAsync(int[] ids, HttpContext context)
        {
            _logger.Log(LogLevel.Debug, $"Starting the get products by IDs '{String.Join(',', ids)}' method...");

            _logger.Log(LogLevel.Trace, "Adding user data to headers...");
            AddUserDataToHeaders(context);
            _logger.Log(LogLevel.Trace, "User data to headers are added");

            _logger.Log(LogLevel.Trace, $"Serializing IDs '{String.Join(',', ids)}'...");
            var idsContent = new StringContent(JsonConvert.SerializeObject(ids), System.Text.Encoding.UTF8, "application/json");
            _logger.Log(LogLevel.Trace, $"Serializing IDs '{String.Join(',', ids)}' is finished");

            _logger.Log(LogLevel.Trace, "Getting response...");
            var response = await _apiClient.PostAsync($"{_pimBaseUrl}/products/ids", idsContent);
            _logger.Log(LogLevel.Trace, "Response successfully fetched");

            _logger.Log(LogLevel.Debug, "Get products by IDs method is over.");
            return JsonConvert.DeserializeObject<List<Product>>(await response.Content.ReadAsStringAsync());
        }

        public async Task<List> GetList(int id, HttpContext context)
        {
            _logger.Log(LogLevel.Debug, "Starting the get list method...");

            _logger.Log(LogLevel.Trace, "Adding user data to headers...");
            AddUserDataToHeaders(context);
            _logger.Log(LogLevel.Trace, "User data to headers are added");

            _logger.Log(LogLevel.Trace, "Getting response...");
            var responseString = await _apiClient.GetStringAsync($"{_pimBaseUrl}/attributes/types/lists/{id}");
            _logger.Log(LogLevel.Trace, "Response successfully fetched");

            _logger.Log(LogLevel.Debug, "Get list method is over.");
            return string.IsNullOrEmpty(responseString) ?
                null :
                JsonConvert.DeserializeObject<List>(responseString);
        }

        public async Task<ListValue> GetListValue(int id, HttpContext context)
        {
            _logger.Log(LogLevel.Debug, "Starting the get list value method...");

            _logger.Log(LogLevel.Trace, "Adding user data to headers...");
            AddUserDataToHeaders(context);
            _logger.Log(LogLevel.Trace, "User data to headers are added");

            _logger.Log(LogLevel.Trace, "Getting response...");
            var responseString = await _apiClient.GetStringAsync($"{_pimBaseUrl}/attributes/types/list-values/{id}");
            _logger.Log(LogLevel.Trace, "Response successfully fetched");

            _logger.Log(LogLevel.Debug, "Get list value method is over.");
            return string.IsNullOrEmpty(responseString) ?
                null :
                JsonConvert.DeserializeObject<ListValue>(responseString);
        }

        private void AddUserDataToHeaders(HttpContext context)
        {
            var userId = Common.Helpers.Headers.GetHeaderValue(context.Request.Headers, "UserId");
            Common.Helpers.Headers.SetHeader(_apiClient.DefaultRequestHeaders, "UserId", userId);
            Common.Helpers.Headers.SetHeader(_apiClient.DefaultRequestHeaders, "ClientIp", context.Connection.RemoteIpAddress.ToString());
        }
    }
}
