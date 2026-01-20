namespace CleanAPIDemo.Domain.Entities.Views;

public class ProductSummaryView
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string PriceCategory { get; set; } = string.Empty;
    public int DaysSinceCreated { get; set; }
}
