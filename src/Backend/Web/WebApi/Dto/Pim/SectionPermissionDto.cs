using Company.Common.Models.Users;

namespace WebApi.Dto.Pim
{
    public class SectionPermissionDto
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public int RoleId { get; set; }

        public SectionPermissionDto() { }

        public SectionPermissionDto(SectionPermission sectionPermission)
        {
            Id = sectionPermission.Id;
            Name = sectionPermission.Name;
            RoleId = sectionPermission.RoleId;
        }

        public SectionPermission ToEntity()
        {
            return new SectionPermission()
            {
                Id = Id,
                Name = Name,
                RoleId = RoleId
            };
        }
    }
}