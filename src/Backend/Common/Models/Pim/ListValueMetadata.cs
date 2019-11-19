using System;

namespace Company.Common.Models.Pim
{
    public class ListValueMetadata
    {
        public int Id { get; set; }

        public int ListValueId { get; set; }

        public virtual ListValue ListValue { get; set; }

        public int ListMetadataId { get; set; }

        public virtual ListMetadata ListMetadata { get; set; }

        public string Value { get; set; }

        public DateTime? DeleteTime { get; set; }

        public int? DeleterId { get; set; }
    }
}
