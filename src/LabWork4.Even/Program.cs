using System.Globalization;
using LabWork4.Even.Data;
using LabWork4.Even.DTOs;
using LabWork4.Even.Exceptions;
using LabWork4.Even.Logging;
using LabWork4.Even.Services;

// настраиваем кодировку консоли и культуру
Console.OutputEncoding = System.Text.Encoding.UTF8;
CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;

// инициализируем бд и сервисы
using var db = new AppDbContext();
db.Database.EnsureCreated();

var customerService = new CustomerService(db, new ConsoleAppLogger("CustomerService"));
var productService = new ProductService(db, new ConsoleAppLogger("ProductService"));
var orderService = new OrderService(db, new ConsoleAppLogger("OrderService"));

// считываем номер сценария из аргументов или запрашиваем ввод
var task = args.FirstOrDefault();
if (string.IsNullOrEmpty(task))
{
    Console.WriteLine("1 - crud клиентов и товаров (задача 2)");
    Console.WriteLine("2 - обновление и удаление сущностей (задача 2)");
    Console.WriteLine("3 - исключение notfoundexception (задача 4)");
    Console.WriteLine("4 - исключение duplicateentityexception (задача 4)");
    Console.WriteLine("5 - заказ со списанием и отменой с возвратом на склад (задачи 2 и 4)");
    Console.WriteLine("all - запуск всех сценариев");
    Console.Write("\nсценарий (1, 2, 3, 4, 5, all) [по умолчанию all]: ");

    var input = Console.ReadLine()?.Trim();
    task = string.IsNullOrEmpty(input) ? "all" : input;
    Console.WriteLine();
}

switch (task.ToLower())
{
    case "1":
        await Scenario1_CreateAndRead(customerService, productService);
        break;

    case "2":
        await Scenario2_UpdateAndDelete(customerService, productService);
        break;

    case "3":
        await Scenario3_NotFoundException(customerService);
        break;

    case "4":
        await Scenario4_DuplicateException(customerService);
        break;

    case "5":
        await Scenario5_OrderStockFlow(customerService, productService, orderService);
        break;

    case "all":
    default:
        await Scenario1_CreateAndRead(customerService, productService);
        await Scenario2_UpdateAndDelete(customerService, productService);
        await Scenario3_NotFoundException(customerService);
        await Scenario4_DuplicateException(customerService);
        await Scenario5_OrderStockFlow(customerService, productService, orderService);
        break;
}

static async Task Scenario1_CreateAndRead(ICustomerService customerService, IProductService productService)
{
    Console.WriteLine("\n[сценарий 1] создание и чтение сущностей");

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

static async Task Scenario2_UpdateAndDelete(ICustomerService customerService, IProductService productService)
{
    Console.WriteLine("\n[сценарий 2] обновление и удаление");

    var product = await productService.CreateProductAsync(new CreateProductDto(
        "Временный товар для теста",
        $"SKU-TMP-{Random.Shared.Next(1000, 9999)}",
        1990.00m,
        5
    ));

    var updated = await productService.UpdateProductAsync(product.Id, new UpdateProductDto(
        "Обновленный товар (скидка)",
        1490.00m,
        10
    ));
    Console.WriteLine($"товар обновлен: {updated.Name}, цена: {updated.Price} ₽");

    var deleted = await productService.DeleteProductAsync(product.Id);
    Console.WriteLine($"товар удален: {deleted}");
}

static async Task Scenario3_NotFoundException(ICustomerService customerService)
{
    Console.WriteLine("\n[сценарий 3] перехват notfoundexception");
    try
    {
        await customerService.UpdateCustomerAsync(99999, new UpdateCustomerDto("Неизвестный", "+0"));
    }
    catch (NotFoundException ex)
    {
        Console.WriteLine($"перехвачено исключение: {ex.Message}");
    }
}

static async Task Scenario4_DuplicateException(ICustomerService customerService)
{
    Console.WriteLine("\n[сценарий 4] перехват duplicateentityexception");
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
    Console.WriteLine("\n[сценарий 5] жизненный цикл заказа и остатков");

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
