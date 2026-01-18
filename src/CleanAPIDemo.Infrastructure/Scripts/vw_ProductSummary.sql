-- =============================================
-- View: vw_ProductSummary
-- Description: Provides a summary view of products with calculated fields
-- =============================================
CREATE OR ALTER VIEW [dbo].[vw_ProductSummary]
AS
SELECT
    p.Id,
    p.Name,
    p.Price,
    CASE
        WHEN p.Price < 50 THEN 'Budget'
        WHEN p.Price < 200 THEN 'Mid-Range'
        WHEN p.Price < 500 THEN 'Premium'
        ELSE 'Luxury'
    END AS PriceCategory,
    DATEDIFF(DAY, p.CreatedAt, GETUTCDATE()) AS DaysSinceCreated
FROM dbo.Products p;
GO
