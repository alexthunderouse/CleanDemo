using CleanAPIDemo.Application.DataSync.Commands.SyncData;
using CleanAPIDemo.Domain.Entities;
using CleanAPIDemo.Domain.Interfaces;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace CleanAPIDemo.Worker.Tests.DataSync.Commands;

public class SyncDataCommandHandlerTests
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IProductRepository _productRepository;
    private readonly ILogger<SyncDataCommandHandler> _logger;
    private readonly SyncDataCommandHandler _handler;

    public SyncDataCommandHandlerTests()
    {
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _productRepository = Substitute.For<IProductRepository>();
        _logger = Substitute.For<ILogger<SyncDataCommandHandler>>();

        _unitOfWork.Products.Returns(_productRepository);
        _handler = new SyncDataCommandHandler(_unitOfWork, _logger);
    }

    [Fact]
    public async Task Handle_NoProductsToSync_ShouldReturnSuccessWithZeroRecords()
    {
        // Arrange
        _productRepository
            .GetProductsForSyncAsync(Arg.Any<DateTime>(), Arg.Any<CancellationToken>())
            .Returns(Enumerable.Empty<Product>());

        // Act
        var result = await _handler.Handle(new SyncDataCommand(), CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.RecordsSynced.Should().Be(0);
        result.ErrorMessage.Should().BeNull();

        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ProductsToSync_ShouldUpdateAndSaveChanges()
    {
        // Arrange
        var products = new List<Product>
        {
            new() { Id = Guid.NewGuid(), Name = "Product 1", Description = "Desc 1", Price = 5.00m },
            new() { Id = Guid.NewGuid(), Name = "Product 2", Description = "Desc 2", Price = 15.00m }
        };

        _productRepository
            .GetProductsForSyncAsync(Arg.Any<DateTime>(), Arg.Any<CancellationToken>())
            .Returns(products);

        _unitOfWork
            .SaveChangesAsync(Arg.Any<CancellationToken>())
            .Returns(2);

        // Act
        var result = await _handler.Handle(new SyncDataCommand(), CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.RecordsSynced.Should().Be(2);
        result.ErrorMessage.Should().BeNull();

        await _productRepository.Received(2).UpdateAsync(Arg.Any<Product>(), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ProductWithLowPrice_ShouldApplyPriceAdjustment()
    {
        // Arrange
        var product = new Product
        {
            Id = Guid.NewGuid(),
            Name = "Low Price Product",
            Description = "Test",
            Price = 5.00m
        };

        _productRepository
            .GetProductsForSyncAsync(Arg.Any<DateTime>(), Arg.Any<CancellationToken>())
            .Returns(new List<Product> { product });

        _unitOfWork
            .SaveChangesAsync(Arg.Any<CancellationToken>())
            .Returns(1);

        // Act
        await _handler.Handle(new SyncDataCommand(), CancellationToken.None);

        // Assert
        product.Price.Should().Be(5.25m); // 5.00 * 1.05 = 5.25
        product.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_ProductWithHighPrice_ShouldNotApplyPriceAdjustment()
    {
        // Arrange
        var originalPrice = 15.00m;
        var product = new Product
        {
            Id = Guid.NewGuid(),
            Name = "High Price Product",
            Description = "Test",
            Price = originalPrice
        };

        _productRepository
            .GetProductsForSyncAsync(Arg.Any<DateTime>(), Arg.Any<CancellationToken>())
            .Returns(new List<Product> { product });

        _unitOfWork
            .SaveChangesAsync(Arg.Any<CancellationToken>())
            .Returns(1);

        // Act
        await _handler.Handle(new SyncDataCommand(), CancellationToken.None);

        // Assert
        product.Price.Should().Be(originalPrice);
        product.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_ProductWithWhitespaceDescription_ShouldTrimDescription()
    {
        // Arrange
        var product = new Product
        {
            Id = Guid.NewGuid(),
            Name = "Test Product",
            Description = "  Test Description  ",
            Price = 20.00m
        };

        _productRepository
            .GetProductsForSyncAsync(Arg.Any<DateTime>(), Arg.Any<CancellationToken>())
            .Returns(new List<Product> { product });

        _unitOfWork
            .SaveChangesAsync(Arg.Any<CancellationToken>())
            .Returns(1);

        // Act
        await _handler.Handle(new SyncDataCommand(), CancellationToken.None);

        // Assert
        product.Description.Should().Be("Test Description");
    }

    [Fact]
    public async Task Handle_ShouldSetUpdatedAtToUtcNow()
    {
        // Arrange
        var product = new Product
        {
            Id = Guid.NewGuid(),
            Name = "Test Product",
            Description = "Test",
            Price = 10.00m,
            UpdatedAt = null
        };

        _productRepository
            .GetProductsForSyncAsync(Arg.Any<DateTime>(), Arg.Any<CancellationToken>())
            .Returns(new List<Product> { product });

        _unitOfWork
            .SaveChangesAsync(Arg.Any<CancellationToken>())
            .Returns(1);

        var beforeTest = DateTime.UtcNow;

        // Act
        await _handler.Handle(new SyncDataCommand(), CancellationToken.None);

        // Assert
        var afterTest = DateTime.UtcNow;
        product.UpdatedAt.Should().NotBeNull();
        product.UpdatedAt.Should().BeOnOrAfter(beforeTest);
        product.UpdatedAt.Should().BeOnOrBefore(afterTest);
    }

    [Fact]
    public async Task Handle_RepositoryThrowsException_ShouldReturnFailure()
    {
        // Arrange
        var exceptionMessage = "Database connection failed";
        _productRepository
            .GetProductsForSyncAsync(Arg.Any<DateTime>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException(exceptionMessage));

        // Act
        var result = await _handler.Handle(new SyncDataCommand(), CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.RecordsSynced.Should().Be(0);
        result.ErrorMessage.Should().Be(exceptionMessage);
    }

    [Fact]
    public async Task Handle_SaveChangesThrowsException_ShouldReturnFailure()
    {
        // Arrange
        var products = new List<Product>
        {
            new() { Id = Guid.NewGuid(), Name = "Product 1", Description = "Desc", Price = 5.00m }
        };

        _productRepository
            .GetProductsForSyncAsync(Arg.Any<DateTime>(), Arg.Any<CancellationToken>())
            .Returns(products);

        var exceptionMessage = "Save failed";
        _unitOfWork
            .SaveChangesAsync(Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException(exceptionMessage));

        // Act
        var result = await _handler.Handle(new SyncDataCommand(), CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.RecordsSynced.Should().Be(0);
        result.ErrorMessage.Should().Be(exceptionMessage);
    }

    [Fact]
    public async Task Handle_CancellationRequested_ShouldPassTokenToRepository()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        var token = cts.Token;

        _productRepository
            .GetProductsForSyncAsync(Arg.Any<DateTime>(), Arg.Any<CancellationToken>())
            .Returns(Enumerable.Empty<Product>());

        // Act
        await _handler.Handle(new SyncDataCommand(), token);

        // Assert
        await _productRepository.Received(1).GetProductsForSyncAsync(Arg.Any<DateTime>(), token);
    }
}
