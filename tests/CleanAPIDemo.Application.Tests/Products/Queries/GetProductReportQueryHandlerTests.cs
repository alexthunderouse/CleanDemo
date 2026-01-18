using CleanAPIDemo.Application.Common.Mappings;
using CleanAPIDemo.Application.Products.Queries.GetProductReport;
using CleanAPIDemo.Domain.Entities.StoredProcedures;
using CleanAPIDemo.Domain.Interfaces;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace CleanAPIDemo.Application.Tests.Products.Queries;

public class GetProductReportQueryHandlerTests
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IProductProcedureRepository _procedureRepository;
    private readonly ProductMapper _mapper;
    private readonly GetProductReportQueryHandler _handler;

    public GetProductReportQueryHandlerTests()
    {
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _procedureRepository = Substitute.For<IProductProcedureRepository>();
        _mapper = new ProductMapper();

        _unitOfWork.ProductProcedures.Returns(_procedureRepository);
        _handler = new GetProductReportQueryHandler(_unitOfWork, _mapper);
    }

    [Fact]
    public async Task Handle_WithMinPrice_ShouldReturnMultipleResultSets()
    {
        // Arrange
        var query = new GetProductReportQuery(MinPrice: 100.00m);

        var multiResult = new ProductReportMultiResult
        {
            Products = new List<ProductByCategoryResult>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    Name = "Product 1",
                    Description = "Description 1",
                    Price = 150.00m,
                    PriceCategory = "Mid-Range"
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    Name = "Product 2",
                    Description = "Description 2",
                    Price = 250.00m,
                    PriceCategory = "Premium"
                }
            },
            Statistics = new List<ProductStatisticsResult>
            {
                new()
                {
                    TotalProducts = 5,
                    AveragePrice = 175.00m,
                    MinPrice = 100.00m,
                    MaxPrice = 199.99m,
                    PriceCategory = "Mid-Range"
                },
                new()
                {
                    TotalProducts = 3,
                    AveragePrice = 350.00m,
                    MinPrice = 200.00m,
                    MaxPrice = 499.99m,
                    PriceCategory = "Premium"
                }
            }
        };

        _procedureRepository
            .GetProductReportAsync(query.MinPrice, Arg.Any<CancellationToken>())
            .Returns(multiResult);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();

        var products = result.Products.ToList();
        products.Should().HaveCount(2);
        products[0].Name.Should().Be("Product 1");
        products[1].PriceCategory.Should().Be("Premium");

        var statistics = result.Statistics.ToList();
        statistics.Should().HaveCount(2);
        statistics[0].TotalProducts.Should().Be(5);
        statistics[0].PriceCategory.Should().Be("Mid-Range");
        statistics[1].AveragePrice.Should().Be(350.00m);

        await _procedureRepository.Received(1)
            .GetProductReportAsync(100.00m, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithoutMinPrice_ShouldPassNullToRepository()
    {
        // Arrange
        var query = new GetProductReportQuery();

        var multiResult = new ProductReportMultiResult
        {
            Products = new List<ProductByCategoryResult>(),
            Statistics = new List<ProductStatisticsResult>()
        };

        _procedureRepository
            .GetProductReportAsync(null, Arg.Any<CancellationToken>())
            .Returns(multiResult);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Products.Should().BeEmpty();
        result.Statistics.Should().BeEmpty();

        await _procedureRepository.Received(1)
            .GetProductReportAsync(null, Arg.Any<CancellationToken>());
    }
}
