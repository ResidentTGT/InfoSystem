using Company.Common.Models.Pim;

namespace WebApi.Dto.Pim
{
    public class ListValueMetadataDto
    {
        public int Id { get; set; }

        public int ListValueId { get; set; }

        public int ListMetadataId { get; set; }

        public string Value { get; set; }

        public ListValueMetadataDto(ListValueMetadata listValueMetadata)
        {
            Id = listValueMetadata.Id;
            ListValueId = listValueMetadata.ListValueId;
            ListMetadataId = listValueMetadata.ListMetadataId;
            Value = listValueMetadata.Value;
        }
    }
}
