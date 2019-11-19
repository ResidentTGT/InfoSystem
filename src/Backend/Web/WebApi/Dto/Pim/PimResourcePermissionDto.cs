using Company.Common.Models.Users;

namespace WebApi.Dto.Pim
{
    public class PimResourcePermissionDto
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public ResourcePermissionsValues Value { get; set; }

        public int RoleId { get; set; }

        public PimResourcePermissionDto() { }

        public PimResourcePermissionDto(ResourcePermission rp)
        {
            Id = rp.Id;
            Name = rp.Name;
            Value = rp.Value;
            RoleId = rp.RoleId;
        }

        public ResourcePermission ToEntity()
        {
            return new ResourcePermission()
            {
                Id = Id,
                Name = Name,
                RoleId = RoleId,
                Value = Value
            };
        }
    }
}