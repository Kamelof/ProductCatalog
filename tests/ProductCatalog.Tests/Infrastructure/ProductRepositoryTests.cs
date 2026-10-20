using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using ProductCatalog.Application.Abstractions;
using ProductCatalog.Infrastructure.Data;
using ProductCatalog.Infrastructure.Repositories;

namespace ProductCatalog.Tests.Infrastructure;

public sealed class ProductRepositoryTests : ProductRepositoryContractTests, IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly DbContextOptions<ProductCatalogDbContext> _options;
    private readonly List<ProductCatalogDbContext> _contexts = [];

    public ProductRepositoryTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        _options = new DbContextOptionsBuilder<ProductCatalogDbContext>()
            .UseSqlite(_connection)
            .Options;

        using var context = new ProductCatalogDbContext(_options);
        context.Database.EnsureCreated();
    }

    protected override IProductRepository CreateRepository()
    {
        var context = new ProductCatalogDbContext(_options);
        _contexts.Add(context);
        return new ProductRepository(context);
    }

    public void Dispose()
    {
        foreach (var context in _contexts)
        {
            context.Dispose();
        }

        _connection.Dispose();
    }
}
