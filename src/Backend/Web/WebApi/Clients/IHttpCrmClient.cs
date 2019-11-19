using WebApi.Clients.Crm;

namespace WebApi.Clients
{
    public interface IHttpCrmClient
    {
        PartnersRequests Partners { get; }
    }
}