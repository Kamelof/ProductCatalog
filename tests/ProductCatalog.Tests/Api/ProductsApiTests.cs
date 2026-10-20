using System.Net;
using System.Net.Http.Json;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ProductCatalog.Application.Common;
using ProductCatalog.Application.Products;
using ProductCatalog.Infrastructure.Data;

namespace ProductCatalog.Tests.Api;

public class ProductsApiTests(ProductCatalogApiFactory factory) : IClassFixture<ProductCatalogApiFactory>
{
    private const string ProblemJson = "application/problem+json";

    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task GetProductsV1_ReturnsAllProducts()
    {
        var response = await _client.GetAsync("/api/v1/products");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var products = await response.Content.ReadFromJsonAsync<List<ProductDto>>();
        products.ShouldNotBeNull().Count.ShouldBe(SeedData.Products.Count);
    }

    [Fact]
    public async Task GetProductsV2_NoParameters_ReturnsFirstPageOfDefaultSize()
    {
        var response = await _client.GetAsync("/api/v2/products");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var paged = (await response.Content.ReadFromJsonAsync<PagedResult<ProductDto>>()).ShouldNotBeNull();
        paged.Page.ShouldBe(Paging.DefaultPage);
        paged.PageSize.ShouldBe(Paging.DefaultPageSize);
        paged.Items.Count.ShouldBe(Paging.DefaultPageSize);
    }

    [Fact]
    public async Task GetProductsV2_SecondPage_ReturnsItemsWithPagingMetadata()
    {
        var response = await _client.GetAsync("/api/v2/products?page=2&pageSize=10");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var paged = (await response.Content.ReadFromJsonAsync<PagedResult<ProductDto>>()).ShouldNotBeNull();
        paged.Items.Select(p => p.Id).ShouldBe(Enumerable.Range(11, 10));
        paged.Page.ShouldBe(2);
        paged.PageSize.ShouldBe(10);
        paged.TotalCount.ShouldBe(SeedData.Products.Count);
        paged.TotalPages.ShouldBe(3);
    }

    [Theory]
    [InlineData("page=0")]
    [InlineData("pageSize=0")]
    [InlineData("pageSize=101")]
    public async Task GetProductsV2_InvalidPaging_ReturnsValidationProblem(string query)
    {
        var response = await _client.GetAsync($"/api/v2/products?{query}");

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        response.Content.Headers.ContentType?.MediaType.ShouldBe(ProblemJson);
        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        problem.ShouldNotBeNull().Errors.ShouldNotBeEmpty();
    }

    [Fact]
    public async Task GetProductById_ExistingId_ReturnsProduct()
    {
        var response = await _client.GetAsync("/api/v1/products/1");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var product = await response.Content.ReadFromJsonAsync<ProductDto>();
        product.ShouldNotBeNull().Id.ShouldBe(1);
    }

    [Fact]
    public async Task GetProductById_UnknownId_ReturnsNotFoundProblem()
    {
        var response = await _client.GetAsync("/api/v1/products/99999");

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        response.Content.Headers.ContentType?.MediaType.ShouldBe(ProblemJson);
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        problem.ShouldNotBeNull().Status.ShouldBe(StatusCodes.Status404NotFound);
    }

    [Fact]
    public async Task GetProductById_NonNumericId_ReturnsNotFound()
    {
        var response = await _client.GetAsync("/api/v1/products/abc");

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task PatchDescription_ValidBody_ReturnsUpdatedProductAndPersistsChange()
    {
        using var isolatedFactory = new ProductCatalogApiFactory();
        var client = isolatedFactory.CreateClient();

        var response = await client.PatchAsJsonAsync(
            "/api/v1/products/4/description",
            new { description = "Nový popis produktu", name = "Ignored" });

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var updated = (await response.Content.ReadFromJsonAsync<ProductDto>()).ShouldNotBeNull();
        updated.Description.ShouldBe("Nový popis produktu");
        updated.Name.ShouldBe(SeedData.Products.Single(p => p.Id == 4).Name);

        var reloaded = await client.GetFromJsonAsync<ProductDto>("/api/v1/products/4");
        reloaded.ShouldNotBeNull().Description.ShouldBe("Nový popis produktu");
    }

    [Theory]
    [InlineData("{\"description\":\"   \"}")]
    [InlineData("{\"description\":null}")]
    [InlineData("{}")]
    public async Task PatchDescription_InvalidDescription_ReturnsValidationProblem(string body)
    {
        var response = await _client.PatchAsync(
            "/api/v1/products/1/description",
            new StringContent(body, Encoding.UTF8, "application/json"));

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        problem.ShouldNotBeNull().Errors.ShouldContainKey("description");
    }

    [Theory]
    [InlineData("{\"description\":\"a\\u0000b\"}")]
    [InlineData("{\"description\":\"safe\\u202Eevil\"}")]
    [InlineData("{\"description\":\"\\u200B\"}")]
    public async Task PatchDescription_ForbiddenOrInvisibleCharacters_ReturnsValidationProblem(string body)
    {
        var response = await _client.PatchAsync(
            "/api/v1/products/1/description",
            new StringContent(body, Encoding.UTF8, "application/json"));

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        problem.ShouldNotBeNull().Errors.ShouldContainKey("description");
    }

    [Fact]
    public async Task PatchDescription_TooLongDescription_ReturnsValidationProblem()
    {
        var response = await _client.PatchAsJsonAsync(
            "/api/v1/products/1/description",
            new { description = new string('a', 4001) });

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task PatchDescription_UnknownId_ReturnsNotFoundProblem()
    {
        var response = await _client.PatchAsJsonAsync(
            "/api/v1/products/99999/description",
            new { description = "New description" });

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        response.Content.Headers.ContentType?.MediaType.ShouldBe(ProblemJson);
    }

    [Theory]
    [InlineData("v1", "/api/v1/products")]
    [InlineData("v2", "/api/v2/products")]
    public async Task SwaggerJson_ApiVersion_IsAvailable(string version, string expectedPath)
    {
        var response = await _client.GetAsync($"/swagger/{version}/swagger.json");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var document = await response.Content.ReadAsStringAsync();
        document.ShouldContain($"\"{expectedPath}\"");
    }
}
