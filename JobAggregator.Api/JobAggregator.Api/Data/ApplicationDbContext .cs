using JobAggregator.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace JobAggregator.Api.Data
{
    public class ApplicationDbContext : DbContext
    {
        public DbSet<JobPosting> JobPostings { get; set; }
        public DbSet<Portal> Portals { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<JobPosting>()
                .HasIndex(j => j.HashCode)
                .IsUnique();

            modelBuilder.Entity<JobPosting>()
                .HasOne(j => j.Portal)
                .WithMany(p => p.JobPostings)
                .HasForeignKey(j => j.PortalID);
        }
    }
}
