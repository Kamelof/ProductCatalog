using ProductCatalog.Application.Abstractions;
using ProductCatalog.Infrastructure.Data;
using ProductCatalog.Infrastructure.Repositories;

namespace ProductCatalog.Tests.Infrastructure;

public class MockProductRepositoryTests : ProductRepositoryContractTests
{
    private readonly MockProductRepository _repository = new(SeedData.Products);

    protected override IProductRepository CreateRepository() => _repository;
}
