using Microsoft.EntityFrameworkCore;
using Company.Common.Models.Identity;

namespace Company.Calculator
{
    public class CalculatorContext : DbContext, IPermissionDbContext
    {
        public CalculatorContext(DbContextOptions<CalculatorContext> options)
            : base(options)
        {

        }

        public DbSet<SectionPermission> SectionPermissions { get; set; }

        public DbSet<ResourcePermission> ResourcePermissions { get; set; }
    }
}
