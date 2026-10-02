using System.Globalization;
using LabWork4.Data;
using LabWork4.DTOs;
using LabWork4.Exceptions;
using LabWork4.Filtering;
using LabWork4.Logging;
using LabWork4.Models;
using LabWork4.Services;
using Microsoft.EntityFrameworkCore;

// настраиваем кодировку консоли и культуру
Console.OutputEncoding = System.Text.Encoding.UTF8;
CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;

// инициализируем контекст бд и сервисы
using var db = new AppDbContext();
DbInitializer.Initialize(db);

var customerService = new CustomerService(db, new ConsoleAppLogger("CustomerService"));
var productService = new ProductService(db, new ConsoleAppLogger("ProductService"));
var orderService = new OrderService(db, new ConsoleAppLogger("OrderService"));

// считываем номер сценария из аргументов или запрашиваем ввод
var task = args.FirstOrDefault();
if (string.IsNullOrEmpty(task))
{
    Console.WriteLine("1 - сидирование бд и доменные модели (задача 1: Skverno-Slov)");
    Console.WriteLine("2 - crud операции в сервисах (задача 2: RuttiTuttii)");
    Console.WriteLine("3 - фильтрация, сортировка и пагинация (задача 3: Skverno-Slov)");
    Console.WriteLine("4 - перехват исключений и логирование (задача 4: RuttiTuttii)");
    Console.WriteLine("5 - жизненный цикл заказа и остатков (интеграция задач)");
    Console.WriteLine("all - запуск всех сценариев");
    Console.Write("\nсценарий (1, 2, 3, 4, 5, all) [по умолчанию all]: ");

    var input = Console.ReadLine()?.Trim();
    task = string.IsNullOrEmpty(input) ? "all" : input;
    Console.WriteLine();
}

switch (task.ToLower())
{
    case "1":
        await Scenario1_ModelsAndDb(db);
        break;

    case "2":
        await Scenario2_CrudServices(customerService, productService);
        break;

    case "3":
        await Scenario3_FilteringAndPagination(db);
        break;

    case "4":
        await Scenario4_ExceptionsAndLogging(customerService);
        break;

    case "5":
        await Scenario5_OrderStockFlow(customerService, productService, orderService);
        break;

    case "all":
    default:
        await Scenario1_ModelsAndDb(db);
        await Scenario2_CrudServices(customerService, productService);
        await Scenario3_FilteringAndPagination(db);
        await Scenario4_ExceptionsAndLogging(customerService);
        await Scenario5_OrderStockFlow(customerService, productService, orderService);
        break;
}

static async Task Scenario1_ModelsAndDb(AppDbContext db)
{
    Console.WriteLine("\n[сценарий 1] модели данных и сидирование бд (задача 1: Skverno-Slov)");
    var categoriesCount = await db.Categories.CountAsync();
    var productsCount = await db.Products.CountAsync();
    var customersCount = await db.Customers.CountAsync();
    Console.WriteLine($"категорий в бд: {categoriesCount}");
    Console.WriteLine($"товаров в бд: {productsCount}");
    Console.WriteLine($"клиентов в бд: {customersCount}");
}

static async Task Scenario2_CrudServices(ICustomerService customerService, IProductService productService)
{
    Console.WriteLine("\n[сценарий 2] crud операции в сервисах (задача 2: RuttiTuttii)");

    var customer = await customerService.CreateCustomerAsync(new CreateCustomerDto(
        "Дмитрий Васильев",
        $"dmitry_{Guid.NewGuid():N}@example.com",
        "+7 999 111-22-33"
    ));

    var product = await productService.CreateProductAsync(new CreateProductDto(
        "Игровая мышь Razer DeathAdder",
        $"SKU-RAZ-{Random.Shared.Next(1000, 9999)}",
        5490.00m,
        15
    ));

    var fetchedCustomer = await customerService.GetCustomerByIdAsync(customer.Id);
    var fetchedProduct = await productService.GetProductByIdAsync(product.Id);

    Console.WriteLine($"клиент: {fetchedCustomer?.Name} ({fetchedCustomer?.Email})");
    Console.WriteLine($"товар: {fetchedProduct?.Name} | sku: {fetchedProduct?.Sku} | цена: {fetchedProduct?.Price} ₽");
}

static async Task Scenario3_FilteringAndPagination(AppDbContext db)
{
    Console.WriteLine("\n[сценарий 3] динамическая фильтрация и пагинация (задача 3: Skverno-Slov)");

    var filter = new ProductFilter(SearchTerm: "Ноутбук", MinPrice: 80000m);
    var page = await db.Products
        .AsNoTracking()
        .ApplyFilter(filter)
        .ApplySort("price", sortDescending: true)
        .ToPagedResultAsync(pageNumber: 1, pageSize: 2);

    Console.WriteLine($"найдено товаров: {page.TotalCount} (страница {page.PageNumber} из {page.TotalPages})");
    foreach (var item in page.Items)
    {
        Console.WriteLine($" - {item.Name}: {item.Price:N0} ₽ (остаток: {item.StockQuantity} шт.)");
    }
}

static async Task Scenario4_ExceptionsAndLogging(ICustomerService customerService)
{
    Console.WriteLine("\n[сценарий 4] перехват исключений и логирование (задача 4: RuttiTuttii)");

    try
    {
        await customerService.UpdateCustomerAsync(99999, new UpdateCustomerDto("Неизвестный", "+0"));
    }
    catch (NotFoundException ex)
    {
        Console.WriteLine($"перехвачено исключение: {ex.Message}");
    }

    var email = $"dup_{Guid.NewGuid():N}@example.com";
    await customerService.CreateCustomerAsync(new CreateCustomerDto("Клиент 1", email, "+7 900 000-00-01"));

    try
    {
        await customerService.CreateCustomerAsync(new CreateCustomerDto("Клиент 2 (дубликат)", email, "+7 900 000-00-02"));
    }
    catch (DuplicateEntityException ex)
    {
        Console.WriteLine($"предотвращен дубликат: {ex.Message}");
    }
}

static async Task Scenario5_OrderStockFlow(ICustomerService customerService, IProductService productService, IOrderService orderService)
{
    Console.WriteLine("\n[сценарий 5] жизненный цикл заказа и остатков (интеграция модулей)");

    var customer = await customerService.CreateCustomerAsync(new CreateCustomerDto("Ольга Кузнецова", $"olga_{Guid.NewGuid():N}@example.com", "+7 911 222-33-44"));
    var product = await productService.CreateProductAsync(new CreateProductDto("SSD NVMe 1TB Samsung", $"SKU-SSD-{Random.Shared.Next(1000, 9999)}", 8990.00m, 5));

    Console.WriteLine($"начальный остаток: {product.StockQuantity} шт.");

    var order = await orderService.CreateOrderAsync(new CreateOrderDto(
        customer.Id,
        new List<CreateOrderItemDto> { new(product.Id, 2) }
    ));

    var prodAfterOrder = await productService.GetProductByIdAsync(product.Id);
    Console.WriteLine($"остаток после заказа (купили 2 шт.): {prodAfterOrder?.StockQuantity} шт.");

    var cancelled = await orderService.CancelOrderAsync(order.Id);
    var prodAfterCancel = await productService.GetProductByIdAsync(product.Id);
    Console.WriteLine($"остаток после отмены заказа: {prodAfterCancel?.StockQuantity} шт.");
}
