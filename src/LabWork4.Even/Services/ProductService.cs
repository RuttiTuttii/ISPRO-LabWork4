using LabWork4.Even.Data;
using LabWork4.Even.DTOs;
using LabWork4.Even.Exceptions;
using LabWork4.Even.Logging;
using LabWork4.Even.Models;
using Microsoft.EntityFrameworkCore;

namespace LabWork4.Even.Services;

public class ProductService : IProductService
{
    private readonly AppDbContext _context;
    private readonly IAppLogger _logger;

    public ProductService(AppDbContext context, IAppLogger? logger = null)
    {
        _context = context;
        _logger = logger ?? new ConsoleAppLogger(nameof(ProductService));
    }

    public async Task<ProductResponseDto> CreateProductAsync(CreateProductDto dto)
    {
        _logger.LogInfo($"Создание нового товара: '{dto.Name}' (SKU: {dto.Sku})");

        if (string.IsNullOrWhiteSpace(dto.Name))
            throw new ValidationException(nameof(dto.Name), "название товара не может быть пустым");

        if (string.IsNullOrWhiteSpace(dto.Sku))
            throw new ValidationException(nameof(dto.Sku), "артикул (SKU) обязателен");

        if (dto.Price < 0)
            throw new ValidationException(nameof(dto.Price), "цена товара не может быть отрицательной");

        var normalizedSku = dto.Sku.Trim().ToUpperInvariant();
        var exists = await _context.Products.AnyAsync(p => p.Sku == normalizedSku);
        if (exists)
        {
            var ex = new DuplicateEntityException("Product", nameof(dto.Sku), normalizedSku);
            _logger.LogWarning(ex.Message);
            throw ex;
        }

        var product = new Product
        {
            Name = dto.Name.Trim(),
            Sku = normalizedSku,
            Price = dto.Price,
            StockQuantity = Math.Max(0, dto.StockQuantity)
        };

        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        _logger.LogInfo($"Товар '{product.Name}' сохранен с ID={product.Id}, остаток={product.StockQuantity}");
        return new ProductResponseDto(product.Id, product.Name, product.Sku, product.Price, product.StockQuantity);
    }

    public async Task<ProductResponseDto?> GetProductByIdAsync(int id)
    {
        _logger.LogInfo($"Поиск товара по ID={id}");
        var p = await _context.Products.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        if (p == null)
        {
            _logger.LogWarning($"Товар с ID={id} не найден");
            return null;
        }

        return new ProductResponseDto(p.Id, p.Name, p.Sku, p.Price, p.StockQuantity);
    }

    public async Task<List<ProductResponseDto>> GetAllProductsAsync()
    {
        _logger.LogInfo("Запрос списка всех товаров");
        return await _context.Products
            .AsNoTracking()
            .Select(p => new ProductResponseDto(p.Id, p.Name, p.Sku, p.Price, p.StockQuantity))
            .ToListAsync();
    }

    public async Task<ProductResponseDto> UpdateProductAsync(int id, UpdateProductDto dto)
    {
        _logger.LogInfo($"Обновление товара ID={id}");
        var product = await _context.Products.FindAsync(id);
        if (product == null)
        {
            var ex = new NotFoundException("Product", id);
            _logger.LogError(ex.Message);
            throw ex;
        }

        if (string.IsNullOrWhiteSpace(dto.Name))
            throw new ValidationException(nameof(dto.Name), "название товара не может быть пустым");

        if (dto.Price < 0)
            throw new ValidationException(nameof(dto.Price), "цена товара не может быть отрицательной");

        product.Name = dto.Name.Trim();
        product.Price = dto.Price;
        product.StockQuantity = Math.Max(0, dto.StockQuantity);

        await _context.SaveChangesAsync();
        _logger.LogInfo($"Товар ID={id} успешно обновлен");

        return new ProductResponseDto(product.Id, product.Name, product.Sku, product.Price, product.StockQuantity);
    }

    public async Task<bool> DeleteProductAsync(int id)
    {
        _logger.LogInfo($"Удаление товара ID={id}");
        var product = await _context.Products.FindAsync(id);
        if (product == null)
        {
            _logger.LogWarning($"Попытка удаления отсутствующего товара ID={id}");
            return false;
        }

        _context.Products.Remove(product);
        await _context.SaveChangesAsync();
        _logger.LogInfo($"Товар ID={id} успешно удален");
        return true;
    }
}
