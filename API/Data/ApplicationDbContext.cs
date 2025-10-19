using Microsoft.EntityFrameworkCore;
using API.Models;

namespace API.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {

    }
    public DbSet<Entity> Entities { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Entity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Code).IsUnique();
            entity.HasIndex(e => e.Name);
            entity.HasIndex(e => e.Category);
            entity.HasIndex(e => e.Status);

            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(e => e.Code)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
        });

        // Seed data
        SeedData(modelBuilder);
    }

    private static void SeedData(ModelBuilder modelBuilder)
    {
        // Seed Entities
        modelBuilder.Entity<Entity>().HasData(
            new Entity
            {
                Id = 1,
                Name = "Sample Organization",
                Code = "ORG-001",
                Description = "A sample organization entity for demonstration",
                Status = "Active",
                Priority = 1,
                Category = "Organization",
                Tags = "sample,demo,organization",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsActive = true,
                CreatedBy = "System"
            },
            new Entity
            {
                Id = 2,
                Name = "Sample Department",
                Code = "DEPT-001",
                Description = "A sample department entity",
                Status = "Active",
                Priority = 2,
                Category = "Department",
                Tags = "sample,demo,department",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsActive = true,
                CreatedBy = "System"
            },
            new Entity
            {
                Id = 3,
                Name = "Sample Project",
                Code = "PROJ-001",
                Description = "A sample project entity with different properties",
                Status = "In Progress",
                Priority = 3,
                Price = 1500.00m,
                Quantity = 5,
                Percentage = 75.50m,
                Category = "Project",
                Tags = "sample,demo,project",
                StartDate = DateTime.UtcNow.AddDays(-30),
                DueDate = DateTime.UtcNow.AddDays(30),
                IsFeatured = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsActive = true,
                CreatedBy = "Admin"
            }
        );
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var entityEntries = ChangeTracker
            .Entries()
            .Where(e => e.Entity is Entity && e.State == EntityState.Modified);

        foreach (var entityEntry in entityEntries)
        {
            ((Entity)entityEntry.Entity).UpdatedAt = DateTime.UtcNow;
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}