namespace CleanAPIDemo.Domain.Interfaces;

public interface IDataSyncService
{
    Task<DataSyncResult> SyncDataAsync(CancellationToken cancellationToken = default);
}

public record DataSyncResult(bool Success, int RecordsSynced, string? ErrorMessage = null);
