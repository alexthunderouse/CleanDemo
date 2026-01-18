using Asp.Versioning;
using CleanAPIDemo.Application.Products.Commands.CreateProduct;
using CleanAPIDemo.Application.Products.DTOs;
using CleanAPIDemo.Application.Products.Queries.GetProductById;
using CleanAPIDemo.Application.Products.Queries.GetProductReport;
using CleanAPIDemo.Application.Products.Queries.GetProductsByCategory;
using CleanAPIDemo.Application.Products.Queries.GetProductSummaryView;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CleanAPIDemo.API.Controllers.V1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ProductsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var product = await _mediator.Send(new GetProductByIdQuery(id), cancellationToken);

        if (product is null)
        {
            return NotFound(new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Product Not Found",
                Detail = $"Product with ID {id} was not found.",
                Instance = HttpContext.Request.Path
            });
        }

        return Ok(product);
    }

    [HttpPost]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateProductDto dto, CancellationToken cancellationToken)
    {
        var command = new CreateProductCommand(dto.Name, dto.Description, dto.Price);
        var product = await _mediator.Send(command, cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id = product.Id, version = "1.0" }, product);
    }

    /// <summary>
    /// Executes stored procedure to get products by price category.
    /// </summary>
    [HttpGet("by-category/{priceCategory}")]
    [ProducesResponseType(typeof(IEnumerable<ProductByCategoryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetByCategory(string priceCategory, CancellationToken cancellationToken)
    {
        var query = new GetProductsByCategoryQuery(priceCategory);
        var products = await _mediator.Send(query, cancellationToken);

        return Ok(products);
    }

    /// <summary>
    /// Executes stored procedure with multiple result sets returning products and statistics.
    /// </summary>
    [HttpGet("report")]
    [ProducesResponseType(typeof(ProductReportDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetReport([FromQuery] decimal? minPrice, CancellationToken cancellationToken)
    {
        var query = new GetProductReportQuery(minPrice);
        var report = await _mediator.Send(query, cancellationToken);

        return Ok(report);
    }

    /// <summary>
    /// Queries the ProductSummary database view.
    /// </summary>
    [HttpGet("summary")]
    [ProducesResponseType(typeof(IEnumerable<ProductSummaryViewDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSummary(CancellationToken cancellationToken)
    {
        var query = new GetProductSummaryViewQuery();
        var summary = await _mediator.Send(query, cancellationToken);

        return Ok(summary);
    }
}
