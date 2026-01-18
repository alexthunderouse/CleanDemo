using CleanAPIDemo.Application.DataSync.DTOs;
using CleanAPIDemo.Domain.Entities;
using CleanAPIDemo.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CleanAPIDemo.Application.DataSync.Commands.SyncData;

public class SyncDataCommandHandler : IRequestHandler<SyncDataCommand, SyncDataResultDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<SyncDataCommandHandler> _logger;

    public SyncDataCommandHandler(IUnitOfWork unitOfWork, ILogger<SyncDataCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<SyncDataResultDto> Handle(SyncDataCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting data synchronization");

        try
        {
            // Step 1: Get records from database via repository
            var olderThan = DateTime.UtcNow.AddHours(-1);
            var products = (await _unitOfWork.Products.GetProductsForSyncAsync(olderThan, cancellationToken)).ToList();

            _logger.LogInformation("Retrieved {Count} products for sync", products.Count);

            if (products.Count == 0)
            {
                return new SyncDataResultDto(true, 0);
            }

            // Step 2: Apply changes to records
            foreach (var product in products)
            {
                ApplyChanges(product);
                await _unitOfWork.Products.UpdateAsync(product, cancellationToken);
            }

            // Step 3: Sync changes back to database
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Successfully synchronized {RecordCount} records", products.Count);

            return new SyncDataResultDto(true, products.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Data synchronization failed");
            return new SyncDataResultDto(false, 0, ex.Message);
        }
    }

    private void ApplyChanges(Product product)
    {
        // Apply price adjustment for low-priced products
        if (product.Price < 10)
        {
            var oldPrice = product.Price;
            product.Price = Math.Round(product.Price * 1.05m, 2);
            _logger.LogDebug("Price adjusted for '{ProductName}': {OldPrice} -> {NewPrice}",
                product.Name, oldPrice, product.Price);
        }

        // Normalize description
        if (!string.IsNullOrEmpty(product.Description))
        {
            product.Description = product.Description.Trim();
        }

        // Mark record as synced
        product.UpdatedAt = DateTime.UtcNow;
    }
}
