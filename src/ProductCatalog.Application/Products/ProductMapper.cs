using ProductCatalog.Domain.Entities;

namespace ProductCatalog.Application.Products;

public static class ProductMapper
{
    public static ProductDto ToDto(this Product product) =>
        new(product.Id, product.Name, product.ImgUri, product.Price, product.Description);
}
