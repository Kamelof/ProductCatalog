namespace ProductCatalog.Domain.Entities;

public class Product
{
    public const int NameMaxLength = 200;
    public const int ImgUriMaxLength = 2048;
    public const int DescriptionMaxLength = 4000;

    public Product(int id, string name, string imgUri, decimal price, string? description = null)
    {
        Id = id;
        Name = name;
        ImgUri = imgUri;
        Price = price;
        Description = description;
    }

    public int Id { get; private set; }

    public string Name { get; private set; }

    public string ImgUri { get; private set; }

    public decimal Price { get; private set; }

    public string? Description { get; private set; }

    public void UpdateDescription(string description)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(description);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(description.Length, DescriptionMaxLength, nameof(description));

        Description = description;
    }
}
