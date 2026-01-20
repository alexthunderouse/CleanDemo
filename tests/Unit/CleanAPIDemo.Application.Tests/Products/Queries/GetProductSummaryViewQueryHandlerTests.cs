using CleanAPIDemo.Application.Common.Mappings;
using CleanAPIDemo.Application.Products.Queries.V1.GetProductSummaryView;
using CleanAPIDemo.Domain.Entities.Views;
using CleanAPIDemo.Domain.Interfaces;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace CleanAPIDemo.Application.Tests.Products.Queries;

public class GetProductSummaryViewQueryHandlerTests
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IProductProcedureRepository _procedureRepository;
    private readonly ProductMapper _mapper;
    private readonly GetProductSummaryViewQueryHandler _handler;

    public GetProductSummaryViewQueryHandlerTests()
    {
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _procedureRepository = Substitute.For<IProductProcedureRepository>();
        _mapper = new ProductMapper();

        _unitOfWork.ProductProcedures.Returns(_procedureRepository);
        _handler = new GetProductSummaryViewQueryHandler(_unitOfWork, _mapper);
    }

    [Fact]
    public async Task Handle_ShouldReturnMappedViewData()
    {
        // Arrange
        var query = new GetProductSummaryViewQuery();

        var viewResults = new List<ProductSummaryView>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Name = "Budget Product",
                Price = 25.00m,
                PriceCategory = "Budget",
                DaysSinceCreated = 30
            },
            new()
            {
                Id = Guid.NewGuid(),
                Name = "Premium Product",
                Price = 300.00m,
                PriceCategory = "Premium",
                DaysSinceCreated = 15
            },
            new()
            {
                Id = Guid.NewGuid(),
                Name = "Luxury Product",
                Price = 1000.00m,
                PriceCategory = "Luxury",
                DaysSinceCreated = 5
            }
        };

        _procedureRepository
            .GetProductSummaryViewAsync(Arg.Any<CancellationToken>())
            .Returns(viewResults);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        var resultList = result.ToList();
        resultList.Should().HaveCount(3);

        resultList[0].Name.Should().Be("Budget Product");
        resultList[0].PriceCategory.Should().Be("Budget");
        resultList[0].DaysSinceCreated.Should().Be(30);

        resultList[1].Name.Should().Be("Premium Product");
        resultList[1].Price.Should().Be(300.00m);

        resultList[2].PriceCategory.Should().Be("Luxury");

        await _procedureRepository.Received(1)
            .GetProductSummaryViewAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_EmptyView_ShouldReturnEmptyCollection()
    {
        // Arrange
        var query = new GetProductSummaryViewQuery();

        _procedureRepository
            .GetProductSummaryViewAsync(Arg.Any<CancellationToken>())
            .Returns(Enumerable.Empty<ProductSummaryView>());

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeEmpty();
    }
}
