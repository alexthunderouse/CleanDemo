using CleanAPIDemo.Domain.Interfaces;
using CleanAPIDemo.Infrastructure.Persistence;

namespace CleanAPIDemo.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;
    private IProductRepository? _productRepository;
    private IProductProcedureRepository? _productProcedureRepository;

    public UnitOfWork(ApplicationDbContext context)
    {
        _context = context;
    }

    public IProductRepository Products => _productRepository ??= new ProductRepository(_context);

    public IProductProcedureRepository ProductProcedures =>
        _productProcedureRepository ??= new ProductProcedureRepository(_context);

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public void Dispose()
    {
        _context.Dispose();
        GC.SuppressFinalize(this);
    }
}
