namespace CleanAPIDemo.Application.DataSync.DTOs;

public record SyncDataResultDto(bool Success, int RecordsSynced, string? ErrorMessage = null);
