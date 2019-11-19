using System;
using System.Collections.Generic;
using System.Text;

namespace Company.Common.Models.Pim
{
    public class ProductSearch
    {
        public int Id { get; set; }

        public int? ParentId { get; set; }

        public int? ImportId { get; set; }

        public int? CategoryId { get; set; }

        public ProductSearch ParentProductSearch { get; set; }

        public int ProductId { get; set; }

        public Product Product { get; set; }

        public string NameOrigEng { get; set; }

        public double BwpMin { get; set; }

        public NpgsqlTypes.NpgsqlRange<int> AgeMonthRange { get; set; } = new NpgsqlTypes.NpgsqlRange<int>();

        public NpgsqlTypes.NpgsqlRange<int> AgeYearRange { get; set; } = new NpgsqlTypes.NpgsqlRange<int>();

        public List<string> FullSearchArray { get; set; } = new List<string>();

        public List<string> SmartSearchArray { get; set; } = new List<string>();

        public ICollection<ProductSearch> SubProductSearch { get; set; } = new List<ProductSearch>();
    }
}
