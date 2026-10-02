using LabWork4.Data;
using LabWork4.DTOs;
using LabWork4.Exceptions;
using LabWork4.Logging;
using LabWork4.Models;
using Microsoft.EntityFrameworkCore;

namespace LabWork4.Services;

public class ProductService : IProductService
{
    private readonly AppDbContext _context;
    private readonly IAppLogger? _logger;

    public ProductService(AppDbContext context, IAppLogger? logger = null)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<ProductDto> CreateProductAsync(CreateProductDto dto)
    {
        _logger?.LogInfo($"Создание нового товара: '{dto.Name}' (SKU: {dto.Sku})");

        if (string.IsNullOrWhiteSpace(dto.Name))
            throw new ValidationException(nameof(dto.Name), "название товара не может быть пустым");

        if (string.IsNullOrWhiteSpace(dto.Sku))
            throw new ValidationException(nameof(dto.Sku), "артикул (SKU) обязателен");

        if (dto.Price < 0)
            throw new ValidationException(nameof(dto.Price), "цена товара не может быть отрицательной");

        if (dto.StockQuantity < 0)
            throw new ValidationException(nameof(dto.StockQuantity), "количество на складе не может быть отрицательным");

        var normalizedSku = dto.Sku.Trim().ToUpperInvariant();
        var exists = await _context.Products.AnyAsync(p => p.Sku == normalizedSku);
        if (exists)
        {
            _logger?.LogWarning($"сущность 'Product' с полем 'Sku'='{dto.Sku}' уже существует");
            throw new DuplicateEntityException(nameof(Product), nameof(Product.Sku), dto.Sku);
        }

        var product = new Product
        {
            Name = dto.Name.Trim(),
            Sku = normalizedSku,
            Price = dto.Price,
            StockQuantity = dto.StockQuantity,
            CategoryId = dto.CategoryId
        };

        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        _logger?.LogInfo($"Товар '{product.Name}' сохранен с ID={product.Id}, остаток={product.StockQuantity}");
        return ToDto(product);
    }

    public async Task<ProductDto?> GetProductByIdAsync(int id)
    {
        _logger?.LogInfo($"Поиск товара по ID={id}");
        var product = await _context.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);
        return product == null ? null : ToDto(product);
    }

    public async Task<List<ProductDto>> GetAllProductsAsync()
    {
        var products = await _context.Products.AsNoTracking().ToListAsync();
        return products.Select(ToDto).ToList();
    }

    public async Task<ProductDto> UpdateProductAsync(int id, UpdateProductDto dto)
    {
        _logger?.LogInfo($"Обновление товара ID={id}");
        var product = await _context.Products.FindAsync(id);
        if (product == null)
        {
            _logger?.LogError($"сущность 'Product' с ключом '{id}' не найдена");
            throw new NotFoundException(nameof(Product), id);
        }

        if (string.IsNullOrWhiteSpace(dto.Name))
            throw new ValidationException(nameof(dto.Name), "название товара не может быть пустым");

        if (dto.Price < 0)
            throw new ValidationException(nameof(dto.Price), "цена товара не может быть отрицательной");

        if (dto.StockQuantity < 0)
            throw new ValidationException(nameof(dto.StockQuantity), "количество на складе не может быть отрицательным");

        product.Name = dto.Name.Trim();
        product.Price = dto.Price;
        product.StockQuantity = dto.StockQuantity;
        if (dto.CategoryId.HasValue)
            product.CategoryId = dto.CategoryId;

        await _context.SaveChangesAsync();
        _logger?.LogInfo($"Товар ID={id} успешно обновлен");
        return ToDto(product);
    }

    public async Task<bool> DeleteProductAsync(int id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null)
            return false;

        _context.Products.Remove(product);
        await _context.SaveChangesAsync();
        _logger?.LogInfo($"Товар ID={id} успешно удален");
        return true;
    }

    private static ProductDto ToDto(Product p) =>
        new(p.Id, p.Name, p.Sku, p.Price, p.StockQuantity, p.CategoryId);
}
