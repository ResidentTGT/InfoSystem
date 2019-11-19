using System;
using Company.Common.Enums;
using Company.Common.Models.Deals;


namespace WebApi.Dto.Deals
{
    public class DiscountParamsDto : IEquatable<DiscountParamsDto>
    {
        public int Id { get; set; }

        public int DealId { get; set; }

        public OrderType OrderType { get; set; }

        public ContractType ContractType { get; set; }

        public float Prepayment { get; set; }

        public float Installment { get; set; }

        public float HeadDiscount { get; set; }

        public float CeoDiscount { get; set; }

        public bool ConsiderMarginality { get; set; }

        public Guid? ImplementationContract { get; set; }

        public Guid? CommissionContract { get; set; }

        public DiscountParamsDto()
        {
        }

        public DiscountParams ToEntity()
        {
            return new DiscountParams()
            {
                Id = Id,
                DealId = DealId,
                OrderType = OrderType,
                ContractType = ContractType,
                Prepayment = Prepayment,
                Installment = Installment,
                HeadDiscount = HeadDiscount,
                CeoDiscount = CeoDiscount,
                ConsiderMarginality = ConsiderMarginality,
                ImplementationContract = ImplementationContract,
                CommissionContract = CommissionContract,
            };
        }

        public DiscountParamsDto(DiscountParams discountParams)
        {
            Id = discountParams.Id;
            DealId = discountParams.DealId;
            OrderType = discountParams.OrderType;
            ContractType = discountParams.ContractType;
            Prepayment = discountParams.Prepayment;
            Installment = discountParams.Installment;
            HeadDiscount = discountParams.HeadDiscount;
            CeoDiscount = discountParams.CeoDiscount;
            ConsiderMarginality = discountParams.ConsiderMarginality;
            ImplementationContract = discountParams.ImplementationContract;
            CommissionContract = discountParams.CommissionContract;
        }

        public bool Equals(DiscountParamsDto other)
            => other != null
               && OrderType.Equals(other.OrderType)
               && ContractType.Equals(other.ContractType)
               && Prepayment.Equals(other.Prepayment)
               && Installment.Equals(other.Installment)
               && HeadDiscount.Equals(other.HeadDiscount)
               && CeoDiscount.Equals(other.CeoDiscount)
               && ConsiderMarginality.Equals(other.ConsiderMarginality);
    }
}