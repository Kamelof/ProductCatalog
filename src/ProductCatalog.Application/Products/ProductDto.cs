namespace ProductCatalog.Application.Products;

public sealed record ProductDto(int Id, string Name, string ImgUri, decimal Price, string? Description);
