using CleanAPIDemo.Domain.Entities.StoredProcedures;
using CleanAPIDemo.Domain.Entities.Views;
using CleanAPIDemo.Domain.Interfaces;
using CleanAPIDemo.Infrastructure.Persistence;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace CleanAPIDemo.Infrastructure.Repositories;

public class ProductProcedureRepository : IProductProcedureRepository
{
    private readonly ApplicationDbContext _context;

    public ProductProcedureRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<ProductByCategoryResult>> GetProductsByCategoryAsync(
        string priceCategory,
        CancellationToken cancellationToken = default)
    {
        var priceCategoryParam = new SqlParameter("@PriceCategory", priceCategory);

        return await _context.Database
            .SqlQueryRaw<ProductByCategoryResult>(
                "EXEC dbo.usp_GetProductsByCategory @PriceCategory",
                priceCategoryParam)
            .ToListAsync(cancellationToken);
    }

    public async Task<ProductReportMultiResult> GetProductReportAsync(
        decimal? minPrice,
        CancellationToken cancellationToken = default)
    {
        var result = new ProductReportMultiResult();

        var connection = _context.Database.GetDbConnection();
        await connection.OpenAsync(cancellationToken);

        try
        {
            await using var command = connection.CreateCommand();
            command.CommandText = "dbo.usp_GetProductReport";
            command.CommandType = System.Data.CommandType.StoredProcedure;

            var minPriceParam = command.CreateParameter();
            minPriceParam.ParameterName = "@MinPrice";
            minPriceParam.Value = minPrice ?? (object)DBNull.Value;
            command.Parameters.Add(minPriceParam);

            await using var reader = await command.ExecuteReaderAsync(cancellationToken);

            // First result set: Products
            var products = new List<ProductByCategoryResult>();
            while (await reader.ReadAsync(cancellationToken))
            {
                products.Add(new ProductByCategoryResult
                {
                    Id = reader.GetGuid(reader.GetOrdinal("Id")),
                    Name = reader.GetString(reader.GetOrdinal("Name")),
                    Description = reader.GetString(reader.GetOrdinal("Description")),
                    Price = reader.GetDecimal(reader.GetOrdinal("Price")),
                    PriceCategory = reader.GetString(reader.GetOrdinal("PriceCategory"))
                });
            }
            result.Products = products;

            // Move to second result set: Statistics
            if (await reader.NextResultAsync(cancellationToken))
            {
                var statistics = new List<ProductStatisticsResult>();
                while (await reader.ReadAsync(cancellationToken))
                {
                    statistics.Add(new ProductStatisticsResult
                    {
                        TotalProducts = reader.GetInt32(reader.GetOrdinal("TotalProducts")),
                        AveragePrice = reader.GetDecimal(reader.GetOrdinal("AveragePrice")),
                        MinPrice = reader.GetDecimal(reader.GetOrdinal("MinPrice")),
                        MaxPrice = reader.GetDecimal(reader.GetOrdinal("MaxPrice")),
                        PriceCategory = reader.GetString(reader.GetOrdinal("PriceCategory"))
                    });
                }
                result.Statistics = statistics;
            }
        }
        finally
        {
            await connection.CloseAsync();
        }

        return result;
    }

    public async Task<IEnumerable<ProductSummaryView>> GetProductSummaryViewAsync(
        CancellationToken cancellationToken = default)
    {
        return await _context.ProductSummaryView
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }
}
