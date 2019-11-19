using System.Collections.Generic;
using System.Linq;
using Company.Common.Enums;
using Company.Common.Models.Pim;

namespace WebApi.Dto.Pim
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

        public int? ParentId { get; set; }

        public ModelLevel ModelLevel { get; set; }

        public List<ProductDto> SubProducts { get; set; }

        public ProductDto ParentProduct { get; set; }

        public List<ProductProperties> Properties { get; set; } = new List<ProductProperties>();

        public ProductDto()
        {
            ImgsIds = new List<int>();
            DocsIds = new List<int>();
            VideosIds = new List<int>();
            Properties = new List<ProductProperties>();
            SubProducts = new List<ProductDto>();
        }

        public ProductDto(Product product)
        {
            Id = product.Id;
            Name = product.Name;
            Sku = product.Sku;
            CategoryId = product.CategoryId;
            ParentId = product.ParentId;
            ModelLevel = product.ModelLevel;
            ImgsIds = product.ProductFiles.Where(f => f.FileType == FileType.Image).Select(pf => pf.FileId).ToList();
            DocsIds = product.ProductFiles.Where(f => f.FileType == FileType.Document).Select(pf => pf.FileId).ToList();
            VideosIds = product.ProductFiles.Where(f => f.FileType == FileType.Video).Select(pf => pf.FileId).ToList();
            MainImgId = product.ProductFiles.Where(pf => pf.IsMain).Select(pf => pf.FileId).FirstOrDefault();
            ParentProduct = product.ParentProduct != null ? new ProductDto(product.ParentProduct) : null;
            SubProducts = product.SubProducts.Select(sp => new ProductDto(sp)).ToList();
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
                ModelLevel = ModelLevel,
                ParentId = ParentId,
                AttributeValues = Properties.Select(p => p.ToEntity()).ToList(),
                SubProducts = SubProducts.Select(sp => sp.ToEntity()).ToList()
            };


            ImgsIds.ForEach(i => product.ProductFiles.Add(new ProductFile() { FileId = i, FileType = FileType.Image, IsMain = i == MainImgId }));
            VideosIds.ForEach(i => product.ProductFiles.Add(new ProductFile() { FileId = i, FileType = FileType.Video }));
            DocsIds.ForEach(i => product.ProductFiles.Add(new ProductFile() { FileId = i, FileType = FileType.Document }));

            return product;
        }
    }
}