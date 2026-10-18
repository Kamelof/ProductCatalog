using ProductCatalog.Application.Abstractions;
using ProductCatalog.Domain.Entities;
using ProductCatalog.Infrastructure.Data;

namespace ProductCatalog.Infrastructure.Repositories;

public class MockProductRepository : IProductRepository
{
    private readonly Dictionary<int, Product> _products;
    private readonly Lock _lock = new();

    public MockProductRepository()
        : this(SeedData.Products)
    {
    }

    public MockProductRepository(IEnumerable<Product> products)
    {
        _products = products.ToDictionary(p => p.Id, Copy);
    }

    public Task<IReadOnlyList<Product>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            IReadOnlyList<Product> result = Ordered().ToList();
            return Task.FromResult(result);
        }
    }

    public Task<IReadOnlyList<Product>> GetPageAsync(int skip, int take, CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            IReadOnlyList<Product> result = Ordered().Skip(skip).Take(take).ToList();
            return Task.FromResult(result);
        }
    }

    public Task<int> CountAsync(CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            return Task.FromResult(_products.Count);
        }
    }

    public Task<Product?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            var product = _products.TryGetValue(id, out var found) ? Copy(found) : null;
            return Task.FromResult(product);
        }
    }

    public Task UpdateAsync(Product product, CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            if (!_products.ContainsKey(product.Id))
            {
                throw new KeyNotFoundException($"Product with id {product.Id} does not exist.");
            }

            _products[product.Id] = Copy(product);
            return Task.CompletedTask;
        }
    }

    // Returned products are detached copies, the same way EF behaves with no tracking.
    // Changes are only stored after UpdateAsync is called.
    private IEnumerable<Product> Ordered() => _products.Values.OrderBy(p => p.Id).Select(Copy);

    private static Product Copy(Product p) => new(p.Id, p.Name, p.ImgUri, p.Price, p.Description);
}
