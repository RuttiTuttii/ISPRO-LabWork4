using LabWork4.Models;

namespace LabWork4.DTOs;

public record CreateCustomerDto(string Name, string Email, string? Phone);
public record UpdateCustomerDto(string Name, string? Phone);
public record CustomerDto(int Id, string Name, string Email, string Phone, DateTime CreatedAt);

public record CreateProductDto(string Name, string Sku, decimal Price, int StockQuantity, int? CategoryId = null);
public record UpdateProductDto(string Name, decimal Price, int StockQuantity, int? CategoryId = null);
public record ProductDto(int Id, string Name, string Sku, decimal Price, int StockQuantity, int? CategoryId);

public record CreateOrderItemDto(int ProductId, int Quantity);
public record CreateOrderDto(int CustomerId, List<CreateOrderItemDto> Items);
public record OrderItemDto(int Id, int ProductId, string ProductName, int Quantity, decimal UnitPrice, decimal Total);
public record OrderDto(int Id, int CustomerId, OrderStatus Status, DateTime OrderDate, decimal TotalAmount, List<OrderItemDto> Items);
