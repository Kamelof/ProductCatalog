using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using ProductCatalog.Domain.Entities;

namespace ProductCatalog.Application.Products;

public static class ProductValidator
{
    public static Dictionary<string, string[]> ValidateDescription(string? description)
    {
        var errors = new Dictionary<string, string[]>();

        if (IsBlank(description))
        {
            errors["description"] = ["Description is required and cannot be empty or whitespace."];
        }
        else if (description.Length > Product.DescriptionMaxLength)
        {
            errors["description"] = [$"Description must be at most {Product.DescriptionMaxLength} characters long."];
        }
        else if (description.Any(IsForbiddenCharacter))
        {
            errors["description"] = ["Description cannot contain control or text direction override characters."];
        }

        return errors;
    }

    private static bool IsBlank([NotNullWhen(false)] string? value) =>
        value is null || value.All(c => char.IsWhiteSpace(c) || IsFormatCharacter(c));

    private static bool IsFormatCharacter(char c) =>
        CharUnicodeInfo.GetUnicodeCategory(c) == UnicodeCategory.Format;

    private static bool IsForbiddenCharacter(char c) =>
        (char.IsControl(c) && c is not ('\r' or '\n' or '\t'))
        || c is >= '\u202A' and <= '\u202E'
        || c is >= '\u2066' and <= '\u2069';
}
