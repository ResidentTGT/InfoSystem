using Company.Common.Models.Seasons;

namespace WebApi.Dto.Seasons
{
    public class BrandPolicyDataDto
    {
        public int Id { get; set; }

        public int DiscountPolicyId { get; set; }

        public string BrandName { get; set; }

        /// <summary>
        /// у.е.
        /// </summary>
        public float Volume { get; set; }

        /// <summary>
        /// %
        /// </summary>
        public float PrepaymentVolumePercent { get; set; }

        /// <summary>
        /// %
        /// </summary>
        public float CutoffDiscountPercent { get; set; }

        public BrandPolicyDataDto()
        {
        }

        public BrandPolicyDataDto(BrandPolicyData brandPolicyData)
        {
            Id = brandPolicyData.Id;
            DiscountPolicyId = brandPolicyData.DiscountPolicyId;
            BrandName = brandPolicyData.BrandName;
            Volume = brandPolicyData.Volume;
            PrepaymentVolumePercent = brandPolicyData.PrepaymentVolumePercent;
            CutoffDiscountPercent = brandPolicyData.CutoffDiscountPercent;
        }

        public BrandPolicyData ToEntity()
        {
            return new BrandPolicyData()
            {
                Id = Id,
                DiscountPolicyId = DiscountPolicyId,
                BrandName = BrandName,
                Volume = Volume,
                PrepaymentVolumePercent = PrepaymentVolumePercent,
                CutoffDiscountPercent = CutoffDiscountPercent,
            };
        }
    }
}