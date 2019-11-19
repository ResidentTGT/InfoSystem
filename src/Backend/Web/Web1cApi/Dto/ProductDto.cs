using System.Collections.Generic;
using System.Linq;
using Company.Common.Models.Pim;

namespace Web1cApi.Dto
{
    public class ProductDto
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Sku { get; set; }

        public int? CategoryId { get; set; }

        public List<int> ImgsIds { get; set; } = new List<int>();

        public List<int> DocsIds { get; set; } = new List<int>();

        public List<int> VideosIds { get; set; }

        public int? MainImgId { get; set; }

        public List<ProductProperties> Properties { get; set; } = new List<ProductProperties>();

        public ProductDto()
        {
            ImgsIds = new List<int>();
            DocsIds = new List<int>();
            VideosIds = new List<int>();
            Properties = new List<ProductProperties>();
        }

        public ProductDto(Product product)
        {
            Id = product.Id;
            Name = product.Name;
            Sku = product.Sku;
            CategoryId = product.CategoryId;

            ImgsIds = product.ProductFiles.Where(f => f.FileType == FileType.Image).Select(pf => pf.FileId).ToList();
            DocsIds = product.ProductFiles.Where(f => f.FileType == FileType.Document).Select(pf => pf.FileId).ToList();
            VideosIds = product.ProductFiles.Where(f => f.FileType == FileType.Video).Select(pf => pf.FileId).ToList();

            MainImgId = product.ProductFiles.Where(pf => pf.IsMain).Select(pf => pf.FileId).FirstOrDefault();

            Properties = product.AttributeValues.Select(av => new ProductProperties(av)).ToList();
        }

        public Product ToEntity()
        {
            var product = new Product()
            {
                Id = Id,
                Name = Name,
                Sku = Sku,
                CategoryId = CategoryId,
                AttributeValues = Properties.Select(p => p.ToEntity()).ToList(),

            };

            ImgsIds.ForEach(i => product.ProductFiles.Add(new ProductFile() { FileId = i, FileType = FileType.Image, IsMain = i == MainImgId }));
            VideosIds.ForEach(i => product.ProductFiles.Add(new ProductFile() { FileId = i, FileType = FileType.Video }));
            DocsIds.ForEach(i => product.ProductFiles.Add(new ProductFile() { FileId = i, FileType = FileType.Document }));

            return product;
        }
    }
}