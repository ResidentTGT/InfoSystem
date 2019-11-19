using WebApi.Clients.Seasons;

namespace WebApi.Clients
{
    public interface IHttpSeasonsClient
    {
        LogisticsRequests Logistics { get; }
        PoliciesRequests Policies { get; }
    }
}