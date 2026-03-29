using Microsoft.EntityFrameworkCore;
using RealEstateSync.Core.Models;

namespace RealEstateSync.Infra.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<SearchHistoryEntry> SearchHistoryEntries { get; set; } = null!;
        public DbSet<SearchConfig> SearchConfigs { get; set; } = null!;

        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            ConfigureSearchHistoryEntry(modelBuilder);
            ConfigureSearchConfig(modelBuilder);
        }

        private static void ConfigureSearchHistoryEntry(ModelBuilder modelBuilder)
        {
            var entity = modelBuilder.Entity<SearchHistoryEntry>();

            entity.ToTable("SearchHistory");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.FileName)
                .IsRequired()
                .HasMaxLength(255);

            entity.Property(x => x.SearchDate)
                .IsRequired();

            entity.Property(x => x.TotalItems)
                .IsRequired();

            entity.Property(x => x.FoundCount)
                .IsRequired();

            entity.Property(x => x.NotFoundCount)
                .IsRequired();

            entity.Property(x => x.ErrorCount)
                .IsRequired();

            entity.Property(x => x.Notes)
                .HasMaxLength(500);
        }

        private static void ConfigureSearchConfig(ModelBuilder modelBuilder)
        {
            var entity = modelBuilder.Entity<SearchConfig>();

            entity.ToTable("SearchConfigs");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.PortalName)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(x => x.BaseUrl)
                .IsRequired()
                .HasMaxLength(500);

            entity.Property(x => x.IsActive)
                .IsRequired();

            entity.Property(x => x.IsDefault)
                .IsRequired();

            entity.Property(x => x.CreatedAt)
                .IsRequired();

            entity.Property(x => x.UpdatedAt);
        }
    }
}