using ProductCatalog.Application.Abstractions;
using ProductCatalog.Application.Common;

namespace ProductCatalog.Application.Products;

public class ProductService(IProductRepository repository) : IProductService
{
    public async Task<IReadOnlyList<ProductDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var products = await repository.GetAllAsync(cancellationToken);
        return products.Select(p => p.ToDto()).ToList();
    }

    public async Task<Result<PagedResult<ProductDto>>> GetPagedAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var errors = Paging.Validate(page, pageSize);
        if (errors.Count > 0)
        {
            return Error.Validation(errors);
        }

        var totalCount = await repository.CountAsync(cancellationToken);

        // Computed as long, so a huge page number can't overflow the int skip value.
        var skip = (long)(page - 1) * pageSize;
        if (skip >= totalCount)
        {
            return new PagedResult<ProductDto>([], page, pageSize, totalCount);
        }

        var products = await repository.GetPageAsync((int)skip, pageSize, cancellationToken);
        var items = products.Select(p => p.ToDto()).ToList();

        return new PagedResult<ProductDto>(items, page, pageSize, totalCount);
    }

    public async Task<ProductDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var product = await repository.GetByIdAsync(id, cancellationToken);
        return product?.ToDto();
    }

    public async Task<Result<ProductDto>> UpdateDescriptionAsync(
        int id,
        string? description,
        CancellationToken cancellationToken = default)
    {
        var errors = ProductValidator.ValidateDescription(description);
        if (errors.Count > 0)
        {
            return Error.Validation(errors);
        }

        var product = await repository.GetByIdAsync(id, cancellationToken);
        if (product is null)
        {
            return Error.NotFound($"Product with id {id} was not found.");
        }

        product.UpdateDescription(description!);
        await repository.UpdateAsync(product, cancellationToken);

        return product.ToDto();
    }
}
