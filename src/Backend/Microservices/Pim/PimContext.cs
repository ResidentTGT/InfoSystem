using Microsoft.EntityFrameworkCore;
using Company.Common.Models.Identity;
using Company.Common.Models.Pim;

namespace Company.Pim
{
    public class PimContext : DbContext, IPermissionDbContext
    {
        public PimContext(DbContextOptions<PimContext> options)
            : base(options)
        {
        }

        public DbSet<Attribute> Attributes { get; set; }

        public DbSet<AttributeCategory> AttributeCategories { get; set; }

        public DbSet<AttributeGroup> AttributeGroups { get; set; }

        public DbSet<AttributeValue> AttributeValues { get; set; }

        public DbSet<Category> Categories { get; set; }

        public DbSet<List> Lists { get; set; }

        public DbSet<ListValue> ListValues { get; set; }

        public DbSet<Product> Products { get; set; }

        public DbSet<ProductSearch> ProductSearch { get; set; }

        public DbSet<ProductFile> ProductFiles { get; set; }

        public DbSet<SectionPermission> SectionPermissions { get; set; }

        public DbSet<ResourcePermission> ResourcePermissions { get; set; }

        public DbSet<AttributePermission> AttributePermissions { get; set; }

        public DbSet<ListMetadata> ListMetadatas { get; set; }

        public DbSet<ListValueMetadata> ListValueMetadatas { get; set; }

        public DbSet<ListMetadataPermission> ListMetadataPermissions { get; set; }

