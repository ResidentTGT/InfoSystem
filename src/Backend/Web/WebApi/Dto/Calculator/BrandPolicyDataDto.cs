namespace WebApi.Dto.Calculator
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
    }
}
