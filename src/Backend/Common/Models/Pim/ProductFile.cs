namespace Company.Common.Models.Pim
{
    public class ProductFile
    {
        public int FileId { get; set; }

        public int ProductId { get; set; }

        public Product Product { get; set; }

        public bool IsMain { get; set; }

        public FileType FileType { get; set; }
    }

    public enum FileType
    {
        Image = 1,
        Video = 2,
        Document = 3
    }
}
