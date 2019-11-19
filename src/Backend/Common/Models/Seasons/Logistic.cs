using System;
using System.Collections.Generic;

namespace Company.Common.Models.Seasons
{
    public class Logistic
    {
        public int Id { get; set; }

        public DateTime CreateTime { get; set; }

        public DateTime? DeleteTime { get; set; }

        public int CreatorId { get; set; }

        public int? DeleterId { get; set; }

        public int SeasonListValueId { get; set; }

        public int BrandListValueId { get; set; }

        public int ProductsVolume { get; set; }

        public int MoneyVolume { get; set; }

        public int BatchesCount { get; set; }

        public float AdditionalFactor { get; set; }

        /// <summary>
        /// %
        /// </summary>
        public float Insurance { get; set; }

        /// <summary>
        /// у.е.
        /// </summary>
        public int OtherAdditional { get; set; }

        public ICollection<Supply> Supplies { get; set; } = new List<Supply>() { };
    }
}
