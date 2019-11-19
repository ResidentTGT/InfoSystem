using System;
using System.Collections.Generic;
using System.Text;
using Company.Common.Models.Identity;

namespace IdentityDatabase.Dto
{
    public class ModulePermissoinDto
    {
        public Module Module { get; set; }

        public List<SectionPermission> SectionPermissions { get; set; } = new List<SectionPermission>();

        public List<ResourcePermissionDto> ResourcePermissions { get; set; } = new List<ResourcePermissionDto>();
    }
}
