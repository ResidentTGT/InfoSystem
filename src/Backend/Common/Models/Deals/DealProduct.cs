namespace Company.Common.Models.Deals
{
    public class DealProduct
    {
        public int DealId { get; set; }

        public virtual Deal Deal { get; set; }

        public int ProductId { get; set; }

        public int Count { get; set; }
    }
}
