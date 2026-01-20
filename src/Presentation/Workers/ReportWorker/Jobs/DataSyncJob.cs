using CleanAPIDemo.Application.Features.DataSync.Commands.SyncData;
using Coravel.Invocable;
using MediatR;
using Polly.Registry;

namespace CleanAPIDemo.Worker.Jobs;

public class DataSyncJob : IInvocable
{
    private readonly IMediator _mediator;
    private readonly ResiliencePipelineProvider<string> _pipelineProvider;
    private readonly ILogger<DataSyncJob> _logger;

    public DataSyncJob(
        IMediator mediator,
        ResiliencePipelineProvider<string> pipelineProvider,
        ILogger<DataSyncJob> logger)
    {
        _mediator = mediator;
        _pipelineProvider = pipelineProvider;
        _logger = logger;
    }

    public async Task Invoke()
    {
        _logger.LogInformation("DataSyncJob started");

        try
        {
            var pipeline = _pipelineProvider.GetPipeline("job-retry");

            var result = await pipeline.ExecuteAsync(async cancellationToken =>
            {
                return await _mediator.Send(new SyncDataCommand(), cancellationToken);
            });

            if (result.Success)
            {
                _logger.LogInformation("DataSyncJob completed successfully. Records synced: {RecordCount}", result.RecordsSynced);
            }
            else
            {
                _logger.LogWarning("DataSyncJob completed with errors: {ErrorMessage}", result.ErrorMessage);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "DataSyncJob failed after all retry attempts");
            throw;
        }
    }
}
