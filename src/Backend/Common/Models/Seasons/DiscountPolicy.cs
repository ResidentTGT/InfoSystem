using System;
using System.Collections.Generic;

namespace Company.Common.Models.Seasons
{
    public class DiscountPolicy
    {
        public int Id { get; set; }

        public int SeasonListValueId { get; set; }

        public DateTime CreateTime { get; set; }

        public int CreatorId { get; set; }

        public ICollection<SalesPlanData> SalesPlanDatas { get; set; } = new List<SalesPlanData>() { };

        public ICollection<BrandPolicyData> BrandPolicyDatas { get; set; } = new List<BrandPolicyData>() { };

        public ICollection<PolicyData> PolicyDatas { get; set; } = new List<PolicyData>() { };

        public virtual ICollection<ExchangeRates> ExchangeRates { get; set; }
    }
}
