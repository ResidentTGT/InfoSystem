using System;

namespace WebApi.Dto.Calculator
{
    public class SupplyDto
    {
        public int Id { get; set; }

        public int LogisticId { get; set; }

        public int BatchesCount { get; set; }

        public float RiskCoefficient { get; set; }

        public DateTime DeliveryDate { get; set; }

        public int BrokerCost { get; set; }

        public int WtsCost { get; set; }

        public int TransportCost { get; set; }

        /// <summary>
        /// у.е.
        /// </summary>
        public int Other { get; set; }
    }
}
