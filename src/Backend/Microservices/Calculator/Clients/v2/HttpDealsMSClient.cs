using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Company.Common.Models.Deals;
using NLog;

namespace Company.Calculator.Clients.v2
{
    public class HttpDealsMSClient
    {
        private static readonly HttpClient client = new HttpClient();
        private readonly Logger _logger;

        public HttpDealsMSClient()
        {
            _logger = LogManager.GetCurrentClassLogger();
        }

        public List<float> GetMarginalities(int dealId, HttpContext context)
        {
            _logger.Log(LogLevel.Debug, "Starting the get marginalities method...");

            _logger.Log(LogLevel.Trace, "Adding user data to headers...");
            AddUserDataToHeaders(context);
            _logger.Log(LogLevel.Trace, "User data to headers are added");

            _logger.Log(LogLevel.Debug, "Get marginalities method is over.");
            return JsonConvert.DeserializeObject<List<float>>(client.GetAsync($"/v2/deals/marginalities?dealId={dealId}").Result.Content.ReadAsStringAsync().Result);
        }

        public Deal GetDeal(int dealId, HttpContext context)
        {
            _logger.Log(LogLevel.Debug, "Starting the get deal method...");

            _logger.Log(LogLevel.Trace, "Adding user data to headers...");
            AddUserDataToHeaders(context);
            _logger.Log(LogLevel.Trace, "User data to headers are added");

            _logger.Log(LogLevel.Debug, "Get deal method is over.");
            return JsonConvert.DeserializeObject<Deal>(client.GetAsync($"/v2/deals/{dealId}").Result.Content.ReadAsStringAsync().Result);
        }

        public async Task<HttpResponseMessage> PutDealAsync(Deal deal, HttpContext context)
        {
            _logger.Log(LogLevel.Debug, "Starting the put deal method...");

            _logger.Log(LogLevel.Trace, "Adding user data to headers...");
            AddUserDataToHeaders(context);
            _logger.Log(LogLevel.Trace, "User data to headers are added");

            _logger.Log(LogLevel.Trace, $"Serializing deal with id='{deal.Id}'...");
            var dealContent = new StringContent(JsonConvert.SerializeObject(deal), System.Text.Encoding.UTF8, "application/json");
            _logger.Log(LogLevel.Trace, "Serializing deal is finished");

            _logger.Log(LogLevel.Trace, "Getting response...");
            var response = await client.PutAsync($"/v2/deals/{deal.Id}", dealContent);
            _logger.Log(LogLevel.Trace, "Response successfully fetched");
        
            return response;
        }

        private void AddUserDataToHeaders(HttpContext context)
        {
            var userId = Common.Helpers.Headers.GetHeaderValue(context.Request.Headers, "UserId");
            Common.Helpers.Headers.SetHeader(client.DefaultRequestHeaders, "UserId", userId);
            Common.Helpers.Headers.SetHeader(client.DefaultRequestHeaders, "ClientIp", context.Connection.RemoteIpAddress.ToString());
        }
    }
}