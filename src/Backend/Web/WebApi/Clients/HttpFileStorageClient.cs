using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Company.Common;
using Company.Common.Models.Identity;
using Company.Common.Models.Seasons;
using WebApi.Clients.FileStorage;
using Company.Common.Factories;
using WebApi.Options;

namespace WebApi.Clients
{
    public class HttpFileStorageClient : IHttpFileStorageClient
    {
        private readonly string _baseUri;
        private readonly IClientProviderFactory<HttpClient> _clientFactory;

        public FileStorageRequests FileStorage { get; }

        public HttpFileStorageClient(IClientProviderFactory<HttpClient> clientFactory, IOptions<BaseMicroservicesUrlsOptions> baseUrlsOptions)
        {
            _clientFactory = clientFactory;
            _baseUri = baseUrlsOptions.Value.FileStorage;

            FileStorage = new FileStorageRequests(_clientFactory, _baseUri);
        }
    }
}