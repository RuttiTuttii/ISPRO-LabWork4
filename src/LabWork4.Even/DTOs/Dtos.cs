using LabWork4.Even.Models;

namespace LabWork4.Even.DTOs;

public record CreateCustomerDto(string Name, string Email, string Phone);
public record UpdateCustomerDto(string Name, string Phone);
public record CustomerResponseDto(int Id, string Name, string Email, string Phone, DateTime CreatedAt);

public record CreateProductDto(string Name, string Sku, decimal Price, int StockQuantity);
public record UpdateProductDto(string Name, decimal Price, int StockQuantity);
public record ProductResponseDto(int Id, string Name, string Sku, decimal Price, int StockQuantity);

public record CreateOrderItemDto(int ProductId, int Quantity);
public record CreateOrderDto(int CustomerId, List<CreateOrderItemDto> Items);
public record OrderResponseDto(int Id, int CustomerId, string CustomerName, OrderStatus Status, decimal TotalAmount, DateTime OrderDate);
