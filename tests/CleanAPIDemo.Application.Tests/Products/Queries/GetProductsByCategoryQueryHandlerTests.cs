using CleanAPIDemo.Application.Common.Mappings;
using CleanAPIDemo.Application.Products.Queries.V1.GetProductsByCategory;
using CleanAPIDemo.Domain.Entities.StoredProcedures;
using CleanAPIDemo.Domain.Interfaces;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace CleanAPIDemo.Application.Tests.Products.Queries;

public class GetProductsByCategoryQueryHandlerTests
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IProductProcedureRepository _procedureRepository;
    private readonly ProductMapper _mapper;
    private readonly GetProductsByCategoryQueryHandler _handler;

    public GetProductsByCategoryQueryHandlerTests()
    {
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _procedureRepository = Substitute.For<IProductProcedureRepository>();
        _mapper = new ProductMapper();

        _unitOfWork.ProductProcedures.Returns(_procedureRepository);
        _handler = new GetProductsByCategoryQueryHandler(_unitOfWork, _mapper);
    }

    [Fact]
    public async Task Handle_ValidCategory_ShouldReturnMappedProducts()
    {
        // Arrange
        var query = new GetProductsByCategoryQuery("Premium");

        var storedProcResults = new List<ProductByCategoryResult>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Name = "Premium Product 1",
                Description = "Description 1",
                Price = 250.00m,
                PriceCategory = "Premium"
            },
            new()
            {
                Id = Guid.NewGuid(),
                Name = "Premium Product 2",
                Description = "Description 2",
                Price = 350.00m,
                PriceCategory = "Premium"
            }
        };

        _procedureRepository
            .GetProductsByCategoryAsync(query.PriceCategory, Arg.Any<CancellationToken>())
            .Returns(storedProcResults);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        var resultList = result.ToList();
        resultList.Should().HaveCount(2);
        resultList[0].Name.Should().Be("Premium Product 1");
        resultList[0].PriceCategory.Should().Be("Premium");
        resultList[1].Name.Should().Be("Premium Product 2");
        resultList[1].Price.Should().Be(350.00m);

        await _procedureRepository.Received(1)
            .GetProductsByCategoryAsync("Premium", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_EmptyResult_ShouldReturnEmptyCollection()
    {
        // Arrange
        var query = new GetProductsByCategoryQuery("Luxury");

        _procedureRepository
            .GetProductsByCategoryAsync(query.PriceCategory, Arg.Any<CancellationToken>())
            .Returns(Enumerable.Empty<ProductByCategoryResult>());

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeEmpty();
    }
}
