using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Company.Common.Factories;
using WebApi.Clients.Crm;
using WebApi.Options;

namespace WebApi.Clients
{
    public class HttpCrmClient : IHttpCrmClient
    {
        public PartnersRequests Partners { get; }

        public HttpCrmClient(IClientProviderFactory<HttpClient> clientFactory, IOptions<BaseMicroservicesUrlsOptions> baseUrlsOptions)
        {
            Partners = new PartnersRequests(clientFactory, baseUrlsOptions.Value.Crm);
        }
    }
}