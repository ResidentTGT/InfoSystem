using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Company.Common.Models.Identity;
using SectionPermission = Company.Common.Models.Users.SectionPermission;

namespace WebApi.Dto.Users
{
    public class RolePermissionDto
    {
        public Role Role { get; set; }
        public List<SectionPermission> SectionPermission { get; set; }
    }
}