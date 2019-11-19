using System;
using System.Collections.Generic;

namespace Company.Common.Models.Pim
{
    public class ListValue
    {
        public int Id { get; set; }

        public string Value { get; set; }

        public int ListId { get; set; }

        public List List { get; set; }

        public int? DeleterId { get; set; }

        public DateTime? DeleteTime { get; set; }

        public ICollection<AttributeValue> AttributeValues { get; set; } = new List<AttributeValue>();

        public ICollection<ListValueMetadata> ListValueMetadatas { get; set; } = new List<ListValueMetadata>();

        public ListValue()
        {

        }
    }
}
