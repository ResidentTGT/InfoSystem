using System.Collections.Generic;

namespace WebApi.Dto.Deals
{
    public class ListValueDto
    {
        public int Id { get; set; }

        public string Value { get; set; }

        public int ListId { get; set; }

        public List<ListMetadataDto> ListMetadatas { get; set; } = new List<ListMetadataDto>();
    }
}
