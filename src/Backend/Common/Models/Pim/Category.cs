using System;
using System.Collections.Generic;

namespace Company.Common.Models.Pim
{
    public class Category
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public DateTime CreateTime { get; set; }

        public DateTime? DeleteTime { get; set; }

        public int? CreatorId { get; set; }

        public int? DeleterId { get; set; }

        public int? ParentId { get; set; }

        public Category ParentCategory { get; set; }

        public ICollection<Category> SubCategories { get; set; } = new List<Category>();

        public ICollection<AttributeCategory> AttributeCategories { get; set; } = new List<AttributeCategory>();

        public ICollection<Product> Products { get; set; } = new List<Product>();

        public Category()
        {

        }
    }
}
