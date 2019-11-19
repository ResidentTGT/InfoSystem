using Microsoft.EntityFrameworkCore;
using Company.Common.Models.Crm;

namespace Company.Crm
{
    public class CrmContext : DbContext
    {
        public CrmContext(DbContextOptions<CrmContext> options)
            : base(options)
        {
        }

        public DbSet<Partner> Partners { get; set; }
        public DbSet<Person> Persons { get; set; }
        public DbSet<TeamRolePeople> TeamRolePeoples { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            
            modelBuilder.Entity<TeamRolePeople>()
                .HasKey(t => new {t.PartnerId, t.PersonId});

            modelBuilder.Entity<TeamRolePeople>()
                .HasOne(t => t.Person)
                .WithMany(p => p.TeamRolePeoples)
                .HasForeignKey(t => t.PersonId);

            modelBuilder.Entity<TeamRolePeople>()
                .HasOne(t => t.Partner)
                .WithMany(p => p.TeamRolePeoples)
                .HasForeignKey(t => t.PartnerId);
        }
    }
}