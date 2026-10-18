namespace ProductCatalog.Application.Common;

public enum ErrorType
{
    Validation,
    NotFound
}

public sealed record Error(
    ErrorType Type,
    string Message,
    IReadOnlyDictionary<string, string[]>? ValidationErrors = null)
{
    public static Error NotFound(string message) => new(ErrorType.NotFound, message);

    public static Error Validation(IReadOnlyDictionary<string, string[]> errors) =>
        new(ErrorType.Validation, "One or more validation errors occurred.", errors);
}
