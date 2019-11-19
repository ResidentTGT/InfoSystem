using Microsoft.Extensions.Configuration;
using NLog;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Company.Pim.Client.v2
{
    public class HttpWebApiCommunicator : IWebApiCommunicator
    {
        private readonly HttpClient _apiClient = new HttpClient();
        private readonly string _webApiBaseUrl;
        private readonly Logger _logger;

        public HttpWebApiCommunicator(IConfiguration configuration)
        {
            _webApiBaseUrl = configuration.GetSection("Urls:WebApiBaseUrl").Value;
            _logger = LogManager.GetCurrentClassLogger();
        }

        public async Task<HttpResponseMessage> GetUser(int id)
        {
            _logger.Log(LogLevel.Trace, $"Start getting user with Id={id}...");
            var user = new HttpResponseMessage();
            try
            {
                user = await _apiClient.GetAsync($"{_webApiBaseUrl}/users/info?userId={id}");
            }
            catch (Exception e)
            {
                _logger.Log(LogLevel.Error, $"User with Id={id} is not received. Error: {e.Message}");
                throw;
            }
            _logger.Log(LogLevel.Trace, $"User with Id={id} is succesfully received.");
            return user;
        }
    }
}
