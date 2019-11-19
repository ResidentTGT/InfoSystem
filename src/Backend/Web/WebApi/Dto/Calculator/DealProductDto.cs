using Company.Common.Models.Deals;

namespace WebApi.Dto.Calculator
{
    public class DealProductDto
    {
        public int DealId { get; set; }

        public int ProductId { get; set; }

        public int Count { get; set; }

        public DealProductDto()
        {
        }

        public DealProductDto(DealProduct dealProduct)
        {
            DealId = dealProduct.DealId;
            ProductId = dealProduct.ProductId;
            Count = dealProduct.Count;
        }

        public DealProduct ToEntity()
        {
            return new DealProduct()
            {
                DealId = DealId,
                ProductId = ProductId,
                Count = Count,
            };
        }
    }
}