using LabWork4.Data;
using LabWork4.DTOs;
using LabWork4.Exceptions;
using LabWork4.Logging;
using LabWork4.Models;
using Microsoft.EntityFrameworkCore;

namespace LabWork4.Services;

public class OrderService : IOrderService
{
    private readonly AppDbContext _context;
    private readonly IAppLogger? _logger;

    public OrderService(AppDbContext context, IAppLogger? logger = null)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<OrderDto> CreateOrderAsync(CreateOrderDto dto)
    {
        _logger?.LogInfo($"Создание заказа для клиента ID={dto.CustomerId}");

        var customer = await _context.Customers.FindAsync(dto.CustomerId);
        if (customer == null)
        {
            _logger?.LogError($"сущность 'Customer' с ключом '{dto.CustomerId}' не найдена");
            throw new NotFoundException(nameof(Customer), dto.CustomerId);
        }

        if (dto.Items == null || dto.Items.Count == 0)
            throw new ValidationException(nameof(dto.Items), "заказ должен содержать хотя бы одну позицию");

        var order = new Order
        {
            CustomerId = customer.Id,
            Customer = customer,
            Status = OrderStatus.Processing
        };

        foreach (var itemDto in dto.Items)
        {
            if (itemDto.Quantity <= 0)
                throw new ValidationException(nameof(itemDto.Quantity), "количество товара должно быть больше 0");

            var product = await _context.Products.FindAsync(itemDto.ProductId);
            if (product == null)
                throw new NotFoundException(nameof(Product), itemDto.ProductId);

            if (product.StockQuantity < itemDto.Quantity)
            {
                throw new ValidationException(
                    nameof(product.StockQuantity),
                    $"недостаточно товара '{product.Name}' на складе (доступно: {product.StockQuantity}, запрошено: {itemDto.Quantity})");
            }

            product.StockQuantity -= itemDto.Quantity;
            _logger?.LogInfo($"Резервирование товара '{product.Name}': -{itemDto.Quantity} шт., остаток={product.StockQuantity}");

            order.Items.Add(new OrderItem
            {
                Order = order,
                ProductId = product.Id,
                Product = product,
                Quantity = itemDto.Quantity,
                UnitPrice = product.Price
            });
        }

        _context.Orders.Add(order);
        await _context.SaveChangesAsync();

        _logger?.LogInfo($"Заказ ID={order.Id} успешно оформлен на сумму {order.TotalAmount:N2} ₽");
        return ToDto(order);
    }

    public async Task<OrderDto?> GetOrderByIdAsync(int id)
    {
        var order = await _context.Orders
            .Include(o => o.Items)
            .ThenInclude(i => i.Product)
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == id);

        return order == null ? null : ToDto(order);
    }

    public async Task<List<OrderDto>> GetOrdersByCustomerAsync(int customerId)
    {
        var orders = await _context.Orders
            .Include(o => o.Items)
            .ThenInclude(i => i.Product)
            .Where(o => o.CustomerId == customerId)
            .AsNoTracking()
            .ToListAsync();

        return orders.Select(ToDto).ToList();
    }

    public async Task<bool> CancelOrderAsync(int orderId)
    {
        _logger?.LogInfo($"Отмена заказа ID={orderId}");
        var order = await _context.Orders
            .Include(o => o.Items)
            .ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(o => o.Id == orderId);

        if (order == null)
            return false;

        if (order.Status == OrderStatus.Cancelled)
            return false;

        foreach (var item in order.Items)
        {
            if (item.Product != null)
            {
                item.Product.StockQuantity += item.Quantity;
                _logger?.LogInfo($"Возврат остатков '{item.Product.Name}': +{item.Quantity} шт., остаток={item.Product.StockQuantity}");
            }
        }

        order.Status = OrderStatus.Cancelled;
        await _context.SaveChangesAsync();

        _logger?.LogInfo($"Заказ ID={orderId} успешно отменен");
        return true;
    }

    private static OrderDto ToDto(Order o) =>
        new(
            o.Id,
            o.CustomerId,
            o.Status,
            o.OrderDate,
            o.TotalAmount,
            o.Items.Select(i => new OrderItemDto(
                i.Id,
                i.ProductId,
                i.Product?.Name ?? $"Товар #{i.ProductId}",
                i.Quantity,
                i.UnitPrice,
                i.UnitPrice * i.Quantity
            )).ToList()
        );
}
