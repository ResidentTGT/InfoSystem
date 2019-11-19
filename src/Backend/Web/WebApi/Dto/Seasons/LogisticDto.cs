using System;
using System.Collections.Generic;
using System.Linq;
using Company.Common.Models.Seasons;

namespace WebApi.Dto.Seasons
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

        public LogisticDto()
        {
        }

        public LogisticDto(Logistic logistic)
        {
            Id = logistic.Id;
            SeasonListValueId = logistic.SeasonListValueId;
            BrandListValueId = logistic.BrandListValueId;
            ProductsVolume = logistic.ProductsVolume;
            MoneyVolume = logistic.MoneyVolume;
            BatchesCount = logistic.BatchesCount;
            AdditionalFactor = logistic.AdditionalFactor;
            Supplies = logistic.Supplies.Select(s => new SupplyDto(s)).ToList();
            CreateTime = logistic.CreateTime;
            Insurance = logistic.Insurance;
            OtherAdditional = logistic.OtherAdditional;
        }

        public Logistic ToEntity()
        {
            return new Logistic()
            {
                Id = Id,
                SeasonListValueId = SeasonListValueId,
                BrandListValueId = BrandListValueId,
                ProductsVolume = ProductsVolume,
                MoneyVolume = MoneyVolume,
                BatchesCount = BatchesCount,
                AdditionalFactor = AdditionalFactor,
                Supplies = Supplies.Select(s => s.ToEntity()).ToList(),
                CreateTime = CreateTime,
                Insurance = Insurance,
                OtherAdditional = OtherAdditional,
            };
        }
    }
}