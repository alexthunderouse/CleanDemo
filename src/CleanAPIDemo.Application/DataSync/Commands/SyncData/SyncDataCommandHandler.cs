using CleanAPIDemo.Domain.Interfaces;
using MediatR;

namespace CleanAPIDemo.Application.DataSync.Commands.SyncData;

public class SyncDataCommandHandler : IRequestHandler<SyncDataCommand, DataSyncResult>
{
    private readonly IDataSyncService _dataSyncService;

    public SyncDataCommandHandler(IDataSyncService dataSyncService)
    {
        _dataSyncService = dataSyncService;
    }

    public async Task<DataSyncResult> Handle(SyncDataCommand request, CancellationToken cancellationToken)
    {
        return await _dataSyncService.SyncDataAsync(cancellationToken);
    }
}
