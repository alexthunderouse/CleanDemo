using System.Diagnostics;
using CleanAPIDemo.Domain.Interfaces;
using CleanAPIDemo.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CleanAPIDemo.Infrastructure.Services;

public class DataSyncService : IDataSyncService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<DataSyncService> _logger;
    private static readonly ActivitySource ActivitySource = new("CleanAPIDemo.Worker");

    public DataSyncService(
        ApplicationDbContext dbContext,
        ILogger<DataSyncService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<DataSyncResult> SyncDataAsync(CancellationToken cancellationToken = default)
    {
        using var activity = ActivitySource.StartActivity("DataSync");

        _logger.LogInformation("Starting data synchronization");

        try
        {
            // Example: Sync products that need updating
            var productsToSync = await _dbContext.Products
                .Where(p => p.UpdatedAt == null || p.UpdatedAt < DateTime.UtcNow.AddHours(-1))
                .ToListAsync(cancellationToken);

            var recordsProcessed = 0;

            foreach (var product in productsToSync)
            {
                // Simulate sync logic - in real scenario, fetch from external API
                product.UpdatedAt = DateTime.UtcNow;
                recordsProcessed++;
            }

            if (recordsProcessed > 0)
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
            }

            _logger.LogInformation("Successfully synchronized {RecordCount} records", recordsProcessed);
            activity?.SetTag("records.synced", recordsProcessed);

            return new DataSyncResult(true, recordsProcessed);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Data synchronization failed");
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);

            return new DataSyncResult(false, 0, ex.Message);
        }
    }
}
