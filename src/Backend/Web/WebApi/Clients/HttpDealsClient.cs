using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Company.Common;
using Company.Common.Models.Deals;
using Company.Common.Models.Identity;
using Company.Common.Models.Seasons;
using WebApi.Clients.Deals;
using Company.Common.Factories;
using WebApi.Options;

namespace WebApi.Clients
{
    public class HttpDealsClient : IHttpDealsClient
    {
        private string _baseUri;

        public DealsRequests Deals { get; }

        public HttpDealsClient(IClientProviderFactory<HttpClient> clientFactory, IOptions<BaseMicroservicesUrlsOptions> baseUrlsOptions)
        {
            _baseUri = baseUrlsOptions.Value.Deals;

            Deals = new DealsRequests(clientFactory, _baseUri);
        }
    }
}