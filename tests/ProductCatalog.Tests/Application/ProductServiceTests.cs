using ProductCatalog.Application.Abstractions;
using ProductCatalog.Application.Common;
using ProductCatalog.Application.Products;
using ProductCatalog.Domain.Entities;

namespace ProductCatalog.Tests.Application;

public class ProductServiceTests
{
    private readonly IProductRepository _repository = Substitute.For<IProductRepository>();
    private readonly ProductService _service;

    public ProductServiceTests()
    {
        _service = new ProductService(_repository);
    }

    [Fact]
    public async Task GetAllAsync_ProductsExist_ReturnsAllProductsMappedToDtos()
    {
        var products = CreateProducts(3);
        _repository.GetAllAsync(Arg.Any<CancellationToken>()).Returns(products);

        var result = await _service.GetAllAsync();

        result.ShouldBe(products.Select(p => new ProductDto(p.Id, p.Name, p.ImgUri, p.Price, p.Description)));
    }

    [Theory]
    [InlineData(1, 10, 0)]
    [InlineData(2, 10, 10)]
    [InlineData(3, 5, 10)]
    public async Task GetPagedAsync_ValidPage_RequestsMatchingSliceFromRepository(int page, int pageSize, int expectedSkip)
    {
        _repository.CountAsync(Arg.Any<CancellationToken>()).Returns(100);
        _repository.GetPageAsync(expectedSkip, pageSize, Arg.Any<CancellationToken>()).Returns(CreateProducts(pageSize));

        var result = await _service.GetPagedAsync(page, pageSize);

        var paged = result.Value.ShouldNotBeNull();
        paged.Items.Count.ShouldBe(pageSize);
        paged.Page.ShouldBe(page);
        paged.PageSize.ShouldBe(pageSize);
        paged.TotalCount.ShouldBe(100);
        await _repository.Received(1).GetPageAsync(expectedSkip, pageSize, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetPagedAsync_PartialLastPage_ReturnsRemainingItems()
    {
        _repository.CountAsync(Arg.Any<CancellationToken>()).Returns(25);
        _repository.GetPageAsync(20, 10, Arg.Any<CancellationToken>()).Returns(CreateProducts(5));

        var result = await _service.GetPagedAsync(3, 10);

        var paged = result.Value.ShouldNotBeNull();
        paged.Items.Count.ShouldBe(5);
        paged.TotalCount.ShouldBe(25);
        paged.TotalPages.ShouldBe(3);
    }

    [Fact]
    public async Task GetPagedAsync_PageBeyondEnd_ReturnsEmptyItems()
    {
        _repository.CountAsync(Arg.Any<CancellationToken>()).Returns(25);

        var result = await _service.GetPagedAsync(4, 10);

        var paged = result.Value.ShouldNotBeNull();
        paged.Items.ShouldBeEmpty();
        paged.TotalCount.ShouldBe(25);
        paged.TotalPages.ShouldBe(3);
        await _repository.DidNotReceive().GetPageAsync(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetPagedAsync_HugePageNumber_ReturnsEmptyItems()
    {
        _repository.CountAsync(Arg.Any<CancellationToken>()).Returns(25);

        var result = await _service.GetPagedAsync(int.MaxValue, Paging.MaxPageSize);

        result.Value.ShouldNotBeNull().Items.ShouldBeEmpty();
    }

    [Theory]
    [InlineData(25, 10, 3)]
    [InlineData(30, 10, 3)]
    [InlineData(31, 10, 4)]
    [InlineData(1, 100, 1)]
    [InlineData(0, 10, 0)]
    public async Task GetPagedAsync_TotalCount_CalculatesTotalPages(int totalCount, int pageSize, int expectedTotalPages)
    {
        _repository.CountAsync(Arg.Any<CancellationToken>()).Returns(totalCount);
        _repository.GetPageAsync(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>()).Returns(CreateProducts(1));

        var result = await _service.GetPagedAsync(1, pageSize);

        result.Value.ShouldNotBeNull().TotalPages.ShouldBe(expectedTotalPages);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task GetPagedAsync_PageLessThanOne_ReturnsValidationError(int page)
    {
        var result = await _service.GetPagedAsync(page, 10);

        AssertValidationError(result.Error, "page");
        await _repository.DidNotReceive().CountAsync(Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    [InlineData(Paging.MaxPageSize + 1)]
    public async Task GetPagedAsync_PageSizeOutOfRange_ReturnsValidationError(int pageSize)
    {
        var result = await _service.GetPagedAsync(1, pageSize);

        AssertValidationError(result.Error, "pageSize");
        await _repository.DidNotReceive().CountAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetPagedAsync_MaxPageSize_IsAccepted()
    {
        _repository.CountAsync(Arg.Any<CancellationToken>()).Returns(0);

        var result = await _service.GetPagedAsync(1, Paging.MaxPageSize);

        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public async Task GetByIdAsync_ExistingProduct_ReturnsProduct()
    {
        var product = CreateProduct(7);
        _repository.GetByIdAsync(7, Arg.Any<CancellationToken>()).Returns(product);

        var result = await _service.GetByIdAsync(7);

        result.ShouldBe(new ProductDto(product.Id, product.Name, product.ImgUri, product.Price, product.Description));
    }

    [Fact]
    public async Task GetByIdAsync_UnknownProduct_ReturnsNull()
    {
        _repository.GetByIdAsync(99, Arg.Any<CancellationToken>()).Returns((Product?)null);

        var result = await _service.GetByIdAsync(99);

        result.ShouldBeNull();
    }

    [Fact]
    public async Task UpdateDescriptionAsync_ExistingProduct_ChangesOnlyDescriptionAndSaves()
    {
        var product = new Product(1, "Phone", "https://example.com/img/1.jpg", 999.90m, "Old description");
        _repository.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(product);

        var result = await _service.UpdateDescriptionAsync(1, "New description");

        result.Value.ShouldBe(new ProductDto(1, "Phone", "https://example.com/img/1.jpg", 999.90m, "New description"));
        await _repository.Received(1).UpdateAsync(
            Arg.Is<Product>(p =>
                p.Id == 1 &&
                p.Name == "Phone" &&
                p.ImgUri == "https://example.com/img/1.jpg" &&
                p.Price == 999.90m &&
                p.Description == "New description"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateDescriptionAsync_UnknownProduct_ReturnsNotFoundAndDoesNotSave()
    {
        _repository.GetByIdAsync(99, Arg.Any<CancellationToken>()).Returns((Product?)null);

        var result = await _service.UpdateDescriptionAsync(99, "New description");

        result.Error.ShouldNotBeNull().Type.ShouldBe(ErrorType.NotFound);
        await _repository.DidNotReceive().UpdateAsync(Arg.Any<Product>(), Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\t\n")]
    public async Task UpdateDescriptionAsync_MissingOrWhitespaceDescription_ReturnsValidationErrorAndDoesNotSave(string? description)
    {
        var result = await _service.UpdateDescriptionAsync(1, description);

        AssertValidationError(result.Error, "description");
        await _repository.DidNotReceive().UpdateAsync(Arg.Any<Product>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateDescriptionAsync_TooLongDescription_ReturnsValidationErrorAndDoesNotSave()
    {
        var description = new string('a', Product.DescriptionMaxLength + 1);

        var result = await _service.UpdateDescriptionAsync(1, description);

        AssertValidationError(result.Error, "description");
        await _repository.DidNotReceive().UpdateAsync(Arg.Any<Product>(), Arg.Any<CancellationToken>());
    }

    private static void AssertValidationError(Error? error, string expectedKey)
    {
        var actual = error.ShouldNotBeNull();
        actual.Type.ShouldBe(ErrorType.Validation);
        actual.ValidationErrors.ShouldNotBeNull().ShouldContainKey(expectedKey);
    }

    private static Product CreateProduct(int id) =>
        new(id, $"Product {id}", $"https://example.com/img/{id}.jpg", id * 100m, $"Description {id}");

    private static List<Product> CreateProducts(int count) =>
        Enumerable.Range(1, count).Select(CreateProduct).ToList();
}
