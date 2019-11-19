using Microsoft.EntityFrameworkCore;

namespace Company.Common.Models.Identity
{
    public interface IPermissionDbContext
    {
        DbSet<SectionPermission> SectionPermissions { get; }
        DbSet<ResourcePermission> ResourcePermissions { get; }
    }
}
