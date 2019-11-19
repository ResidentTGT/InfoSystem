using System.Net.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using WebApi.Clients.Pim;
using Company.Common.Factories;
using WebApi.Options;

namespace WebApi.Clients
{
    public class HttpPimClient : IHttpPimClient
    {
        public ProductsRequests Products { get; }
        public AttributesCategoriesRequests AttributesCategories { get; }
        public AttributesRequests Attributes { get; }
        public AttributesTypesRequests AttributesTypes { get; }
        public CategoriesRequests Categories { get; }
        public ImportsRequests Imports { get; }
        public ExportsRequests Exports { get; }
        public PermissionsRequests Permissions { get; }

        public HttpPimClient(IClientProviderFactory<HttpClient> clientFactory, IOptions<BaseMicroservicesUrlsOptions> baseUrlsOptions)
        {
            var baseUri = baseUrlsOptions.Value.Pim;

            Products = new ProductsRequests(clientFactory, baseUri);
            AttributesCategories = new AttributesCategoriesRequests(clientFactory, baseUri);
            Attributes = new AttributesRequests(clientFactory, baseUri);
            AttributesTypes = new AttributesTypesRequests(clientFactory, baseUri);
            Categories = new CategoriesRequests(clientFactory, baseUri);
            Imports = new ImportsRequests(clientFactory, baseUri);
            Exports = new ExportsRequests(clientFactory, baseUri);
            Permissions = new PermissionsRequests(clientFactory, baseUri);
        }
    }
}