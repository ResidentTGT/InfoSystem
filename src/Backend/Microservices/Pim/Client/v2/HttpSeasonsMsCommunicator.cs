using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using NLog;
using Company.Common.Models.Seasons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Company.Pim.Client.v2
{
    public class HttpSeasonsMsCommunicator : ISeasonsMsCommunicator
    {
        private readonly HttpClient _apiClient = new HttpClient();
        private readonly string _seasonCmpaseUrl;
        private readonly Logger _logger;

        public HttpSeasonsMsCommunicator(IConfiguration configuration)
        {
            //_apiClient = httpClient;
            // TODO: Move to settings
            _seasonCmpaseUrl = configuration.GetSection("Urls:SeasonCmpaseUrl").Value;
            _logger = LogManager.GetCurrentClassLogger();
        }

        public async Task<DiscountPolicy> GetDiscountPolicyAsync(int id, HttpContext context)
        {
            _logger.Log(LogLevel.Debug, $"Start getting discount policy with Id={id} ...");

            _logger.Log(LogLevel.Trace, "Adding user data to headers...");
            AddUserDataToHeaders(context);
            _logger.Log(LogLevel.Trace, "User data is added to headers.");

            _logger.Log(LogLevel.Trace, $"Getting discount policy with Id={id} ...");
            var responseString = await _apiClient.GetStringAsync($"{_seasonCmpaseUrl}/policies?seasonId={id}");
            _logger.Log(LogLevel.Trace, $"Discount policy with Id={id} successfully received.");

            _logger.Log(LogLevel.Debug, $"Getting discount policy with Id={id} is finished.");
            return string.IsNullOrEmpty(responseString) ?
                null :
                JsonConvert.DeserializeObject<DiscountPolicy>(responseString);
        }

        private void AddUserDataToHeaders(HttpContext context)
        {
            var userId = Common.Helpers.Headers.GetHeaderValue(context.Request.Headers, "UserId");
            Common.Helpers.Headers.SetHeader(_apiClient.DefaultRequestHeaders, "UserId", userId);
            Common.Helpers.Headers.SetHeader(_apiClient.DefaultRequestHeaders, "ClientIp", context.Connection.RemoteIpAddress.ToString());
        }
    }
}
