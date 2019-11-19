using System.Collections.Generic;
using System.Linq;
using Company.Common.Models.Seasons;

namespace WebApi.Dto.Seasons
{
    public class DiscountPolicyDto
    {
        public int Id { get; set; }

        public int SeasonListValueId { get; set; }

        public SalesPlanDataDto SalesPlanData { get; set; }

        public List<BrandPolicyDataDto> BrandPolicyDatas { get; set; } = new List<BrandPolicyDataDto>() { };

        public PolicyDataDto PolicyData { get; set; }

        public ExchangeRatesDto ExchangeRates { get; set; }

        public DiscountPolicyDto()
        {

        }

        public DiscountPolicyDto(DiscountPolicy discountPolicy)
        {
            Id = discountPolicy.Id;
            SeasonListValueId = discountPolicy.SeasonListValueId;

            var salesPlanData = discountPolicy.SalesPlanDatas.LastOrDefault(spd => spd.DeleteTime == null);
            SalesPlanData = salesPlanData == null ? null : new SalesPlanDataDto(salesPlanData);

            BrandPolicyDatas = discountPolicy.BrandPolicyDatas.Where(bpd => bpd.DeleteTime == null).Select(bpd => new BrandPolicyDataDto(bpd)).ToList();

            var policyData = discountPolicy.PolicyDatas.LastOrDefault(pd => pd.DeleteTime == null);
            PolicyData = policyData == null ? null : new PolicyDataDto(policyData);

            var exchangeRates = discountPolicy.ExchangeRates.OrderBy(er => er.CreateTime).LastOrDefault(er => er.DeleteTime == null);
            ExchangeRates = exchangeRates == null ? null : new ExchangeRatesDto(exchangeRates);
        }

        public DiscountPolicy ToEntity()
        {
            return new DiscountPolicy()
            {
                Id = Id,
                SeasonListValueId = SeasonListValueId,
                SalesPlanDatas = new List<SalesPlanData>() { SalesPlanData.ToEntity() },
                BrandPolicyDatas = BrandPolicyDatas.Select(bpd => bpd.ToEntity()).ToList(),
                PolicyDatas = new List<PolicyData>() { PolicyData.ToEntity() },
                ExchangeRates = new List<ExchangeRates>() { ExchangeRates.ToEntity() }
            };
        }
    }
}
