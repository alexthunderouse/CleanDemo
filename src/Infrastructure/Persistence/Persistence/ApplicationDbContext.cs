using CleanAPIDemo.Domain.Entities;
using CleanAPIDemo.Domain.Entities.Views;
using Microsoft.EntityFrameworkCore;

namespace CleanAPIDemo.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductSummaryView> ProductSummaryView => Set<ProductSummaryView>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        // Configure the view as keyless entity
        modelBuilder.Entity<ProductSummaryView>(entity =>
        {
            entity.HasNoKey();
            entity.ToView("vw_ProductSummary");
        });

        base.OnModelCreating(modelBuilder);
    }
}
