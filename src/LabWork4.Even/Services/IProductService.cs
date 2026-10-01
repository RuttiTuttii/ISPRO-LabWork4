using LabWork4.Even.DTOs;

namespace LabWork4.Even.Services;

public interface IProductService
{
    Task<ProductResponseDto> CreateProductAsync(CreateProductDto dto);
    Task<ProductResponseDto?> GetProductByIdAsync(int id);
    Task<List<ProductResponseDto>> GetAllProductsAsync();
    Task<ProductResponseDto> UpdateProductAsync(int id, UpdateProductDto dto);
    Task<bool> DeleteProductAsync(int id);
}
