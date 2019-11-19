using System;
using System.Collections.Generic;
using System.Linq;
using Company.Common.Enums;
using Company.Common.Models.Deals;

namespace WebApi.Dto.Calculator
{
    public class DealDto
    {
        public int Id { get; set; }
        public string Contractor { get; set; }
        public int SeasonId { get; set; }
        public int OrderFormId { get; set; }
        public int ContractId { get; set; }
        public string Tin { get; set; }
        public string Rrc { get; set; }
        public int ManagerId { get; set; }
        public float ManagerMarginality { get; set; }
        public DateTime? CompleteDate { get; set; }
        public DateTime CreateDate { get; set; }
        public int BrandMix { get; set; }
        public string Brand { get; set; }
        public int BrandId { get; set; }
        public string ManagerName { get; set; }
        public float Volume { get; set; }
        public float VolumePure { get; set; }
        public string PartnerNameOnMarket { get; set; }
        public float Discount { get; set; }
        public float NetCost { get; set; }
        public float NetCostPure { get; set; }
        public DealStatus Status { get; set; }
        public PartnersType PartnersType { get; set; }
        public bool Repeatedness { get; set; }
        public string Comment { get; set; }
        public DateTime? Upload1cTime { get; set; }
        public DeliveryType? Delivery { get; set; }
        public ProductType ProductType { get; set; }

        public DiscountParamsDto DiscountParams { get; set; }
        public List<HeadDiscountRequestDto> HeadDiscountRequests { get; set; } = new List<HeadDiscountRequestDto>();
        public List<DealProductDto> DealProducts { get; set; } = new List<DealProductDto>();

        public DealDto()
        {
        }

        public DealDto(Deal deal)
        {
            Id = deal.Id;
            Contractor = deal.Contractor;
            Tin = deal.Tin;
            Rrc = deal.Rrc;
            ManagerId = deal.ManagerId;
            CompleteDate = deal.CompleteDate;
            BrandMix = deal.BrandMix;
            Brand = deal.Brand;
            BrandId = deal.BrandId;
            Volume = deal.Volume;
            SeasonId = deal.SeasonId;
            VolumePure = deal.VolumePure;
            PartnerNameOnMarket = deal.PartnerNameOnMarket;
            Discount = deal.Discount;
            NetCost = deal.NetCost;
            NetCostPure = deal.NetCostPure;
            Status = deal.DealStatus;
            PartnersType = deal.PartnersType;
            Repeatedness = deal.Repeatedness;
            OrderFormId = deal.OrderFormId;
            ContractId = deal.ContractId;
            CreateDate = deal.CreateDate;
            Comment = deal.Comment;
            Delivery = deal.Delivery;
            ProductType = deal.ProductType;
            Upload1cTime = deal.Upload1cTime;

            DiscountParams = new DiscountParamsDto(deal.DiscountParams.LastOrDefault());

            var lastHeadDiscount = deal.HeadDiscountRequests.LastOrDefault(hdp => hdp.Receiver == ReceiverType.Head);
            var lastCeoDiscount = deal.HeadDiscountRequests.LastOrDefault(hdp => hdp.Receiver == ReceiverType.Ceo);

            if (lastHeadDiscount != null)
            {
                HeadDiscountRequests.Add(new HeadDiscountRequestDto(lastHeadDiscount));
            }

            if (lastCeoDiscount != null)
            {
                HeadDiscountRequests.Add(new HeadDiscountRequestDto(lastCeoDiscount));
            }

            DealProducts = deal.DealProducts.Select(dp => new DealProductDto(dp)).ToList();
        }

        public Deal ToEntity()
        {
            return new Deal()
            {
                Id = Id,
                Contractor = Contractor,
                SeasonId = SeasonId,
                OrderFormId = OrderFormId,
                ContractId = ContractId,
                Tin = Tin,
                Rrc = Rrc,
                ManagerId = ManagerId,
                ManagerMarginality = ManagerMarginality,
                CompleteDate = CompleteDate,
                CreateDate = CreateDate,
                BrandMix = BrandMix,
                Brand = Brand,
                BrandId = BrandId,
                Volume = Volume,
                VolumePure = VolumePure,
                PartnerNameOnMarket = PartnerNameOnMarket,
                Discount = Discount,
                NetCost = NetCost,
                NetCostPure = NetCostPure,
                DealStatus = Status,
                PartnersType = PartnersType,
                Repeatedness = Repeatedness,
                Delivery = Delivery,
                ProductType = ProductType,
                Upload1cTime = Upload1cTime,
                DiscountParams = new List<DiscountParams>() {DiscountParams.ToEntity()},
                HeadDiscountRequests = HeadDiscountRequests.Select(hdr => hdr.ToEntity()).ToList(),
                DealProducts = DealProducts.Select(dp => dp.ToEntity()).ToList(),
                Comment = Comment
            };
        }
    }
}