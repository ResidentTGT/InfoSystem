using Microsoft.EntityFrameworkCore;
using Company.Common.Models.FileStorage;

namespace Company.FileStorage
{
    public class FileStorageContext : DbContext
    {
        public FileStorageContext(DbContextOptions<FileStorageContext> options)
            : base(options)
        {
        }

        public FileStorageContext()
        {

        }

        public DbSet<File> Files { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder
                .Entity<File>()
                .Property(f => f.Name)
                .IsRequired();

            modelBuilder
                .Entity<File>()
                .Property(f => f.UserId)
                .IsRequired();
        }
    }
}
