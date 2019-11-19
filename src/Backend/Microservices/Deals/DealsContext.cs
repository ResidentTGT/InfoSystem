using Microsoft.EntityFrameworkCore;
using Company.Common.Models.Deals;
using Company.Common.Models.Identity;

namespace Company.Deals
{

    public class DealsContext : DbContext, IPermissionDbContext
    {
        public DealsContext(DbContextOptions<DealsContext> options)
            : base(options)
        {

        }

        public DbSet<Deal> Deals { get; set; }

        public DbSet<DealProduct> DealProducts { get; set; }

        public DbSet<DiscountParams> DiscountParams { get; set; }

        public DbSet<HeadDiscountRequest> HeadDiscountRequests { get; set; }

        public DbSet<SectionPermission> SectionPermissions { get; set; }

        public DbSet<ResourcePermission> ResourcePermissions { get; set; }

        public DbSet<DealFile> DealFiles { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<DiscountParams>()
                .HasOne(dp => dp.Deal)
                .WithMany(d => d.DiscountParams)
                .HasForeignKey(dp => dp.DealId);

            builder.Entity<HeadDiscountRequest>()
                .HasOne(hdr => hdr.Deal)
                .WithMany(d => d.HeadDiscountRequests)
                .HasForeignKey(hdr => hdr.DealId);

            builder.Entity<DealProduct>()
                .HasKey(dp => new { dp.DealId, dp.ProductId });

            builder.Entity<DealProduct>()
                .HasOne(dp => dp.Deal)
                .WithMany(d => d.DealProducts)
                .HasForeignKey(dp => dp.DealId);

            builder.Entity<DealFile>()
                .HasOne(df => df.Deal)
                .WithMany(d => d.DealFiles)
                .HasForeignKey(df => df.DealId);

            builder.Entity<DealFile>()
                .HasKey(df => new { df.FileId, df.DealId });
        }
    }
}
