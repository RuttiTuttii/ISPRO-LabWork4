using LabWork4.Models;

namespace LabWork4.Data;

public static class DbInitializer
{
    public static void Initialize(AppDbContext context)
    {
        context.Database.EnsureCreated();

        if (context.Categories.Any())
            return;

        var catLaptops = new Category { Name = "Ноутбуки", Description = "Портативные компьютеры и ультрабуки" };
        var catPhones = new Category { Name = "Смартфоны", Description = "Мобильные телефоны и аксессуары" };
        var catPeripherals = new Category { Name = "Периферия", Description = "Клавиатуры, мыши, гарнитуры" };

        context.Categories.AddRange(catLaptops, catPhones, catPeripherals);
        context.SaveChanges();

        var products = new List<Product>
        {
            new() { Name = "Ноутбук ASUS ZenBook 14", Sku = "SKU-NB-ASUS", Price = 89990.00m, StockQuantity = 12, Category = catLaptops },
            new() { Name = "Ноутбук Apple MacBook Air M3", Sku = "SKU-NB-MAC", Price = 129990.00m, StockQuantity = 8, Category = catLaptops },
            new() { Name = "Смартфон Google Pixel 9 Pro", Sku = "SKU-PH-PIX", Price = 98990.00m, StockQuantity = 15, Category = catPhones },
            new() { Name = "Смартфон Samsung Galaxy S25", Sku = "SKU-PH-SAM", Price = 91990.00m, StockQuantity = 20, Category = catPhones },
            new() { Name = "Механическая клавиатура Keychron K2", Sku = "SKU-KB-KEY", Price = 9500.00m, StockQuantity = 30, Category = catPeripherals },
            new() { Name = "Беспроводная мышь Logitech MX Master 3S", Sku = "SKU-MS-LOG", Price = 11200.00m, StockQuantity = 25, Category = catPeripherals },
            new() { Name = "Монитор Dell UltraSharp 27 4K", Sku = "SKU-MN-DEL", Price = 54900.00m, StockQuantity = 7, Category = catPeripherals }
        };

        context.Products.AddRange(products);

        var alice = new Customer { Name = "Алиса Смирнова", Email = "alice@example.com", Phone = "+7 999 100-20-30" };
        var bob = new Customer { Name = "Борис Кузнецов", Email = "bob@example.com", Phone = "+7 999 200-30-40" };

        context.Customers.AddRange(alice, bob);
        context.SaveChanges();

        var order1 = new Order
        {
            Customer = alice,
            Status = OrderStatus.Processing,
            Items = new List<OrderItem>
            {
                new() { Product = products[0], Quantity = 1, UnitPrice = products[0].Price },
                new() { Product = products[4], Quantity = 1, UnitPrice = products[4].Price }
            }
        };

        context.Orders.Add(order1);
        context.SaveChanges();
    }
}