        public DbSet<Import> Imports { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<ListMetadata>()
                .HasOne(lm => lm.List)
                .WithMany(l => l.ListMetadatas)
                .HasForeignKey(lm => lm.ListId);

            builder.Entity<ListMetadata>()
                .Property(lm => lm.Name)
                .IsRequired();

            builder.Entity<ListMetadata>()
                .Property(lm => lm.ListId)
                .IsRequired();

            builder.Entity<ListValueMetadata>()
                .HasOne(lvm => lvm.ListValue)
                .WithMany(lv => lv.ListValueMetadatas)
                .HasForeignKey(lvm => lvm.ListValueId);

            builder.Entity<ListValueMetadata>()
                .HasOne(lvm => lvm.ListMetadata)
                .WithMany(lm => lm.ListValueMetadatas)
                .HasForeignKey(lvm => lvm.ListMetadataId);

            builder.Entity<ListValueMetadata>()
                .Property(lvm => lvm.ListValueId)
                .IsRequired();

            builder.Entity<ListValueMetadata>()
                .Property(lvm => lvm.ListMetadataId)
                .IsRequired();

            builder.Entity<ListMetadataPermission>()
                .HasOne(lmp => lmp.ListMetadata)
                .WithMany(lm => lm.ListMetadataPermissions)
                .HasForeignKey(lmp => lmp.ListMetadataId);

            builder.Entity<ListMetadataPermission>()
                .HasIndex(lmp => new { lmp.ListMetadataId, lmp.RoleId })
                .IsUnique();

            builder.Entity<AttributePermission>()
                .HasOne(ap => ap.Attribute)
                .WithMany(a => a.AttributePermissions)
                .HasForeignKey(ap => ap.AttributeId);

            builder.Entity<AttributePermission>()
                .HasIndex(ap => new { ap.AttributeId, ap.RoleId })
                .IsUnique();

            builder.Entity<Category>()
                .HasMany(c => c.SubCategories)
                .WithOne(c => c.ParentCategory)
                .HasForeignKey(c => c.ParentId);

            builder.Entity<Category>()
                .Property(c => c.Name)
                .IsRequired();

            builder.Entity<Category>()
                .Property(c => c.CreateTime)
                .IsRequired();

            builder.Entity<Category>()
                .HasIndex(c => new { c.Name, c.ParentId, c.DeleteTime })
                .IsUnique();

            builder.Entity<Attribute>()
                .HasOne(a => a.AttributeGroup)
                .WithMany(ag => ag.Attributes)
                .HasForeignKey(a => a.GroupId);

            builder.Entity<Attribute>()
                .HasOne(a => a.List)
                .WithMany(l => l.Attributes)
                .HasForeignKey(a => a.ListId);

            builder.Entity<Attribute>()
                .Property(a => a.CreateTime)
                .IsRequired();

            builder.Entity<Attribute>()
                .Property(a => a.Name)
                .IsRequired();

            builder.Entity<Attribute>()
                .Property(a => a.CreateTime)
                .IsRequired();

            builder.Entity<Attribute>()
                .HasIndex(a => new { a.Name, a.DeleteTime })
                .IsUnique();

            builder.Entity<AttributeCategory>()
                .HasKey(ac => new { ac.AttributeId, ac.CategoryId });

            builder.Entity<AttributeCategory>()
                .HasOne(ac => ac.Attribute)
                .WithMany(a => a.AttributeCategories)
                .HasForeignKey(ac => ac.AttributeId);

            builder.Entity<AttributeCategory>()
                .HasOne(ac => ac.Category)
                .WithMany(c => c.AttributeCategories)
                .HasForeignKey(ac => ac.CategoryId);

            builder.Entity<AttributeValue>()
                .HasOne(av => av.Attribute)
                .WithMany(a => a.AttributeValues)
                .HasForeignKey(av => av.AttributeId);

            builder.Entity<AttributeValue>()
                .HasOne(av => av.Product)
                .WithMany(p => p.AttributeValues)
                .HasForeignKey(av => av.ProductId);

            builder.Entity<AttributeValue>()
                .HasOne(av => av.ListValue)
                .WithMany(lv => lv.AttributeValues)
                .HasForeignKey(av => av.ListValueId);

            builder.Entity<AttributeValue>()
                .Property(a => a.AttributeId)
                .IsRequired();

            builder.Entity<AttributeValue>()
                .Property(a => a.ProductId)
                .IsRequired();

            builder.Entity<AttributeValue>()
                .Property(a => a.CreateTime)
                .IsRequired();

            builder.Entity<AttributeValue>()
                .HasIndex(p => p.SearchVector)
                .ForNpgsqlHasMethod("GIN");

            builder.Entity<AttributeGroup>()
                .Property(ag => ag.Name)
                .IsRequired();

            builder.Entity<AttributeGroup>()
                .Property(a => a.CreateTime)
                .IsRequired();

            builder.Entity<AttributeGroup>()
                .HasIndex(ag => ag.Name)
                .IsUnique();

            builder.Entity<ListValue>()
                .HasOne(lv => lv.List)
                .WithMany(l => l.ListValues)
                .HasForeignKey(lv => lv.ListId);

            builder.Entity<ListValue>()
                .Property(lv => lv.Value)
                .IsRequired();

            builder.Entity<ListValue>()
                .Property(lv => lv.ListId)
                .IsRequired();

            builder.Entity<List>()
                .Property(l => l.Name)
                .IsRequired();

            builder.Entity<Product>()
                .HasOne(p => p.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(p => p.CategoryId);

            builder.Entity<Product>()
                .HasMany(p => p.SubProducts)
                .WithOne(p => p.ParentProduct)
                .HasForeignKey(p => p.ParentId);

            builder.Entity<Product>()
                .Property(p => p.Name)
                .IsRequired();

            builder.Entity<Product>()
                .Property(p => p.Sku)
                .IsRequired();

            builder.Entity<Product>()
                .HasIndex(p => p.Sku)
                .IsUnique();

            builder.Entity<Product>()
                .Property(p => p.CreateTime)
                .IsRequired();

            builder.Entity<Product>()
                .HasOne(p => p.Import)
                .WithMany(c => c.Products)
                .HasForeignKey(p => p.ImportId);

            builder.Entity<Product>()
                .HasOne(p => p.ProductSearch)
                .WithOne(p => p.Product)
                .HasForeignKey<ProductSearch>(p => p.ProductId);

            builder.Entity<ProductFile>()
                .HasOne(pf => pf.Product)
                .WithMany(p => p.ProductFiles)
                .HasForeignKey(pf => pf.ProductId);

            builder.Entity<ProductFile>()
                .HasKey(pf => new { pf.FileId, pf.ProductId });

            builder.Entity<ProductSearch>()
                .HasOne(p => p.ParentProductSearch)
                .WithMany(p => p.SubProductSearch)
                .HasForeignKey(p => p.ParentId);

            builder.Entity<ProductSearch>()
                .Property(ps => ps.BwpMin)
                .HasDefaultValue(0);
        }
    }
}