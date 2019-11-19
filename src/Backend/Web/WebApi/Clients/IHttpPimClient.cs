using System.Net.Http;
using WebApi.Clients.Pim;

namespace WebApi.Clients
{
    public interface IHttpPimClient
    {
        ProductsRequests Products { get; }
        AttributesCategoriesRequests AttributesCategories { get; }
        AttributesRequests Attributes { get; }
        AttributesTypesRequests AttributesTypes { get; }
        CategoriesRequests Categories { get; }
        ImportsRequests Imports { get; }
        ExportsRequests Exports { get; }
        PermissionsRequests Permissions { get; }
    };
}