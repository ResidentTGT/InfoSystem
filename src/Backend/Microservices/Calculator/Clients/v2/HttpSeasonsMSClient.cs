using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Company.Calculator.Interfaces;
using Company.Calculator.Interfaces.v2;
using Company.Common.Models.Seasons;
using ISeasonsMSCommunicator = Company.Calculator.Interfaces.v2.ISeasonsMSCommunicator;
using NLog;

namespace Company.Calculator.Clients.v2
{
    public class HttpSeasonsMSClient : ISeasonsMSCommunicator
    {
        private static readonly HttpClient client = new HttpClient();
        private readonly Logger _logger;

        public HttpSeasonsMSClient()
        {
            _logger = LogManager.GetCurrentClassLogger();
        }

        public DiscountPolicy GetPolicyBySeasonValue(int seasonListValueId, HttpContext context)
        {
            _logger.Log(LogLevel.Debug, "Starting the get policy by season value method...");

            _logger.Log(LogLevel.Trace, "Adding user data to headers...");
            AddUserDataToHeaders(context);
            _logger.Log(LogLevel.Trace, "User data to headers are added");

            _logger.Log(LogLevel.Debug, "Get policy by season value method is over.");
            return JsonConvert.DeserializeObject<DiscountPolicy>(client.GetAsync($"/v2/policies?seasonId={seasonListValueId}").Result.Content.ReadAsStringAsync().Result);
        }

        public Logistic GetLogisticBySeasonAndBrandValues(int seasonId, int brandId, HttpContext context)
        {
            _logger.Log(LogLevel.Debug, "Starting the get logistic by season and brand values method...");

            _logger.Log(LogLevel.Trace, "Adding user data to headers...");
            AddUserDataToHeaders(context);
            _logger.Log(LogLevel.Trace, "User data to headers are added");

            _logger.Log(LogLevel.Debug, "Get logistic by season and brand values method is over.");
            return JsonConvert.DeserializeObject<Logistic>(client.GetAsync($"/v2/logistics?seasonId={seasonId}&brandId={brandId}").Result.Content.ReadAsStringAsync().Result);
        }

        private void AddUserDataToHeaders(HttpContext context)
        {
            var userId = Common.Helpers.Headers.GetHeaderValue(context.Request.Headers, "UserId");
            Common.Helpers.Headers.SetHeader(client.DefaultRequestHeaders, "UserId", userId);
            Common.Helpers.Headers.SetHeader(client.DefaultRequestHeaders, "ClientIp", context.Connection.RemoteIpAddress.ToString());
        }
    }
}