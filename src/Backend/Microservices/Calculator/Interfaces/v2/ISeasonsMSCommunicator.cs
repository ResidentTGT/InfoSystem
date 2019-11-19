using Microsoft.AspNetCore.Http;
using Company.Common.Models.Seasons;

namespace Company.Calculator.Interfaces.v2
{
    public interface ISeasonsMSCommunicator
    {
        DiscountPolicy GetPolicyBySeasonValue(int seasonListValueId, HttpContext context);
    }
}
