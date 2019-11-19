namespace WebApi.Dto.Calculator
{
    public class CalcParamsDto
    {
        /// <summary>
        /// %
        /// </summary>
        public float MarginalityPlan { get; set; }

        /// <summary>
        /// %
        /// </summary>
        public float SeasonMarginality { get; set; }

        public float CoefA { get; set; }

        public float CoefB { get; set; }

        public float CoefC { get; set; }

        public float PrepaymentLimit { get; set; }
    }
}
