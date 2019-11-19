using System.Collections.Generic;
using System.Linq;
using Company.Common.Models.Pim;

namespace WebApi.Dto.Calculator
{
    public class ProductDto
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Sku { get; set; }

        public int? CategoryId { get; set; }

        public List<int> ImgsIds { get; set; }

        public List<int> DocsIds { get; set; }

        public List<int> VideosIds { get; set; }

        public int? MainImgId { get; set; }

        public List<ProductProperties> Properties { get; set; }

        public ProductDto()
        {
            ImgsIds = new List<int>();
            DocsIds = new List<int>();
            VideosIds = new List<int>();
            Properties = new List<ProductProperties>();
        }

        public Product ToEntity()
        {
            return new Product()
            {
                Id = Id,
                Name = Name,
                Sku = Sku,
                CategoryId = CategoryId,
                AttributeValues = Properties.Select(p => p.AttributeValue.ToEntity()).ToList(),
            };
        }
    }
}