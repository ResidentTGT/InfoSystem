using System;
using Company.Common.Enums;

namespace Company.Common.Models.Deals
{
    public class DiscountParams
    {
        public int Id { get; set; }

        public int DealId { get; set; }

        public virtual Deal Deal { get; set; }

        public DateTime CreateDate { get; set; }

        public int CreatorId { get; set; }

        public OrderType OrderType { get; set; }

        public ContractType ContractType { get; set; }

        public float Prepayment { get; set; }

        public float Installment { get; set; }

        public float HeadDiscount { get; set; }

        public float CeoDiscount { get; set; }

        public bool ConsiderMarginality { get; set; }

        public Guid? ImplementationContract { get; set; }

        public Guid? CommissionContract { get; set; }
    }
}
