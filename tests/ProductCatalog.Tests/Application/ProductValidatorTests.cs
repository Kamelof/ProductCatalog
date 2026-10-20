using ProductCatalog.Application.Products;
using ProductCatalog.Domain.Entities;

namespace ProductCatalog.Tests.Application;

public class ProductValidatorTests
{
    [Theory]
    [InlineData("Wireless headphones with noise cancellation.")]
    [InlineData("Sluchátka s aktivním potlačením hluku, černá barva.")]
    [InlineData("First line\nSecond line\r\n\tIndented line")]
    [InlineData("Price includes 21 % VAT – shipping is free.")]
    public void ValidateDescription_ValidDescription_ReturnsNoErrors(string description)
    {
        var errors = ProductValidator.ValidateDescription(description);

        errors.ShouldBeEmpty();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("\t\r\n")]
    [InlineData("\u00A0")]
    [InlineData("\u200B")]
    [InlineData(" \u200B\uFEFF ")]
    public void ValidateDescription_NullEmptyOrInvisible_ReturnsError(string? description)
    {
        var errors = ProductValidator.ValidateDescription(description);

        errors.ShouldContainKey("description");
    }

    [Theory]
    [InlineData("a\u0000b")]
    [InlineData("bell\u0007")]
    [InlineData("escape\u001B[31m")]
    [InlineData("delete\u007F")]
    public void ValidateDescription_ControlCharacters_ReturnsError(string description)
    {
        var errors = ProductValidator.ValidateDescription(description);

        errors.ShouldContainKey("description");
    }

    [Theory]
    [InlineData("safe\u202Eevil")]
    [InlineData("left\u202Aembedding")]
    [InlineData("isolate\u2066text\u2069")]
    public void ValidateDescription_TextDirectionOverrides_ReturnsError(string description)
    {
        var errors = ProductValidator.ValidateDescription(description);

        errors.ShouldContainKey("description");
    }

    [Fact]
    public void ValidateDescription_HtmlMarkup_IsAcceptedAsPlainText()
    {
        var errors = ProductValidator.ValidateDescription("<script>alert(1)</script>");

        errors.ShouldBeEmpty();
    }

    [Fact]
    public void ValidateDescription_ExactlyMaxLength_ReturnsNoErrors()
    {
        var errors = ProductValidator.ValidateDescription(new string('a', Product.DescriptionMaxLength));

        errors.ShouldBeEmpty();
    }

    [Fact]
    public void ValidateDescription_MaxLengthPlusOne_ReturnsError()
    {
        var errors = ProductValidator.ValidateDescription(new string('a', Product.DescriptionMaxLength + 1));

        errors.ShouldContainKey("description");
    }
}
