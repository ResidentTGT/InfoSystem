using System.Collections.Generic;

namespace WebApi.Dto.Calculator
{
    public class DiscountPolicyDto
    {
        public int Id { get; set; }

        public int SeasonListValueId { get; set; }

        public SalesPlanDataDto SalesPlanData { get; set; }

        public List<BrandPolicyDataDto> BrandPolicyDatas { get; set; } = new List<BrandPolicyDataDto>() { };

        public PolicyDataDto PolicyData { get; set; }
    }
}
