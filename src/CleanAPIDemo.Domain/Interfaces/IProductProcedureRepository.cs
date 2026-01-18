using CleanAPIDemo.Domain.Entities.StoredProcedures;
using CleanAPIDemo.Domain.Entities.Views;

namespace CleanAPIDemo.Domain.Interfaces;

public interface IProductProcedureRepository
{
    Task<IEnumerable<ProductByCategoryResult>> GetProductsByCategoryAsync(
        string priceCategory,
        CancellationToken cancellationToken = default);

    Task<ProductReportMultiResult> GetProductReportAsync(
        decimal? minPrice,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<ProductSummaryView>> GetProductSummaryViewAsync(
        CancellationToken cancellationToken = default);
}
