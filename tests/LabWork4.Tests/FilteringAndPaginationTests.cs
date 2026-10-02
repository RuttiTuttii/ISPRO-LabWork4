using LabWork4.Data;
using LabWork4.Filtering;
using LabWork4.Models;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace LabWork4.Tests;

public class FilteringAndPaginationTests
{
    private AppDbContext CreateSeededContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        var context = new AppDbContext(options);

        var catA = new Category { Name = "Электроника" };
        var catB = new Category { Name = "Бытовая техника" };
        context.Categories.AddRange(catA, catB);
        context.SaveChanges();

        var products = new List<Product>
        {
            new() { Name = "Ноутбук Pro", Sku = "SKU-1", Price = 100000m, StockQuantity = 5, CategoryId = catA.Id },
            new() { Name = "Ноутбук Air", Sku = "SKU-2", Price = 70000m, StockQuantity = 0, CategoryId = catA.Id },
            new() { Name = "Смартфон Ultra", Sku = "SKU-3", Price = 80000m, StockQuantity = 10, CategoryId = catA.Id },
            new() { Name = "Чайник электрический", Sku = "SKU-4", Price = 3000m, StockQuantity = 15, CategoryId = catB.Id },
            new() { Name = "Пылесос робот", Sku = "SKU-5", Price = 25000m, StockQuantity = 8, CategoryId = catB.Id }
        };
        context.Products.AddRange(products);
        context.SaveChanges();

        return context;
    }

    [Fact]
    public void ApplyFilter_FiltersBySearchTerm()
    {
        using var context = CreateSeededContext();
        var filter = new ProductFilter(SearchTerm: "ноутбук");

        var result = context.Products.ApplyFilter(filter).ToList();

        Assert.Equal(2, result.Count);
        Assert.All(result, p => Assert.Contains("ноутбук", p.Name, StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void ApplyFilter_FiltersByPriceRange()
    {
        using var context = CreateSeededContext();
        var filter = new ProductFilter(MinPrice: 20000m, MaxPrice: 75000m);

        var result = context.Products.ApplyFilter(filter).ToList();

        Assert.Equal(2, result.Count); // Ноутбук Air (70000) и Пылесос (25000)
    }

    [Fact]
    public void ApplyFilter_FiltersByInStockOnly()
    {
        using var context = CreateSeededContext();
        var filter = new ProductFilter(InStockOnly: true);

        var result = context.Products.ApplyFilter(filter).ToList();

        Assert.Equal(4, result.Count);
        Assert.DoesNotContain(result, p => p.StockQuantity == 0);
    }

    [Fact]
    public void ApplySort_SortsAscendingAndDescending()
    {
        using var context = CreateSeededContext();

        var desc = context.Products.ApplySort("price", sortDescending: true).ToList();
        Assert.Equal(100000m, desc.First().Price);

        var asc = context.Products.ApplySort("price", sortDescending: false).ToList();
        Assert.Equal(3000m, asc.First().Price);
    }

    [Fact]
    public async Task ToPagedResultAsync_PaginatesCorrectly()
    {
        using var context = CreateSeededContext();

        var paged = await context.Products.OrderBy(p => p.Id).ToPagedResultAsync(pageNumber: 1, pageSize: 2);

        Assert.Equal(5, paged.TotalCount);
        Assert.Equal(3, paged.TotalPages);
        Assert.Equal(2, paged.Items.Count);
        Assert.True(paged.HasNextPage);
        Assert.False(paged.HasPreviousPage);
    }
}
