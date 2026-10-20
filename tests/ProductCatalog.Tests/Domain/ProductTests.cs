using ProductCatalog.Domain.Entities;

namespace ProductCatalog.Tests.Domain;

public class ProductTests
{
    [Fact]
    public void UpdateDescription_ValidDescription_ChangesOnlyDescription()
    {
        var product = CreateProduct();

        product.UpdateDescription("New description");

        product.Description.ShouldBe("New description");
        product.Id.ShouldBe(1);
        product.Name.ShouldBe("Phone");
        product.ImgUri.ShouldBe("https://example.com/img/1.jpg");
        product.Price.ShouldBe(999.90m);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void UpdateDescription_BlankDescription_Throws(string description)
    {
        var product = CreateProduct();

        Should.Throw<ArgumentException>(() => product.UpdateDescription(description));
        product.Description.ShouldBe("Old description");
    }

    [Fact]
    public void UpdateDescription_NullDescription_Throws()
    {
        var product = CreateProduct();

        Should.Throw<ArgumentNullException>(() => product.UpdateDescription(null!));
    }

    [Fact]
    public void UpdateDescription_TooLongDescription_Throws()
    {
        var product = CreateProduct();

        Should.Throw<ArgumentOutOfRangeException>(
            () => product.UpdateDescription(new string('a', Product.DescriptionMaxLength + 1)));
        product.Description.ShouldBe("Old description");
    }

    private static Product CreateProduct() =>
        new(1, "Phone", "https://example.com/img/1.jpg", 999.90m, "Old description");
}
