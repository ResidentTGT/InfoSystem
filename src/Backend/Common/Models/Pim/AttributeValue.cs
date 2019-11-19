using System;
using NpgsqlTypes;

namespace Company.Common.Models.Pim
{
    public class AttributeValue
    {
        public int Id { get; set; }

        public int? ListValueId { get; set; }

        public ListValue ListValue { get; set; }

        public string StrValue { get; set; }

        public string SearchString { get; set; }

        public double? NumValue { get; set; }

        public bool? BoolValue { get; set; }

        public DateTime? DateValue { get; set; }

        public DateTime CreateTime { get; set; }

        public int? CreatorId { get; set; }

        public int AttributeId { get; set; }

        public Attribute Attribute { get; set; }

        public int ProductId { get; set; }

        public Product Product { get; set; }

        public NpgsqlTsVector SearchVector { get; set; }

    }
}
