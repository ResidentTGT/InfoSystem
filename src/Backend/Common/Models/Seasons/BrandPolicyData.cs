using System;

namespace Company.Common.Models.Seasons
{
    public class BrandPolicyData
    {
        public int Id { get; set; }

        public int DiscountPolicyId { get; set; }

        public DiscountPolicy DiscountPolicy { get; set; }

        public DateTime CreateTime { get; set; }

        public DateTime? DeleteTime { get; set; }

        public int CreatorId { get; set; }

        public int? DeleterId { get; set; }

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
    }
}
