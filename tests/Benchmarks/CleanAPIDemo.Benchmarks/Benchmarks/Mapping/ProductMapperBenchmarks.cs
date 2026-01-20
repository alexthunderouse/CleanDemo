using BenchmarkDotNet.Attributes;
using CleanAPIDemo.Application.Mappings;
using CleanAPIDemo.Domain.Entities;

namespace CleanAPIDemo.Benchmarks.Benchmarks.Mapping;

[MemoryDiagnoser]
public class ProductMapperBenchmarks
{
    private ProductMapper _mapper = null!;
    private Product _singleProduct = null!;
    private List<Product> _productBatch = null!;

    [GlobalSetup]
    public void Setup()
    {
        _mapper = new ProductMapper();

        _singleProduct = new Product
        {
            Id = Guid.NewGuid(),
            Name = "Test Product",
            Description = "A test product description",
            Price = 99.99m,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _productBatch = Enumerable.Range(1, 100)
            .Select(i => new Product
            {
                Id = Guid.NewGuid(),
                Name = $"Product {i}",
                Description = $"Description for product {i}",
                Price = i * 10.0m,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            })
            .ToList();
    }

    [Benchmark]
    public void ToProductDto_SingleItem()
    {
        _ = _mapper.ToProductDto(_singleProduct);
    }

    [Benchmark]
    public void ToProductDtoV2_SingleItem()
    {
        _ = _mapper.ToProductDtoV2(_singleProduct);
    }

    [Benchmark]
    public void ToProductDto_Batch()
    {
        foreach (var product in _productBatch)
        {
            _ = _mapper.ToProductDto(product);
        }
    }

    [Benchmark]
    public void ToProductDtoV2_Batch()
    {
        foreach (var product in _productBatch)
        {
            _ = _mapper.ToProductDtoV2(product);
        }
    }
}
