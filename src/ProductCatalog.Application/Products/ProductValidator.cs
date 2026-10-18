using ProductCatalog.Domain.Entities;

namespace ProductCatalog.Application.Products;

public static class ProductValidator
{
    public static Dictionary<string, string[]> ValidateDescription(string? description)
    {
        var errors = new Dictionary<string, string[]>();

        if (string.IsNullOrWhiteSpace(description))
        {
            errors["description"] = ["Description is required and cannot be empty or whitespace."];
        }
        else if (description.Length > Product.DescriptionMaxLength)
        {
            errors["description"] = [$"Description must be at most {Product.DescriptionMaxLength} characters long."];
        }

        return errors;
    }
}
