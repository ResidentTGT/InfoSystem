
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Company.Common;
using System;
using Company.Common.Models.Identity;
using SectionPermission = Company.Common.Models.Users.SectionPermission;
using ResourcePermission = Company.Common.Models.Users.ResourcePermission;

namespace IdentityDatabase
{
    public class IdentityContext : IdentityDbContext<User, Role, int>
    {
        public IdentityContext(DbContextOptions<IdentityContext> options)
         : base(options)
        {
        }

        public DbSet<ExternalApplication> ExternalApplications { get; set; }

        public DbSet<UserRole> UserRoles { get; set; }

        public DbSet<ExternalApplicationRole> ExternalApplicationRoles { get; set; }

        public DbSet<Department> Departments { get; set; }

        public DbSet<SectionPermission> SectionPermissions { get; set; }

        public DbSet<ResourcePermission> ResourcePermissions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<UserRole>()
                .HasKey(ur => new { ur.UserId, ur.RoleId });

            modelBuilder.Entity<UserRole>()
                .HasOne(ur => ur.User)
                .WithMany(u => u.UserRoles)
                .HasForeignKey(ur => ur.UserId);

            modelBuilder.Entity<UserRole>()
                .HasOne(ur => ur.Role)
                .WithMany(r => r.UserRoles)
                .HasForeignKey(ur => ur.RoleId);

            modelBuilder.Entity<SectionPermission>()
                .HasOne(ur => ur.Role)
                .WithMany(r => r.SectionPermissions)
                .HasForeignKey(ur => ur.RoleId);

            modelBuilder.Entity<ResourcePermission>()
                .HasOne(ur => ur.Role)
                .WithMany(r => r.ResourcePermissions)
                .HasForeignKey(ur => ur.RoleId);

            modelBuilder.Entity<ExternalApplicationRole>()
                .HasKey(ear => new { ear.ExternalApplicationId, ear.RoleId });

            modelBuilder.Entity<ExternalApplicationRole>()
                .HasOne(ear => ear.ExternalApplication)
                .WithMany(e => e.ExternalApplicationRoles)
                .HasForeignKey(ear => ear.ExternalApplicationId);

            modelBuilder.Entity<ExternalApplicationRole>()
                .HasOne(ear => ear.Role)
                .WithMany(r => r.ExternalApplicationRoles)
                .HasForeignKey(ear => ear.RoleId);

            modelBuilder.Entity<Department>()
                .HasMany(d => d.SubDepartments)
                .WithOne(d => d.ParentDepartment)
                .HasForeignKey(d => d.ParentId);

            modelBuilder.Entity<Department>()
               .Property(d => d.Name)
               .IsRequired();

            modelBuilder.Entity<Department>()
               .Property(d => d.CreateTime)
               .IsRequired();

            modelBuilder.Entity<Department>()
                .HasIndex(d => new { d.Name, d.ParentId, d.DeleteTime })
                .IsUnique();

            /*modelBuilder.Entity<User>()
                .HasOne(u => u.Department)
                .WithMany(d => d.Users)
                .HasForeignKey(u => u.DepartmentId);*/

            modelBuilder.Entity<Department>()
                .HasMany(d => d.Users)
                .WithOne(u => u.Department)
                .HasForeignKey(d => d.DepartmentId);
        }
    }
}
