using Asp.Versioning.ApiExplorer;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace ProductCatalog.Api.Swagger;

public sealed class ConfigureSwaggerOptions(IApiVersionDescriptionProvider provider)
    : IConfigureOptions<SwaggerGenOptions>
{
    public void Configure(SwaggerGenOptions options)
    {
        foreach (var description in provider.ApiVersionDescriptions)
        {
            options.SwaggerDoc(description.GroupName, CreateInfo(description));
        }

        options.IncludeXmlComments(typeof(ConfigureSwaggerOptions).Assembly);
    }

    private static OpenApiInfo CreateInfo(ApiVersionDescription description)
    {
        var info = new OpenApiInfo
        {
            Title = "Product Catalog API",
            Version = description.ApiVersion.ToString(),
            Description = "REST API for listing e-shop products and updating product descriptions."
        };

        if (description.IsDeprecated)
        {
            info.Description += " This API version has been deprecated.";
        }

        return info;
    }
}
