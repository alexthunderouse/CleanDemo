using CleanAPIDemo.Application.Common.Mappings;
using CleanAPIDemo.Application.Products.Commands.V1.CreateProduct;
using CleanAPIDemo.Domain.Entities;
using CleanAPIDemo.Domain.Interfaces;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace CleanAPIDemo.Application.Tests.Products.Commands;

public class CreateProductCommandHandlerTests
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IProductRepository _productRepository;
    private readonly ProductMapper _mapper;
    private readonly CreateProductCommandHandler _handler;

    public CreateProductCommandHandlerTests()
    {
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _productRepository = Substitute.For<IProductRepository>();
        _mapper = new ProductMapper();

        _unitOfWork.Products.Returns(_productRepository);
        _handler = new CreateProductCommandHandler(_unitOfWork, _mapper);
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldCreateProductAndReturnDto()
    {
        // Arrange
        var command = new CreateProductCommand(
            Name: "Test Product",
            Description: "Test Description",
            Price: 99.99m);

        _productRepository
            .AddAsync(Arg.Any<Product>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => callInfo.Arg<Product>());

        _unitOfWork
            .SaveChangesAsync(Arg.Any<CancellationToken>())
            .Returns(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be(command.Name);
        result.Description.Should().Be(command.Description);
        result.Price.Should().Be(command.Price);
        result.Id.Should().NotBeEmpty();

        await _productRepository.Received(1).AddAsync(
            Arg.Is<Product>(p =>
                p.Name == command.Name &&
                p.Description == command.Description &&
                p.Price == command.Price),
            Arg.Any<CancellationToken>());

        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldSetCreatedAtToUtcNow()
    {
        // Arrange
        var command = new CreateProductCommand(
            Name: "Test Product",
            Description: "Test Description",
            Price: 49.99m);

        var beforeTest = DateTime.UtcNow;

        _productRepository
            .AddAsync(Arg.Any<Product>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => callInfo.Arg<Product>());

        _unitOfWork
            .SaveChangesAsync(Arg.Any<CancellationToken>())
            .Returns(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        var afterTest = DateTime.UtcNow;
        result.CreatedAt.Should().BeOnOrAfter(beforeTest);
        result.CreatedAt.Should().BeOnOrBefore(afterTest);
    }
}
