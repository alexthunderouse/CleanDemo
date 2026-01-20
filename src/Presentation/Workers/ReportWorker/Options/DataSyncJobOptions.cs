namespace CleanAPIDemo.Worker.Options;

public class DataSyncJobOptions
{
    public const string SectionName = "DataSyncJob";

    public int IntervalMinutes { get; set; } = 10;
    public string? ApiKey { get; set; }
    public string? SourceEndpoint { get; set; }
    public int MaxRetryAttempts { get; set; } = 3;
    public int RetryDelaySeconds { get; set; } = 2;
}
