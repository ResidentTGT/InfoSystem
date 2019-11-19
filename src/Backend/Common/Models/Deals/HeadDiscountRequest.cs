using System;
using Company.Common.Enums;

namespace Company.Common.Models.Deals
{
    public class HeadDiscountRequest
    {
        public int Id { get; set; }

        public int DealId { get; set; }

        public virtual Deal Deal { get; set; }

        public float Discount { get; set; }

        public ReceiverType Receiver { get; set; }

        public int CreatorId { get; set; }

        public DateTime CreateTime { get; set; }

        public bool? Confirmed { get; set; }
    }
}
