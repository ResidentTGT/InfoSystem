using System.Collections.Generic;
using System.Linq;
using Company.Common.Extensions;
using Company.Common.Models.Pim;

namespace WebApi.Dto.Pim
{
    public class ListDto
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Template { get; set; }

        public List<ListValueDto> ListValues { get; set; } = new List<ListValueDto>();

        public List<ListMetadataDto> ListMetadatas { get; set; } = new List<ListMetadataDto>();

        public ListDto()
        {
        }

        public ListDto(List list)
        {
            Id = list.Id;
            Name = list.Name;
            Template = list.Template;
            ListValues = list.ListValues.Select(lv => new ListValueDto(lv)).ToList();

            ListMetadatas = list.ListMetadatas.Select(
                    lm => new ListMetadataDto()
                    {
                        Id = lm.Id,
                        ListId = lm.ListId,
                        Name = lm.Name
                    }
                )
                .ToList();

            /*ListValues.ForEach(
                lv => lv.ListMetadatas.AddRange(
                    
                    ));*/

            list.ListMetadatas.ForEach(
                lm => ListValues.ForEach(
                    lv => lv.ListMetadatas.AddRange(
                        lm.ListValueMetadatas.Where(lvm => lvm.ListValueId == lv.Id)
                            .Select(
                                lvm => new ListMetadataDto()
                                {
                                    Id = lm.Id,
                                    ListId = lm.ListId,
                                    Value = lvm.Value,
                                    Name = lm.Name
                                }
                            )
                            .ToList()
                    )
                )
            );
        }

        public List ToEntity()
        {
            return new List()
            {
                Id = Id,
                Name = Name,
                Template = Template,
                ListMetadatas=ListMetadatas.Select(lm=>lm.ToEntity()).ToList(),
                ListValues = ListValues.Select(lv => lv.ToEntity()).ToList(),
            };
        }
    }
}