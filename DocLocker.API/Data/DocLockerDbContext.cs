using Microsoft.EntityFrameworkCore;
using DocLocker.Core.Models;

namespace DocLocker.API.Data
{
    public class DocLockerDbContext : DbContext
    {
        public DocLockerDbContext(DbContextOptions<DocLockerDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Make Email unique
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();
        }
    }
}