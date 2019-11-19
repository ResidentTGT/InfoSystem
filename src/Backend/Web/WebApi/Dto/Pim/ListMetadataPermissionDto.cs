using Company.Common.Models.Identity;
using Company.Common.Models.Pim;

namespace WebApi.Dto.Pim
{
    public class ListMetadataPermissionDto
    {
        public int Id { get; set; }

        public int ListMetadataId { get; set; }

        public int RoleId { get; set; }

        public DataAccessMethods Value { get; set; }

        public ListMetadataPermissionDto(ListMetadataPermission listMetadataPermission)
        {
            Id = listMetadataPermission.Id;
            ListMetadataId = listMetadataPermission.ListMetadataId;
            RoleId = listMetadataPermission.RoleId;
            Value = listMetadataPermission.Value;
        }
    }
}
