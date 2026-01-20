using CleanAPIDemo.Application.Features.DataSync.DTOs;
using MediatR;

namespace CleanAPIDemo.Application.Features.DataSync.Commands.SyncData;

public record SyncDataCommand : IRequest<SyncDataResultDto>;
