namespace CleanAPIDemo.Domain.Entities.StoredProcedures;

public class ProductReportMultiResult
{
    public IEnumerable<ProductByCategoryResult> Products { get; set; } = [];
    public IEnumerable<ProductStatisticsResult> Statistics { get; set; } = [];
}
