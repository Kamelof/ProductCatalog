using ProductCatalog.Application.Abstractions;
using ProductCatalog.Infrastructure.Data;

namespace ProductCatalog.Tests.Infrastructure;

public abstract class ProductRepositoryContractTests
{
    private static readonly int SeedCount = SeedData.Products.Count;

    protected abstract IProductRepository CreateRepository();

    [Fact]
    public async Task GetAllAsync_SeededData_ReturnsAllProductsOrderedById()
    {
        var products = await CreateRepository().GetAllAsync();

        products.Select(p => p.Id).ShouldBe(SeedData.Products.Select(p => p.Id).Order());
    }

    [Fact]
    public async Task CountAsync_SeededData_ReturnsNumberOfProducts()
    {
        var count = await CreateRepository().CountAsync();

        count.ShouldBe(SeedCount);
    }

    [Fact]
    public async Task GetPageAsync_MiddlePage_ReturnsSliceInIdOrder()
    {
        var products = await CreateRepository().GetPageAsync(skip: 10, take: 10);

        products.Select(p => p.Id).ShouldBe(Enumerable.Range(11, 10));
    }

    [Fact]
    public async Task GetPageAsync_LastPartialPage_ReturnsRemainingItems()
    {
        var products = await CreateRepository().GetPageAsync(skip: 20, take: 10);

        products.Count.ShouldBe(SeedCount - 20);
        products.Select(p => p.Id).ShouldBe(Enumerable.Range(21, SeedCount - 20));
    }

    [Fact]
    public async Task GetPageAsync_SkipBeyondEnd_ReturnsEmpty()
    {
        var products = await CreateRepository().GetPageAsync(skip: SeedCount, take: 10);

        products.ShouldBeEmpty();
    }

    [Fact]
    public async Task GetPageAsync_NegativeSkip_Throws()
    {
        var repository = CreateRepository();

        await Should.ThrowAsync<ArgumentOutOfRangeException>(async () => await repository.GetPageAsync(skip: -1, take: 10));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task GetPageAsync_NonPositiveTake_Throws(int take)
    {
        var repository = CreateRepository();

        await Should.ThrowAsync<ArgumentOutOfRangeException>(async () => await repository.GetPageAsync(skip: 0, take));
    }

    [Theory]
    [InlineData(12)]
    [InlineData(25)]
    [InlineData(27)]
    [InlineData(28)]
    public async Task GetByIdAsync_ExistingId_ReturnsProductWithAllFields(int id)
    {
        var expected = SeedData.Products.Single(p => p.Id == id);

        var product = await CreateRepository().GetByIdAsync(id);

        product.ShouldNotBeNull();
        product.Name.ShouldBe(expected.Name);
        product.ImgUri.ShouldBe(expected.ImgUri);
        product.Price.ShouldBe(expected.Price);
        product.Description.ShouldBe(expected.Description);
    }

    [Fact]
    public async Task GetByIdAsync_UnknownId_ReturnsNull()
    {
        var product = await CreateRepository().GetByIdAsync(99999);

        product.ShouldBeNull();
    }

    [Fact]
    public async Task UpdateAsync_ChangedDescription_IsVisibleOnReRead()
    {
        var repository = CreateRepository();
        var product = (await repository.GetByIdAsync(4)).ShouldNotBeNull();

        product.UpdateDescription("Updated description");
        await repository.UpdateAsync(product);

        var reloaded = await CreateRepository().GetByIdAsync(4);
        reloaded.ShouldNotBeNull().Description.ShouldBe("Updated description");
    }

    [Fact]
    public async Task GetByIdAsync_ModifiedWithoutUpdate_ChangeIsNotPersisted()
    {
        var product = (await CreateRepository().GetByIdAsync(4)).ShouldNotBeNull();
        var originalDescription = product.Description;

        product.UpdateDescription("Not saved");

        var reloaded = await CreateRepository().GetByIdAsync(4);
        reloaded.ShouldNotBeNull().Description.ShouldBe(originalDescription);
    }
}
