using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace Company.Common.Models.Identity
{
    public class User : IdentityUser<int>
    {
        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string ProviderData { get; set; }

        public int? DepartmentId { get; set; }

        public bool IsLead { get; set; }

        public AuthorizationType AuthorizationType { get; set; }

        public string DisplayName { get; set; }

        public virtual ICollection<UserRole> UserRoles { get; set; }

        public virtual Department Department { get; set; }
    }

    public enum AuthorizationType
    {
        Local = 0,

        Microsoft = 1
    }
}
