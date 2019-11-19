using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace Company.Common.Models.Identity
{
    public class Role : IdentityRole<int>
    {
        public bool ICmpelongsToUser { get; set; }
        public Module Module { get; set; }

        public virtual ICollection<UserRole> UserRoles { get; set; } 

        public virtual ICollection<ExternalApplicationRole> ExternalApplicationRoles { get; set; }

        public virtual ICollection<Company.Common.Models.Users.SectionPermission> SectionPermissions { get; set; }

        public virtual ICollection<Company.Common.Models.Users.ResourcePermission> ResourcePermissions { get; set; }
    }

    public enum Module
    {
        PIM = 1,
        Administration = 2,
        B2B = 3,
        Calculator = 4,
        Seasons = 5,
        Deals = 6
    }
}
