using System.Collections.Generic;
using System.Linq;
using Company.Common.Models.Pim;

namespace WebApi.Dto.Pim
{
    public class ListValueDto
    {
        public int Id { get; set; }

        public string Value { get; set; }

        public int ListId { get; set; }

        public List<ListMetadataDto> ListMetadatas { get; set; } = new List<ListMetadataDto>();

        public ListValueDto()
        {
        }

        public ListValueDto(ListValue listValue)
        {
            Id = listValue.Id;
            Value = listValue.Value;
            ListId = listValue.ListId;

            ListMetadatas = listValue.ListValueMetadatas.Select(lmv => new ListMetadataDto()
            {
                Id = lmv.ListMetadataId,
                ListId = listValue.ListId,
                Value = lmv.Value,
                Name = lmv.ListMetadata.Name
            })
                .ToList();
        }

        public ListValue ToEntity()
        {
            var lv = new ListValue()
            {
                Id = Id,
                Value = Value,
                ListId = ListId,
            };
            ListMetadatas.ForEach(lmd => lv.ListValueMetadatas.Add(new ListValueMetadata()
            {
                Value = lmd.Value,
                ListMetadata = ListMetadatas.First(lm => lm.Name == lmd.Name).ToEntity()
            }));

            return lv;
        }
    }
}