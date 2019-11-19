using System.Collections.Generic;
using System.Linq;
using Company.Common.Models.Identity;
using SectionPermission = Company.Common.Models.Users.SectionPermission;


namespace WebApi.Dto.Pim
{
    public class RoleDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public Module Module { get; set; }
        public bool IsUser { get; set; }

        public List<SectionPermissionDto> SectionPermissions { get; set; } = new List<SectionPermissionDto>();

        public RoleDto(Role role, List<SectionPermission> sectionPermissions)
        {
            Id = role.Id;
            Name = role.Name;
            Module = role.Module;

            SectionPermissions.AddRange(sectionPermissions.Select(sp => new SectionPermissionDto(sp)));
        }
    }
}