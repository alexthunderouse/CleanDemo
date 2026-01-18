using CleanAPIDemo.Domain.Interfaces;
using MediatR;

namespace CleanAPIDemo.Application.DataSync.Commands.SyncData;

public record SyncDataCommand : IRequest<DataSyncResult>;
