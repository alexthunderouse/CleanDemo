namespace CleanAPIDemo.Domain.Entities.StoredProcedures;

public class ProductStatisticsResult
{
    public int TotalProducts { get; set; }
    public decimal AveragePrice { get; set; }
    public decimal MinPrice { get; set; }
    public decimal MaxPrice { get; set; }
    public string PriceCategory { get; set; } = string.Empty;
}
