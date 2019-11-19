using Company.Common.Models.Identity;
using Company.Common.Models.Users;
using System;
using System.Collections.Generic;
using System.Text;

namespace WebApi.Dto.Users
{
    public class ModulePermissoinDto
    {
        public Module Module { get; set; }

        public List<Company.Common.Models.Users.SectionPermission> SectionPermissions { get; set; } = new List<Company.Common.Models.Users.SectionPermission>();

        public List<ResourcePermissionDto> ResourcePermissions { get; set; } = new List<ResourcePermissionDto>();
    }
}
