using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ProductCatalog.Application.Abstractions;
using ProductCatalog.Infrastructure.Data;
using ProductCatalog.Infrastructure.Repositories;

namespace ProductCatalog.Infrastructure;

public static class DependencyInjection
{
    public const string DataSourceKey = "DataSource";
    public const string ConnectionStringName = "ProductCatalog";

    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var dataSource = GetDataSource(configuration);

        if (dataSource == DataSource.Sql)
        {
            var connectionString = configuration.GetConnectionString(ConnectionStringName);
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException(
                    $"Connection string '{ConnectionStringName}' is required when {DataSourceKey} is '{DataSource.Sql}'.");
            }

            services.AddDbContext<ProductCatalogDbContext>(options =>
                options.UseSqlServer(connectionString, sql => sql.EnableRetryOnFailure()));

            services.AddScoped<IProductRepository, ProductRepository>();
        }
        else
        {
            services.AddSingleton<IProductRepository>(_ => new MockProductRepository(SeedData.Products));
        }

        return services;
    }

    public static async Task MigrateDatabaseAsync(this IServiceProvider serviceProvider, CancellationToken cancellationToken = default)
    {
        await using var scope = serviceProvider.CreateAsyncScope();

        var context = scope.ServiceProvider.GetService<ProductCatalogDbContext>();
        if (context is null)
        {
            return;
        }

        await context.Database.MigrateAsync(cancellationToken);
    }

    private static DataSource GetDataSource(IConfiguration configuration)
    {
        var value = configuration[DataSourceKey];
        if (string.IsNullOrWhiteSpace(value))
        {
            return DataSource.Mock;
        }

        if (!Enum.TryParse<DataSource>(value, ignoreCase: true, out var dataSource) || !Enum.IsDefined(dataSource))
        {
            throw new InvalidOperationException(
                $"Unknown {DataSourceKey} '{value}'. Allowed values: {string.Join(", ", Enum.GetNames<DataSource>())}.");
        }

        return dataSource;
    }
}
