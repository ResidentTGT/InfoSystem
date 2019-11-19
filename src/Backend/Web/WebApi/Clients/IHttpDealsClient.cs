using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Company.Common.Models.Deals;
using WebApi.Clients.Deals;

namespace WebApi.Clients
{
    public interface IHttpDealsClient
    {
        DealsRequests Deals { get; }
    }
}