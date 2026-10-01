using LabWork4.Even.DTOs;
using LabWork4.Even.Models;

namespace LabWork4.Even.Services;

public interface IOrderService
{
    Task<OrderResponseDto> CreateOrderAsync(CreateOrderDto dto);
    Task<OrderResponseDto?> GetOrderByIdAsync(int id);
    Task<List<OrderResponseDto>> GetOrdersByCustomerAsync(int customerId);
    Task<bool> UpdateOrderStatusAsync(int id, OrderStatus newStatus);
    Task<bool> CancelOrderAsync(int id);
}
