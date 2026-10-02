using LabWork4.DTOs;

namespace LabWork4.Services;

public interface ICustomerService
{
    Task<CustomerDto> CreateCustomerAsync(CreateCustomerDto dto);
    Task<CustomerDto?> GetCustomerByIdAsync(int id);
    Task<List<CustomerDto>> GetAllCustomersAsync();
    Task<CustomerDto> UpdateCustomerAsync(int id, UpdateCustomerDto dto);
    Task<bool> DeleteCustomerAsync(int id);
}

public interface IProductService
{
    Task<ProductDto> CreateProductAsync(CreateProductDto dto);
    Task<ProductDto?> GetProductByIdAsync(int id);
    Task<List<ProductDto>> GetAllProductsAsync();
    Task<ProductDto> UpdateProductAsync(int id, UpdateProductDto dto);
    Task<bool> DeleteProductAsync(int id);
}

public interface IOrderService
{
    Task<OrderDto> CreateOrderAsync(CreateOrderDto dto);
    Task<OrderDto?> GetOrderByIdAsync(int id);
    Task<List<OrderDto>> GetOrdersByCustomerAsync(int customerId);
    Task<bool> CancelOrderAsync(int orderId);
}
