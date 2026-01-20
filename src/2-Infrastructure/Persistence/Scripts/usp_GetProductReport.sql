-- =============================================
-- Stored Procedure: usp_GetProductReport
-- Description: Returns multiple result sets - products and statistics
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[usp_GetProductReport]
    @MinPrice DECIMAL(18,2) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    -- First Result Set: Products with their categories
    SELECT
        p.Id,
        p.Name,
        p.Description,
        p.Price,
        CASE
            WHEN p.Price < 50 THEN 'Budget'
            WHEN p.Price < 200 THEN 'Mid-Range'
            WHEN p.Price < 500 THEN 'Premium'
            ELSE 'Luxury'
        END AS PriceCategory
    FROM dbo.Products p
    WHERE @MinPrice IS NULL OR p.Price >= @MinPrice
    ORDER BY p.Price;

    -- Second Result Set: Statistics grouped by category
    SELECT
        COUNT(*) AS TotalProducts,
        AVG(p.Price) AS AveragePrice,
        MIN(p.Price) AS MinPrice,
        MAX(p.Price) AS MaxPrice,
        CASE
            WHEN p.Price < 50 THEN 'Budget'
            WHEN p.Price < 200 THEN 'Mid-Range'
            WHEN p.Price < 500 THEN 'Premium'
            ELSE 'Luxury'
        END AS PriceCategory
    FROM dbo.Products p
    WHERE @MinPrice IS NULL OR p.Price >= @MinPrice
    GROUP BY
        CASE
            WHEN p.Price < 50 THEN 'Budget'
            WHEN p.Price < 200 THEN 'Mid-Range'
            WHEN p.Price < 500 THEN 'Premium'
            ELSE 'Luxury'
        END
    ORDER BY MIN(p.Price);
END;
GO
