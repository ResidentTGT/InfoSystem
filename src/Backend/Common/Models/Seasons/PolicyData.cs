﻿using System;

namespace Company.Common.Models.Seasons
{
    public class PolicyData
    {
        public int Id { get; set; }

        public int DiscountPolicyId { get; set; }

        public DiscountPolicy DiscountPolicy { get; set; }

        public DateTime CreateTime { get; set; }

        public DateTime? DeleteTime { get; set; }

        public int CreatorId { get; set; }

        public int? DeleterId { get; set; }

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
        /// ед, months
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
        /// ед, periods
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
    }
}
