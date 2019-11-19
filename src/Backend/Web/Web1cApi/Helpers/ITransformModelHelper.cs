using System.Collections.Generic;
using System.Threading.Tasks;
using Web1cApi.Dto;

namespace Web1cApi.Helpers
{
    public interface ITransformModelHelper
    {
        List<Product1cDto> PopulateProduct1cDto(List<TradeManagementResponseDto> ltmr, Dictionary<int, string> lvDictionary);
    }
}