using LabWork4.Even.Data;
using LabWork4.Even.DTOs;
using LabWork4.Even.Exceptions;
using LabWork4.Even.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace LabWork4.Even.Tests;

public class ProductServiceTests
{
    private AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    [Fact]
    public async Task CreateProductAsync_CreatesProduct_WhenValid()
    {
        using var db = CreateContext();
        var service = new ProductService(db);

        var dto = new CreateProductDto("Монитор LG 27", "LG-27-4K", 32000m, 10);
        var product = await service.CreateProductAsync(dto);

        Assert.True(product.Id > 0);
        Assert.Equal("LG-27-4K", product.Sku);
        Assert.Equal(32000m, product.Price);
    }

    [Fact]
    public async Task CreateProductAsync_ThrowsDuplicateEntityException_OnSameSku()
    {
        using var db = CreateContext();
        var service = new ProductService(db);

        await service.CreateProductAsync(new CreateProductDto("Товар А", "SKU-DUP", 100m, 5));

        await Assert.ThrowsAsync<DuplicateEntityException>(() =>
            service.CreateProductAsync(new CreateProductDto("Товар Б", "SKU-DUP", 200m, 10)));
    }

    [Fact]
    public async Task CreateProductAsync_ThrowsValidationException_WhenPriceIsNegative()
    {
        using var db = CreateContext();
        var service = new ProductService(db);

        var dto = new CreateProductDto("Товар с минусовой ценой", "SKU-NEG", -10m, 5);
        await Assert.ThrowsAsync<ValidationException>(() => service.CreateProductAsync(dto));
    }

    [Fact]
    public async Task UpdateProductAsync_UpdatesValues_WhenFound()
    {
        using var db = CreateContext();
        var service = new ProductService(db);

        var created = await service.CreateProductAsync(new CreateProductDto("Мышь", "SKU-MOU", 1000m, 5));
        var updated = await service.UpdateProductAsync(created.Id, new UpdateProductDto("Мышь оптическая", 1200m, 8));

        Assert.Equal("Мышь оптическая", updated.Name);
        Assert.Equal(1200m, updated.Price);
        Assert.Equal(8, updated.StockQuantity);
    }
}
