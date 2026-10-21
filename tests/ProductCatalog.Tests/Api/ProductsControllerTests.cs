using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using ProductCatalog.Api.Contracts;
using ProductCatalog.Api.Controllers;
using ProductCatalog.Application.Common;
using ProductCatalog.Application.Products;

namespace ProductCatalog.Tests.Api;

public class ProductsControllerTests
{
    private static readonly ProductDto Product =
        new(1, "Phone", "https://example.com/img/1.jpg", 999.90m, "Description");

    private readonly IProductService _service = Substitute.For<IProductService>();
    private readonly ProductsController _controller;

    public ProductsControllerTests()
    {
        var services = new ServiceCollection()
            .AddLogging()
            .AddControllers().Services
            .BuildServiceProvider();

        _controller = new ProductsController(_service)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { RequestServices = services }
            }
        };
    }

    [Fact]
    public async Task GetAll_ProductsExist_ReturnsOkWithProducts()
    {
        IReadOnlyList<ProductDto> products = [Product];
        _service.GetAllAsync(Arg.Any<CancellationToken>()).Returns(products);

        var response = await _controller.GetAll(CancellationToken.None);

        var ok = response.Result.ShouldBeOfType<OkObjectResult>();
        ok.Value.ShouldBe(products);
    }

    [Fact]
    public async Task GetPaged_ValidRequest_ReturnsOkWithPage()
    {
        var page = new PagedResult<ProductDto>([Product], 2, 5, 6);
        _service.GetPagedAsync(2, 5, Arg.Any<CancellationToken>()).Returns(page);

        var response = await _controller.GetPaged(2, 5, CancellationToken.None);

        var ok = response.Result.ShouldBeOfType<OkObjectResult>();
        ok.Value.ShouldBe(page);
    }

    [Fact]
    public async Task GetPaged_InvalidPaging_ReturnsBadRequestWithValidationErrors()
    {
        _service.GetPagedAsync(0, 10, Arg.Any<CancellationToken>())
            .Returns(Error.Validation(new Dictionary<string, string[]> { ["page"] = ["Invalid page."] }));

        var response = await _controller.GetPaged(0, 10, CancellationToken.None);

        AssertValidationProblem(response.Result, "page");
    }

    [Fact]
    public async Task GetById_ExistingProduct_ReturnsOkWithProduct()
    {
        _service.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(Product);

        var response = await _controller.GetById(1, CancellationToken.None);

        var ok = response.Result.ShouldBeOfType<OkObjectResult>();
        ok.Value.ShouldBe(Product);
    }

    [Fact]
    public async Task GetById_UnknownProduct_ReturnsNotFoundProblem()
    {
        _service.GetByIdAsync(99, Arg.Any<CancellationToken>()).Returns((ProductDto?)null);

        var response = await _controller.GetById(99, CancellationToken.None);

        AssertNotFoundProblem(response.Result);
    }

    [Fact]
    public async Task UpdateDescription_ValidRequest_ReturnsOkWithUpdatedProduct()
    {
        var updated = Product with { Description = "New description" };
        _service.UpdateDescriptionAsync(1, "New description", Arg.Any<CancellationToken>()).Returns(updated);

        var response = await _controller.UpdateDescription(
            1, new UpdateProductDescriptionRequest("New description"), CancellationToken.None);

        var ok = response.Result.ShouldBeOfType<OkObjectResult>();
        ok.Value.ShouldBe(updated);
    }

    [Fact]
    public async Task UpdateDescription_InvalidDescription_ReturnsBadRequestWithValidationErrors()
    {
        _service.UpdateDescriptionAsync(1, " ", Arg.Any<CancellationToken>())
            .Returns(Error.Validation(new Dictionary<string, string[]> { ["description"] = ["Invalid description."] }));

        var response = await _controller.UpdateDescription(
            1, new UpdateProductDescriptionRequest(" "), CancellationToken.None);

        AssertValidationProblem(response.Result, "description");
    }

    [Fact]
    public async Task UpdateDescription_UnknownProduct_ReturnsNotFoundProblem()
    {
        _service.UpdateDescriptionAsync(99, "New description", Arg.Any<CancellationToken>())
            .Returns(Error.NotFound("Product with id 99 was not found."));

        var response = await _controller.UpdateDescription(
            99, new UpdateProductDescriptionRequest("New description"), CancellationToken.None);

        AssertNotFoundProblem(response.Result);
    }

    private static void AssertValidationProblem(ActionResult? result, string expectedKey)
    {
        var objectResult = result.ShouldBeAssignableTo<ObjectResult>().ShouldNotBeNull();
        objectResult.StatusCode.ShouldBe(StatusCodes.Status400BadRequest);
        objectResult.Value.ShouldBeOfType<ValidationProblemDetails>().Errors.ShouldContainKey(expectedKey);
    }

    private static void AssertNotFoundProblem(ActionResult? result)
    {
        var objectResult = result.ShouldBeAssignableTo<ObjectResult>().ShouldNotBeNull();
        objectResult.StatusCode.ShouldBe(StatusCodes.Status404NotFound);
        objectResult.Value.ShouldBeOfType<ProblemDetails>().Status.ShouldBe(StatusCodes.Status404NotFound);
    }
}
