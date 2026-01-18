-- =============================================
-- Stored Procedure: usp_GetProductsByCategory
-- Description: Returns products filtered by price category
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[usp_GetProductsByCategory]
    @PriceCategory NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;

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
    WHERE
        (@PriceCategory = 'Budget' AND p.Price < 50) OR
        (@PriceCategory = 'Mid-Range' AND p.Price >= 50 AND p.Price < 200) OR
        (@PriceCategory = 'Premium' AND p.Price >= 200 AND p.Price < 500) OR
        (@PriceCategory = 'Luxury' AND p.Price >= 500)
    ORDER BY p.Price;
END;
GO
