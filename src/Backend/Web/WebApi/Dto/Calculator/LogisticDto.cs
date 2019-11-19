using System;
using System.Collections.Generic;
using System.Linq;

namespace WebApi.Dto.Calculator
{
    public class LogisticDto
    {
        public int Id { get; set; }

        public int SeasonListValueId { get; set; }

        public int BrandListValueId { get; set; }

        /// <summary>
        /// ед.
        /// </summary>
        public int ProductsVolume { get; set; }

        /// <summary>
        ///  у.е.
        /// </summary>
        public int MoneyVolume { get; set; }

        public int BatchesCount { get; set; }

        /// <summary>
        /// ед.
        /// </summary>
        public float AdditionalFactor { get; set; }

        public List<SupplyDto> Supplies { get; set; } = new List<SupplyDto>() { };

        public DateTime CreateTime { get; set; }

        /// <summary>
        /// %, < 100
        /// </summary>
        public float Insurance { get; set; }

        /// <summary>
        /// у.е.
        /// </summary>
        public int OtherAdditional { get; set; }

        public float LogisticValueWithoutFob
        {
            get => LogisticValueWithoutFob = Supplies.Sum(s => (s.TransportCost * (s.RiskCoefficient / 100 + 1) + s.BrokerCost + s.WtsCost + s.Other) * s.BatchesCount) * (Insurance / 100 + 1) * AdditionalFactor / MoneyVolume;

            set { }
        }
    }
}
