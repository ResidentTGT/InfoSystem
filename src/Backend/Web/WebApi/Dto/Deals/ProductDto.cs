using System;
using System.Collections.Generic;

namespace WebApi.Dto.Deals
{
    public class ProductDto
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Sku { get; set; }

        public List<ProductProperties> Properties { get; set; } = new List<ProductProperties>() { };

        public ProductDto()
        {
        }
    }

    public class Attribute
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public int? ListId { get; set; }
    }

    public class AttributeValue
    {
        public int Id { get; set; }

        public int? ListValueId { get; set; }

        public string StrValue { get; set; }

        public double? NumValue { get; set; }

        public bool? BoolValue { get; set; }

        public DateTime? DateValue { get; set; }
    }

    public class ProductProperties
    {
        public Attribute Attribute { get; set; }
        public AttributeValue AttributeValue { get; set; }
    }

}
