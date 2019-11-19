namespace Company.Common.Models.Deals
{
    public class DealFile
    {
        public int FileId { get; set; }

        public int DealId { get; set; }

        public virtual Deal Deal { get; set; }
    }
}
