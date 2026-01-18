namespace CleanAPIDemo.Domain.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IProductRepository Products { get; }
    IProductProcedureRepository ProductProcedures { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
