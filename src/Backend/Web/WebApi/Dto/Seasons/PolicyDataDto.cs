using Company.Common.Models.Seasons;

namespace WebApi.Dto.Seasons
{
    public class PolicyDataDto
    {
        public int Id { get; set; }

        public int DiscountPolicyId { get; set; }

        /// ===========================
        /// ==       Constants       ==
        /// ===========================
        /// <summary>
        /// %
        /// </summary>
        public float InternetKeyPartnerImportanceDiscount { get; set; }

        public float NetworkPartnerImportanceDiscount { get; set; }

        public float KeyPartnerImportanceDiscount { get; set; }

        public float InternetPartnerImportanceDiscount { get; set; }

        public float WholesalePartnerImportanceDiscount { get; set; }

        /// <summary>
        /// %
        /// </summary>
        public float NewPartnerDiscount { get; set; }

        public float RepeatedPartnerDiscount { get; set; }

        /// <summary>
        /// %
        /// </summary>
        public float PurchaseAndSaleDiscount { get; set; }

        /// <summary>
        /// %
        /// </summary>
        public float Commission { get; set; }

        /// <summary>
        /// %
        /// </summary>
        public float PreOrderDiscount { get; set; }

        public float FreeWarehouseCurrentOrderDiscount { get; set; }

        public float FreeWarehousePastOrderDiscount { get; set; }

        /// <summary>
        /// %
        /// </summary>
        public float MarkupForMismatchOfVolume { get; set; }

        /// <summary>
        /// months
        /// </summary>
        public int PlannedInstallment { get; set; }

        /// <summary>
        /// %
        /// </summary>
        public float PlannedPrepayment { get; set; }

        /// <summary>
        /// %
        /// </summary>
        public float AnnualRate { get; set; }

        /// <summary>
        /// periods
        /// </summary>
        public int MaxCountOfInstallmentPeriods { get; set; }

        /// ============================
        /// ==          Lists         ==
        /// ============================
        /// <summary>
        /// %
        /// </summary>
        public string VolumeDiscount { get; set; }

        // Brand-mix discount, string dictionary numberOfBrands:%
        public string BrandMixDiscount { get; set; }

        // Prepayment discount, string dictionary %:%
        public string PrepaymentDiscount { get; set; }

        public PolicyDataDto()
        {
        }

        public PolicyDataDto(PolicyData policyData)
        {
            Id = policyData.Id;
            DiscountPolicyId = policyData.DiscountPolicyId;

            // Importance Discounts, %
            InternetKeyPartnerImportanceDiscount = policyData.InternetKeyPartnerImportanceDiscount;
            NetworkPartnerImportanceDiscount = policyData.NetworkPartnerImportanceDiscount;
            KeyPartnerImportanceDiscount = policyData.KeyPartnerImportanceDiscount;
            InternetPartnerImportanceDiscount = policyData.InternetPartnerImportanceDiscount;
            WholesalePartnerImportanceDiscount = policyData.WholesalePartnerImportanceDiscount;

            // Partner type Discount, %
            NewPartnerDiscount = policyData.NewPartnerDiscount;
            RepeatedPartnerDiscount = policyData.RepeatedPartnerDiscount;

            // Contract type Discount, %
            PurchaseAndSaleDiscount = policyData.PurchaseAndSaleDiscount;
            Commission = policyData.Commission;

            // Order type Discount, %
            PreOrderDiscount = policyData.PreOrderDiscount;
            FreeWarehouseCurrentOrderDiscount = policyData.FreeWarehouseCurrentOrderDiscount;
            FreeWarehousePastOrderDiscount = policyData.FreeWarehousePastOrderDiscount;

            // Markup for non-compliance with volumes, %
            MarkupForMismatchOfVolume = policyData.MarkupForMismatchOfVolume;

            // Planned installment, months
            PlannedInstallment = policyData.PlannedInstallment;

            // Planned prepayment, %
            PlannedPrepayment = policyData.PlannedPrepayment;

            // Annual rate, %
            AnnualRate = policyData.AnnualRate;

            // Max count of installment periods, periods
            MaxCountOfInstallmentPeriods = policyData.MaxCountOfInstallmentPeriods;

            /// ============================
            /// ==          Lists         ==
            /// ============================

            // Volume discount, string dictionary volume:%
            VolumeDiscount = policyData.VolumeDiscount;

            // Brand-mix discount, string dictionary numberOfBrands:%
            BrandMixDiscount = policyData.BrandMixDiscount;

            // Prepayment discount, string dictionary %:%
            PrepaymentDiscount = policyData.PrepaymentDiscount;
        }

        public PolicyData ToEntity()
        {
            return new PolicyData()
            {
                Id = Id,
                DiscountPolicyId = DiscountPolicyId,
                InternetKeyPartnerImportanceDiscount = InternetKeyPartnerImportanceDiscount,
                NetworkPartnerImportanceDiscount = NetworkPartnerImportanceDiscount,
                KeyPartnerImportanceDiscount = KeyPartnerImportanceDiscount,
                InternetPartnerImportanceDiscount = InternetPartnerImportanceDiscount,
                WholesalePartnerImportanceDiscount = WholesalePartnerImportanceDiscount,
                NewPartnerDiscount = NewPartnerDiscount,
                RepeatedPartnerDiscount = RepeatedPartnerDiscount,
                PurchaseAndSaleDiscount = PurchaseAndSaleDiscount,
                Commission = Commission,
                PreOrderDiscount = PreOrderDiscount,
                FreeWarehouseCurrentOrderDiscount = FreeWarehouseCurrentOrderDiscount,
                FreeWarehousePastOrderDiscount = FreeWarehousePastOrderDiscount,
                MarkupForMismatchOfVolume = MarkupForMismatchOfVolume,
                PlannedInstallment = PlannedInstallment,
                PlannedPrepayment = PlannedPrepayment,
                AnnualRate = AnnualRate,
                MaxCountOfInstallmentPeriods = MaxCountOfInstallmentPeriods,
                VolumeDiscount = VolumeDiscount,
                BrandMixDiscount = BrandMixDiscount,
                PrepaymentDiscount = PrepaymentDiscount,
            };
        }
    }
}