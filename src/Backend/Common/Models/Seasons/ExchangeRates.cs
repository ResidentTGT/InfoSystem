using System;
using System.Collections.Generic;
using System.Text;

namespace Company.Common.Models.Seasons
{
    public class ExchangeRates
    {
        public int Id { get; set; }

        public DateTime CreateTime { get; set; }

        public DateTime? DeleteTime { get; set; }

        public int CreatorId { get; set; }

        public int? DeleterId { get; set; }

        public float? EurUsd { get; set; }

        public float? EurRub { get; set; }

        public int DiscountPolicyId { get; set; }

        public virtual DiscountPolicy DiscountPolicy { get; set; }
    }
}
