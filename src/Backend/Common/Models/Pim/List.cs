using System;
using System.Collections.Generic;

namespace Company.Common.Models.Pim
{
    public class List
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public DateTime CreateTime { get; set; }

        public DateTime? DeleteTime { get; set; }

        public int? CreatorId { get; set; }

        public int? DeleterId { get; set; }

        public string Template { get; set; }

        public ICollection<Attribute> Attributes { get; set; } = new List<Attribute>();

        public ICollection<ListValue> ListValues { get; set; } = new List<ListValue>();

        public ICollection<ListMetadata> ListMetadatas { get; set; } = new List<ListMetadata>();

        public List()
        {

        }
    }
}
