namespace WebApi.Dto.Calculator
{
    public class MaxDiscountsDto
    {
        public int Id { get; set; }

        public int DealId { get; set; }

        public float MaxDiscount { get; set; }

        public float MaxManagerDiscount { get; set; }
    }
}
