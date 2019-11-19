using System;
using System.Collections.Generic;
using Company.Common.Enums;

namespace Company.Common.Models.Deals
{
    public class Deal
    {
        public int Id { get; set; }

        public int OrderFormId { get; set; }

        public int ContractId { get; set; }

        public int SeasonId { get; set; }

        public int BrandId { get; set; }

        public int ManagerId { get; set; }

        public int DeleterId { get; set; }

        public int BrandMix { get; set; }

        public string Contractor { get; set; }

        public string Tin { get; set; }

        public string Rrc { get; set; }

        public string Brand { get; set; }

        public string PartnerNameOnMarket { get; set; }

        public string Comment { get; set; }

        public DeliveryType? Delivery { get; set; }

        public ProductType ProductType { get; set; }

        public DateTime CreateDate { get; set; }

        public DateTime? DeleteTime { get; set; }

        public DateTime? CompleteDate { get; set; }

        public DateTime? Upload1cTime { get; set; }

        public float Volume { get; set; }

        public float VolumePure { get; set; }

        public float NetCost { get; set; }

        public float NetCostPure { get; set; }

        public float Discount { get; set; }

        public float DealMarginality { get; set; }

        public float ManagerMarginality { get; set; }

        public DealStatus DealStatus { get; set; }

        public PartnersType PartnersType { get; set; }

        public bool Repeatedness { get; set; }

        public ICollection<DealProduct> DealProducts { get; set; } = new List<DealProduct>();

        public ICollection<DiscountParams> DiscountParams { get; set; } = new List<DiscountParams>();

        public ICollection<HeadDiscountRequest> HeadDiscountRequests { get; set; } = new List<HeadDiscountRequest>();

        public ICollection<DealFile> DealFiles { get; set; } = new List<DealFile>();
    }
}