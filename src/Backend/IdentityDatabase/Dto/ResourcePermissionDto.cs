using System;
using System.Collections.Generic;
using System.Text;
using Company.Common.Models.Identity;

namespace IdentityDatabase.Dto
{
    public class ResourcePermissionDto
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public ResourcePermissionsValues Value { get; set; }

        public int RoleId { get; set; }
    }
}
