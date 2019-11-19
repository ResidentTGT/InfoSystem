using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Company.Common.Models.Pim;
using Company.Common.Requests.Pim;
using Web1cApi.Dto;

namespace Web1cApi.Communicators
{
    public interface IPimCommunicator
    {
        Task<ProductDto> GetProduct(int id);
        Task<List<Product1cDto>> GetProductCmpySkuWithAttributesIds(List<int> ids, List<string> skus);
        Task<string>  GetProductSkuCmpyGUID(string guidN, string guidX);
        Task<List<Product1cDto>> GetProductCmpySkuWithAttributesIdsPost(ProductCmpySkusAndAttributesIdsRequest request);
        Task<List<ListValue>> GetListValueCmpyIds(List<int> ids);
    };
}