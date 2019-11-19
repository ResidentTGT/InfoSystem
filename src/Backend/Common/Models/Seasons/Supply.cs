using System;

namespace Company.Common.Models.Seasons
{
    public class Supply
    {
        public int Id { get; set; }

        public DateTime? DeleteTime { get; set; }

        public int? DeleterId { get; set; }

        public int LogisticId { get; set; }

        public Logistic Logistic { get; set; }

        public int BatchesCount { get; set; }

        public float RiskCoefficient { get; set; }

        public DateTime DeliveryDate { get; set; }

        public DateTime FabricDate { get; set; }

        public int TransportCost { get; set; }

        public int BrokerCost { get; set; }

        public int WtsCost { get; set; }

        /// <summary>
        /// у.е.
        /// </summary>
        public int Other { get; set; }
    }
}
