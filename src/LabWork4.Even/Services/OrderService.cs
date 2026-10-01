using LabWork4.Even.Data;
using LabWork4.Even.DTOs;
using LabWork4.Even.Exceptions;
using LabWork4.Even.Logging;
using LabWork4.Even.Models;
using Microsoft.EntityFrameworkCore;

namespace LabWork4.Even.Services;

public class OrderService : IOrderService
{
    private readonly AppDbContext _context;
    private readonly IAppLogger _logger;

    public OrderService(AppDbContext context, IAppLogger? logger = null)
    {
        _context = context;
        _logger = logger ?? new ConsoleAppLogger(nameof(OrderService));
    }

    public async Task<OrderResponseDto> CreateOrderAsync(CreateOrderDto dto)
    {
        _logger.LogInfo($"Создание заказа для клиента ID={dto.CustomerId}");

        var customer = await _context.Customers.FindAsync(dto.CustomerId);
        if (customer == null)
        {
            var ex = new NotFoundException("Customer", dto.CustomerId);
            _logger.LogError(ex.Message);
            throw ex;
        }

        if (dto.Items == null || dto.Items.Count == 0)
            throw new ValidationException(nameof(dto.Items), "заказ должен содержать хотя бы одну позицию");

        var order = new Order
        {
            CustomerId = customer.Id,
            Customer = customer,
            Status = OrderStatus.Pending,
            OrderDate = DateTime.UtcNow
        };

        foreach (var itemDto in dto.Items)
        {
            if (itemDto.Quantity <= 0)
                throw new ValidationException(nameof(itemDto.Quantity), "количество товара должно быть больше нуля");

            var product = await _context.Products.FindAsync(itemDto.ProductId);
            if (product == null)
            {
                var ex = new NotFoundException("Product", itemDto.ProductId);
                _logger.LogError(ex.Message);
                throw ex;
            }

            if (product.StockQuantity < itemDto.Quantity)
            {
                var ex = new ValidationException(nameof(product.StockQuantity),
                    $"недостаточно товара '{product.Name}' на складе (остаток: {product.StockQuantity}, запрошено: {itemDto.Quantity})");
                _logger.LogWarning(ex.Message);
                throw ex;
            }

            // резервируем остатки
            product.StockQuantity -= itemDto.Quantity;
            _logger.LogInfo($"Резервирование товара '{product.Name}': -{itemDto.Quantity} шт., остаток={product.StockQuantity}");

            order.Items.Add(new OrderItem
            {
                Product = product,
                ProductId = product.Id,
                Quantity = itemDto.Quantity,
                UnitPrice = product.Price
            });
        }

        _context.Orders.Add(order);
        await _context.SaveChangesAsync();

        _logger.LogInfo($"Заказ ID={order.Id} успешно оформлен на сумму {order.TotalAmount:N2} ₽");
        return new OrderResponseDto(order.Id, customer.Id, customer.Name, order.Status, order.TotalAmount, order.OrderDate);
    }

    public async Task<OrderResponseDto?> GetOrderByIdAsync(int id)
    {
        _logger.LogInfo($"Запрос данных заказа ID={id}");
        var order = await _context.Orders
            .Include(o => o.Customer)
            .Include(o => o.Items)
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == id);

        return order == null
            ? null
            : new OrderResponseDto(order.Id, order.CustomerId, order.Customer.Name, order.Status, order.TotalAmount, order.OrderDate);
    }

    public async Task<List<OrderResponseDto>> GetOrdersByCustomerAsync(int customerId)
    {
        _logger.LogInfo($"Запрос заказов клиента ID={customerId}");
        return await _context.Orders
            .Where(o => o.CustomerId == customerId)
            .Include(o => o.Customer)
            .Include(o => o.Items)
            .AsNoTracking()
            .Select(o => new OrderResponseDto(o.Id, o.CustomerId, o.Customer.Name, o.Status, o.TotalAmount, o.OrderDate))
            .ToListAsync();
    }

    public async Task<bool> UpdateOrderStatusAsync(int id, OrderStatus newStatus)
    {
        _logger.LogInfo($"Обновление статуса заказа ID={id} на {newStatus}");
        var order = await _context.Orders.FindAsync(id);
        if (order == null)
        {
            _logger.LogWarning($"Заказ ID={id} не найден");
            return false;
        }

        order.Status = newStatus;
        await _context.SaveChangesAsync();
        _logger.LogInfo($"Статус заказа ID={id} обновлен на {newStatus}");
        return true;
    }

    public async Task<bool> CancelOrderAsync(int id)
    {
        _logger.LogInfo($"Отмена заказа ID={id}");
        var order = await _context.Orders
            .Include(o => o.Items)
            .ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order == null || order.Status == OrderStatus.Cancelled)
        {
            _logger.LogWarning($"Заказ ID={id} не может быть отменен (не найден или уже отменен)");
            return false;
        }

        // возвращаем остатки на склад
        foreach (var item in order.Items)
        {
            item.Product.StockQuantity += item.Quantity;
            _logger.LogInfo($"Возврат остатков '{item.Product.Name}': +{item.Quantity} шт., остаток={item.Product.StockQuantity}");
        }

        order.Status = OrderStatus.Cancelled;
        await _context.SaveChangesAsync();
        _logger.LogInfo($"Заказ ID={id} успешно отменен");
        return true;
    }
}
