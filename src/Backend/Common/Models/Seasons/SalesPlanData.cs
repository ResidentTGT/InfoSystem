using System;

namespace Company.Common.Models.Seasons
{
    public class SalesPlanData
    {
        public int Id { get; set; }

        public int DiscountPolicyId { get; set; }

        public DiscountPolicy DiscountPolicy { get; set; }

        public DateTime CreateTime { get; set; }

        public DateTime? DeleteTime { get; set; }

        public int CreatorId { get; set; }

        public int? DeleterId { get; set; }

        public float WholesaleMarginality { get; set; }

        public float NetworkMarginality { get; set; }
    }
}
