using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute.ExceptionExtensions;
using ProductCatalog.Application.Abstractions;

namespace ProductCatalog.Tests.Api;

public class ErrorHandlingTests(ProductCatalogApiFactory factory) : IClassFixture<ProductCatalogApiFactory>
{
    private const string SensitiveMessage = "Connection to db-prod-01 failed for user admin";

    [Theory]
    [InlineData("/api/v1/products")]
    [InlineData("/api/v2/products")]
    [InlineData("/api/v1/products/1")]
    public async Task UnhandledException_ReturnsGenericProblemWithoutDetails(string url)
    {
        var repository = Substitute.For<IProductRepository>();
        var exception = new InvalidOperationException(SensitiveMessage);
        repository.GetAllAsync(Arg.Any<CancellationToken>()).ThrowsAsync(exception);
        repository.CountAsync(Arg.Any<CancellationToken>()).ThrowsAsync(exception);
        repository.GetByIdAsync(Arg.Any<int>(), Arg.Any<CancellationToken>()).ThrowsAsync(exception);

        var client = factory
            .WithWebHostBuilder(builder => builder.ConfigureTestServices(services => services.AddSingleton(repository)))
            .CreateClient();

        var response = await client.GetAsync(url);

        response.StatusCode.ShouldBe(HttpStatusCode.InternalServerError);
        response.Content.Headers.ContentType?.MediaType.ShouldBe("application/problem+json");

        var body = await response.Content.ReadAsStringAsync();
        body.ShouldNotContain(SensitiveMessage);
        body.ShouldNotContain(nameof(InvalidOperationException));
        body.ShouldNotContain("   at ");

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        problem.ShouldNotBeNull().Status.ShouldBe(StatusCodes.Status500InternalServerError);
    }

    [Fact]
    public async Task Swagger_ProductionEnvironment_IsNotExposed()
    {
        var client = factory
            .WithWebHostBuilder(builder => builder.UseEnvironment("Production"))
            .CreateClient();

        var response = await client.GetAsync("/swagger/v1/swagger.json");

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }
}
