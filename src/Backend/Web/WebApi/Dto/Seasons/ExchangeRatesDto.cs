using Company.Common.Models.Seasons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApi.Dto.Seasons
{
    public class ExchangeRatesDto
    {
        public int Id { get; set; }

        public float? EurUsd { get; set; }

        public float? EurRub { get; set; }

        public ExchangeRatesDto()
        {
        }

        public ExchangeRatesDto(ExchangeRates exchangeRates)
        {
            Id = exchangeRates.Id;
            EurRub = exchangeRates.EurRub;
            EurUsd = exchangeRates.EurUsd;
        }

        public ExchangeRates ToEntity()
        {
            return new ExchangeRates()
            {
                Id = Id,
                EurRub = EurRub,
                EurUsd = EurUsd
            };
        }
    }
}
