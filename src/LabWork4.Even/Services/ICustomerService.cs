using LabWork4.Even.DTOs;

namespace LabWork4.Even.Services;

public interface ICustomerService
{
    Task<CustomerResponseDto> CreateCustomerAsync(CreateCustomerDto dto);
    Task<CustomerResponseDto?> GetCustomerByIdAsync(int id);
    Task<List<CustomerResponseDto>> GetAllCustomersAsync();
    Task<CustomerResponseDto> UpdateCustomerAsync(int id, UpdateCustomerDto dto);
    Task<bool> DeleteCustomerAsync(int id);
}
