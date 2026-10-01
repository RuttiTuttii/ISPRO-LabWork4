using LabWork4.Even.Data;
using LabWork4.Even.DTOs;
using LabWork4.Even.Exceptions;
using LabWork4.Even.Models;
using LabWork4.Even.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace LabWork4.Even.Tests;

public class OrderServiceTests
{
    private (AppDbContext Context, Customer Customer, Product Product) CreateSeededContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        var context = new AppDbContext(options);

        var customer = new Customer { Name = "Анна", Email = "anna@test.com", Phone = "+7" };
        var product = new Product { Name = "Клавиатура", Sku = "SKU-KEY", Price = 3000m, StockQuantity = 10 };

        context.Customers.Add(customer);
        context.Products.Add(product);
        context.SaveChanges();

        return (context, customer, product);
    }

    [Fact]
    public async Task CreateOrderAsync_DecreasesProductStock_AndReturnsOrder()
    {
        var (context, customer, product) = CreateSeededContext();
        using (context)
        {
            var service = new OrderService(context);

            var dto = new CreateOrderDto(customer.Id, new List<CreateOrderItemDto>
            {
                new(product.Id, 3)
            });

            var order = await service.CreateOrderAsync(dto);

            Assert.True(order.Id > 0);
            Assert.Equal(9000m, order.TotalAmount);

            var productInDb = await context.Products.FindAsync(product.Id);
            Assert.Equal(7, productInDb!.StockQuantity); // 10 - 3 = 7
        }
    }

    [Fact]
    public async Task CreateOrderAsync_ThrowsValidationException_WhenStockIsInsufficient()
    {
        var (context, customer, product) = CreateSeededContext();
        using (context)
        {
            var service = new OrderService(context);

            var dto = new CreateOrderDto(customer.Id, new List<CreateOrderItemDto>
            {
                new(product.Id, 999) // больше чем 10
            });

            await Assert.ThrowsAsync<ValidationException>(() => service.CreateOrderAsync(dto));
        }
    }

    [Fact]
    public async Task CancelOrderAsync_RestoresProductStock()
    {
        var (context, customer, product) = CreateSeededContext();
        using (context)
        {
            var service = new OrderService(context);

            var dto = new CreateOrderDto(customer.Id, new List<CreateOrderItemDto>
            {
                new(product.Id, 4)
            });

            var order = await service.CreateOrderAsync(dto);

            var cancelled = await service.CancelOrderAsync(order.Id);
            Assert.True(cancelled);

            var productInDb = await context.Products.FindAsync(product.Id);
            Assert.Equal(10, productInDb!.StockQuantity); // остаток вернулся к 10
        }
    }
}
