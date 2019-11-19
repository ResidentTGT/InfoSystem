namespace WebApi.Dto.Deals
{
    public class SliderParams
    {
        public int Id { get; set; }

        public int DealId { get; set; }

        public virtual DealDto Deal { get; set; }

        public float MaxDiscount { get; set; }

        public float MinDiscount { get; set; }
    }
}
