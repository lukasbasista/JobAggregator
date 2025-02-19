using JobAggregator.Api.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace JobAggregator.Api.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public DbSet<JobPosting> JobPostings { get; set; }
        public DbSet<Portal> Portals { get; set; }
        public DbSet<Company> Companies { get; set; }
        private readonly ILogger<ApplicationDbContext> _logger;

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, ILogger<ApplicationDbContext> logger)
            : base(options)
        {
            _logger = logger;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<JobPosting>()
                .HasIndex(j => j.HashCode)
                .IsUnique();

            modelBuilder.Entity<JobPosting>()
                .HasIndex(j => j.ExternalID)
                .IsUnique();

            modelBuilder.Entity<JobPosting>()
                .HasOne(j => j.Portal)
                .WithMany(p => p.JobPostings)
                .HasForeignKey(j => j.PortalID);
        }
    }
}
