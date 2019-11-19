using Company.Common.Models.Pim;

namespace WebApi.Dto.Pim
{
    public class ListMetadataDto
    {
        public int Id { get; set; }

        public int ListId { get; set; }

        public string Name { get; set; }

        public string Value { get; set; }

        public ListMetadataDto()
        {
        }

        public ListMetadataDto(ListMetadata listMetadata)
        {
            Id = listMetadata.Id;
            ListId = listMetadata.ListId;
            Name = listMetadata.Name;

            //ListMetadataPermissions = listMetadata.ListMetadataPermissions.Select(lmp => new ListMetadataPermissionDto(lmp)).ToList();
        }

        public ListMetadata ToEntity()
        {
            return new ListMetadata()
            {
                Id = Id,
                ListId = ListId,
                Name = Name,
                
            };
        }
    }
}