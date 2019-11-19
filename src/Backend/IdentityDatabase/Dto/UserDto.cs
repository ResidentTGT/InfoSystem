using System;
using System.Collections.Generic;
using System.Text;
using Company.Common.Models.Identity;

namespace IdentityDatabase.Dto
{
    public class UserDto
    {
        public int Id { get; set; }

        public string UserName { get; set; }

        public string Email { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string ProviderData { get; set; }

        public int? DepartmentId { get; set; }

        public bool IsLead { get; set; }

        public AuthorizationType AuthorizationType { get; set; }

        public string DisplayName { get; set; }

        public List<ModulePermissoinDto> ModulePermissions { get; set; } = new List<ModulePermissoinDto>();

        public List<RoleDto> Roles { get; set; } = new List<RoleDto>();
    }
}
