using System;
using Company.Common.Models.Seasons;

namespace WebApi.Dto.Seasons
{
    public class SupplyDto
    {
        public int Id { get; set; }

        public int LogisticId { get; set; }

        public int BatchesCount { get; set; }

        public float RiskCoefficient { get; set; }

        public DateTime DeliveryDate { get; set; }

        public DateTime FabricDate { get; set; }

        public int BrokerCost { get; set; }

        public int WtsCost { get; set; }

        public int TransportCost { get; set; }

        /// <summary>
        /// у.е.
        /// </summary>
        public int Other { get; set; }

        public SupplyDto()
        {
        }

        public SupplyDto(Supply supply)
        {
            Id = supply.Id;
            LogisticId = supply.LogisticId;
            BatchesCount = supply.BatchesCount;
            RiskCoefficient = supply.RiskCoefficient;
            DeliveryDate = supply.DeliveryDate;
            FabricDate = supply.FabricDate;
            BrokerCost = supply.BrokerCost;
            WtsCost = supply.WtsCost;
            TransportCost = supply.TransportCost;
            Other = supply.Other;
        }

        public Supply ToEntity()
        {
            return new Supply()
            {
                Id = Id,
                LogisticId = LogisticId,
                BatchesCount = BatchesCount,
                RiskCoefficient = RiskCoefficient,
                DeliveryDate = DeliveryDate,
                FabricDate = FabricDate,
                BrokerCost = BrokerCost,
                WtsCost = WtsCost,
                TransportCost = TransportCost,
                Other = Other,
            };
        }
    }
}