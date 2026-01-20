using BenchmarkDotNet.Attributes;
using CleanAPIDemo.Application.Features.Products.v1.Commands.CreateProduct;

namespace CleanAPIDemo.Benchmarks.Benchmarks.Validation;

[MemoryDiagnoser]
public class ProductValidatorBenchmarks
{
    private CreateProductCommandValidator _validator = null!;
    private CreateProductCommand _validCommand = null!;
    private CreateProductCommand _invalidCommand = null!;

    [GlobalSetup]
    public void Setup()
    {
        _validator = new CreateProductCommandValidator();

        _validCommand = new CreateProductCommand(
            Name: "Valid Product Name",
            Description: "A valid product description",
            Price: 99.99m);

        _invalidCommand = new CreateProductCommand(
            Name: "",
            Description: new string('x', 1500),
            Price: -10m);
    }

    [Benchmark]
    public void ValidateValidCommand()
    {
        _ = _validator.Validate(_validCommand);
    }

    [Benchmark]
    public void ValidateInvalidCommand()
    {
        _ = _validator.Validate(_invalidCommand);
    }
}
