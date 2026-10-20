using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ProductCatalog.Application.Abstractions;
using ProductCatalog.Infrastructure;
using ProductCatalog.Infrastructure.Repositories;

namespace ProductCatalog.Tests.Infrastructure;

public class DependencyInjectionTests
{
    private const string ConnectionString = "Server=localhost;Database=ProductCatalog;Integrated Security=true";

    [Theory]
    [InlineData("Mock")]
    [InlineData("mock")]
    [InlineData(" Mock ")]
    [InlineData(null)]
    [InlineData("")]
    public void AddInfrastructure_MockOrMissingDataSource_RegistersMockRepository(string? dataSource)
    {
        using var provider = BuildProvider(dataSource, connectionString: null);

        provider.GetRequiredService<IProductRepository>().ShouldBeOfType<MockProductRepository>();
    }

    [Theory]
    [InlineData("Sql")]
    [InlineData("SQL")]
    public void AddInfrastructure_SqlDataSource_RegistersEfRepository(string dataSource)
    {
        using var provider = BuildProvider(dataSource, ConnectionString);
        using var scope = provider.CreateScope();

        scope.ServiceProvider.GetRequiredService<IProductRepository>().ShouldBeOfType<ProductRepository>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void AddInfrastructure_SqlWithoutConnectionString_Throws(string? connectionString)
    {
        var exception = Should.Throw<InvalidOperationException>(() => BuildProvider("Sql", connectionString));

        exception.Message.ShouldContain(DependencyInjection.ConnectionStringName);
    }

    [Theory]
    [InlineData("Redis")]
    [InlineData("0")]
    [InlineData("1")]
    [InlineData("Mock,Sql")]
    public void AddInfrastructure_UnknownDataSource_Throws(string dataSource)
    {
        var exception = Should.Throw<InvalidOperationException>(() => BuildProvider(dataSource, ConnectionString));

        exception.Message.ShouldContain(dataSource);
    }

    private static ServiceProvider BuildProvider(string? dataSource, string? connectionString)
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                [DependencyInjection.DataSourceKey] = dataSource,
                [$"ConnectionStrings:{DependencyInjection.ConnectionStringName}"] = connectionString
            })
            .Build();

        var services = new ServiceCollection();
        services.AddInfrastructure(configuration);

        return services.BuildServiceProvider();
    }
}
