using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Company.Common.Models.Pim;

namespace Company.Deals.Interfaces.v2
{
    public interface IPimMSCommunicator
    {
        Task<List<Product>> GetProductCmpySkus(string[] skus, HttpContext context);
        Task<List<Product>> GetProductCmpyIdsPostAsync(int[] ids, HttpContext context);
        Task<List> GetList(int id, HttpContext context);
        Task<ListValue> GetListValue(int id, HttpContext context);
    }
}
