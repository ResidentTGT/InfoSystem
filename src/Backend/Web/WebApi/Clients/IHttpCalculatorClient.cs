using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Company.Common.Enums;
using Company.Common.Models.Deals;
using Company.Common.Models.Pim;
using WebApi.Clients.Calculator;

namespace WebApi.Clients
{
    public interface IHttpCalculatorClient
    {
        BwpRrcRequests BwpRrc { get; }
        DiscountRequests Discount { get; }
        NetCostRequests NetCost { get; }
    }
}