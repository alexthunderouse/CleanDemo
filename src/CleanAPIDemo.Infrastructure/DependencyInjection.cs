using CleanAPIDemo.Domain.Interfaces;
using CleanAPIDemo.Infrastructure.Persistence;
using CleanAPIDemo.Infrastructure.Repositories;
using CleanAPIDemo.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CleanAPIDemo.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(connectionString));

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IProductProcedureRepository, ProductProcedureRepository>();
        services.AddScoped<IDataSyncService, DataSyncService>();

        return services;
    }
}
