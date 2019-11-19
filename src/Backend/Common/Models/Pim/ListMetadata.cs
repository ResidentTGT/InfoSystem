using System;
using System.Collections.Generic;

namespace Company.Common.Models.Pim
{
    public class ListMetadata
    {
        public int Id { get; set; }

        public int ListId { get; set; }

        public virtual List List { get; set; }

        public string Name { get; set; }

        public DateTime? DeleteTime { get; set; }

        public int? DeleterId { get; set; }

        public ICollection<ListMetadataPermission> ListMetadataPermissions { get; set; } = new List<ListMetadataPermission>();

        public ICollection<ListValueMetadata> ListValueMetadatas { get; set; } = new List<ListValueMetadata>();
    }
}
