using Microsoft.EntityFrameworkCore;
using Company.Common.Models.Identity;
using Company.Common.Models.Seasons;

namespace Company.Seasons
{
    public class SeasonsContext : DbContext, IPermissionDbContext
    {
        public SeasonsContext(DbContextOptions<SeasonsContext> options)
            : base(options)
        {

        }

        public DbSet<BrandPolicyData> BrandPolicyDatas { get; set; }

        public DbSet<DiscountPolicy> DiscountPolicies { get; set; }

        public DbSet<Logistic> Logistics { get; set; }

        public DbSet<PolicyData> PolicyDatas { get; set; }

        public DbSet<SalesPlanData> SalesPlanDatas { get; set; }

        public DbSet<Supply> Supplies { get; set; }

        public DbSet<SectionPermission> SectionPermissions { get; set; }

        public DbSet<ResourcePermission> ResourcePermissions { get; set; }

        public DbSet<ExchangeRates> ExchangeRates { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<BrandPolicyData>()
                .HasOne(b => b.DiscountPolicy)
                .WithMany(dp => dp.BrandPolicyDatas)
                .HasForeignKey(b => b.DiscountPolicyId);

            builder.Entity<SalesPlanData>()
                .HasOne(s => s.DiscountPolicy)
                .WithMany(dp => dp.SalesPlanDatas)
                .HasForeignKey(s => s.DiscountPolicyId);

            builder.Entity<PolicyData>()
                .HasOne(p => p.DiscountPolicy)
                .WithMany(dp => dp.PolicyDatas)
                .HasForeignKey(p => p.DiscountPolicyId);

            builder.Entity<Supply>()
                .HasOne(s => s.Logistic)
                .WithMany(l => l.Supplies)
                .HasForeignKey(s => s.LogisticId);

            builder.Entity<ExchangeRates>()
                .HasOne(p => p.DiscountPolicy)
                .WithMany(dp => dp.ExchangeRates)
                .HasForeignKey(p => p.DiscountPolicyId);
        }
    }
}
