using Company.Common.Models.Seasons;

namespace WebApi.Dto.Seasons
{
    public class SalesPlanDataDto
    {
        public int Id { get; set; }

        public int DiscountPolicyId { get; set; }

        public float WholesaleMarginality { get; set; }

        public float NetworkMarginality { get; set; }

        public SalesPlanDataDto()
        {
        }

        public SalesPlanDataDto(SalesPlanData salesPlanData)
        {
            Id = salesPlanData.Id;
            DiscountPolicyId = salesPlanData.DiscountPolicyId;
            WholesaleMarginality = salesPlanData.WholesaleMarginality;
            NetworkMarginality = salesPlanData.NetworkMarginality;
        }

        public SalesPlanData ToEntity()
        {
            return new SalesPlanData()
            {
                Id = Id,
                DiscountPolicyId = DiscountPolicyId,
                WholesaleMarginality = WholesaleMarginality,
                NetworkMarginality = NetworkMarginality,
            };
        }
    }
}