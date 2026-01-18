using CleanAPIDemo.Application.DataSync.DTOs;
using MediatR;

namespace CleanAPIDemo.Application.DataSync.Commands.SyncData;

public record SyncDataCommand : IRequest<SyncDataResultDto>;
