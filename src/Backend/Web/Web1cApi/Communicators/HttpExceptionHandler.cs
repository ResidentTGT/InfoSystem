using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Web1cApi.Exceptions;

namespace Web1cApi.Communicators
{
    public class HttpExceptionHandler : DelegatingHandler
    {
        public HttpExceptionHandler(HttpMessageHandler innerHandler) : base(innerHandler)
        {
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            HttpResponseMessage response;
            try
            {
                response = await base.SendAsync(request, cancellationToken);
            }
            catch (Exception e)
            {
                throw new CommunicateException(CommunicateError.NoResponse, "No response!");
            }
            if (!response.IsSuccessStatusCode)
                throw new CommunicateException(response.StatusCode, await response.Content.ReadAsStringAsync());

            return response;
        }
    }
}