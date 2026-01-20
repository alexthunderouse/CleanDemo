using Asp.Versioning;
using CleanAPIDemo.Application.Features.Products.v2.Commands.CreateProduct;
using CleanAPIDemo.Application.Features.Products.v2;
using CleanAPIDemo.Application.Features.Products.v2.Queries.GetProductById;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CleanAPIDemo.API.Controllers.V2;

[ApiController]
[ApiVersion("2.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly ISender _sender;

    public ProductsController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var product = await _sender.Send(new GetProductByIdQuery(id), cancellationToken);

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
        var command = new CreateProductCommand(dto.Name, dto.Description, dto.Price, dto.Category);
        var product = await _sender.Send(command, cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id = product.Id, version = "2.0" }, product);
    }
}
