using Company.Common.Enums;
using Company.Common.Models.Deals;

namespace WebApi.Dto.Calculator
{
    public class HeadDiscountRequestDto
    {
        public int Id { get; set; }

        public int DealId { get; set; }

        public float Discount { get; set; }

        public ReceiverType Receiver { get; set; }

        public bool? Confirmed { get; set; }

        public HeadDiscountRequestDto()
        {

        }

        public HeadDiscountRequestDto(HeadDiscountRequest headDiscountRequest)
        {
            Id = headDiscountRequest.Id;
            DealId = headDiscountRequest.DealId;
            Discount = headDiscountRequest.Discount;
            Receiver = headDiscountRequest.Receiver;
            Confirmed = headDiscountRequest.Confirmed;
        }
        public HeadDiscountRequest ToEntity()
        {
            return new HeadDiscountRequest()
            {
                Id = Id,
                DealId = DealId,
                Discount = Discount,
                Receiver = Receiver,
                Confirmed = Confirmed,
            };
        }
    }
}