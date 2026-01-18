using CleanAPIDemo.Application.DataSync.Commands.SyncData;
using CleanAPIDemo.Application.DataSync.DTOs;
using CleanAPIDemo.Worker.Jobs;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Polly;
using Polly.Registry;
using Xunit;

namespace CleanAPIDemo.Worker.Tests.Jobs;

public class DataSyncJobTests
{
    private readonly IMediator _mediator;
    private readonly ResiliencePipelineProvider<string> _pipelineProvider;
    private readonly ILogger<DataSyncJob> _logger;
    private readonly DataSyncJob _job;

    public DataSyncJobTests()
    {
        _mediator = Substitute.For<IMediator>();
        _pipelineProvider = Substitute.For<ResiliencePipelineProvider<string>>();
        _logger = Substitute.For<ILogger<DataSyncJob>>();

        // Setup a pass-through pipeline (no retry for testing)
        var pipeline = ResiliencePipeline.Empty;
        _pipelineProvider.GetPipeline("job-retry").Returns(pipeline);

        _job = new DataSyncJob(_mediator, _pipelineProvider, _logger);
    }

    [Fact]
    public async Task Invoke_SuccessfulSync_ShouldSendCommandViaMediatR()
    {
        // Arrange
        var expectedResult = new SyncDataResultDto(true, 10);
        _mediator
            .Send(Arg.Any<SyncDataCommand>(), Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        // Act
        await _job.Invoke();

        // Assert
        await _mediator.Received(1).Send(Arg.Any<SyncDataCommand>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Invoke_SuccessfulSync_ShouldNotThrowException()
    {
        // Arrange
        var expectedResult = new SyncDataResultDto(true, 5);
        _mediator
            .Send(Arg.Any<SyncDataCommand>(), Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        // Act
        var act = async () => await _job.Invoke();

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task Invoke_SyncWithErrors_ShouldNotThrowException()
    {
        // Arrange
        var expectedResult = new SyncDataResultDto(false, 0, "Some error occurred");
        _mediator
            .Send(Arg.Any<SyncDataCommand>(), Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        // Act
        var act = async () => await _job.Invoke();

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task Invoke_MediatorThrowsException_ShouldRethrowException()
    {
        // Arrange
        var expectedException = new InvalidOperationException("MediatR failed");
        _mediator
            .Send(Arg.Any<SyncDataCommand>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(expectedException);

        // Act
        var act = async () => await _job.Invoke();

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("MediatR failed");
    }

    [Fact]
    public async Task Invoke_ShouldUseResiliencePipeline()
    {
        // Arrange
        var expectedResult = new SyncDataResultDto(true, 3);
        _mediator
            .Send(Arg.Any<SyncDataCommand>(), Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        // Act
        await _job.Invoke();

        // Assert
        _pipelineProvider.Received(1).GetPipeline("job-retry");
    }

    [Fact]
    public async Task Invoke_ZeroRecordsSynced_ShouldCompleteSuccessfully()
    {
        // Arrange
        var expectedResult = new SyncDataResultDto(true, 0);
        _mediator
            .Send(Arg.Any<SyncDataCommand>(), Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        // Act
        var act = async () => await _job.Invoke();

        // Assert
        await act.Should().NotThrowAsync();
    }
}
