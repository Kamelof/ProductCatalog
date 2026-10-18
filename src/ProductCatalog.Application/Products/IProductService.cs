using ProductCatalog.Application.Common;

namespace ProductCatalog.Application.Products;

public interface IProductService
{
    Task<IReadOnlyList<ProductDto>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<Result<PagedResult<ProductDto>>> GetPagedAsync(int page, int pageSize, CancellationToken cancellationToken = default);

    Task<ProductDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<Result<ProductDto>> UpdateDescriptionAsync(int id, string? description, CancellationToken cancellationToken = default);
}
