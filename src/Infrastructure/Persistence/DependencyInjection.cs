using CleanAPIDemo.Domain.Interfaces;
using CleanAPIDemo.Infrastructure.Persistence;
using CleanAPIDemo.Infrastructure.Repositories;
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

        return services;
    }

    public static IServiceCollection AddInfrastructureInMemory(this IServiceCollection services, string databaseName = "CleanAPIDemoDb")
    {
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseInMemoryDatabase(databaseName));

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IProductProcedureRepository, ProductProcedureRepository>();

        return services;
    }
}
