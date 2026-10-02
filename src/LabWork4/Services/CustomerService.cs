using LabWork4.Data;
using LabWork4.DTOs;
using LabWork4.Exceptions;
using LabWork4.Logging;
using LabWork4.Models;
using Microsoft.EntityFrameworkCore;

namespace LabWork4.Services;

public class CustomerService : ICustomerService
{
    private readonly AppDbContext _context;
    private readonly IAppLogger? _logger;

    public CustomerService(AppDbContext context, IAppLogger? logger = null)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<CustomerDto> CreateCustomerAsync(CreateCustomerDto dto)
    {
        _logger?.LogInfo($"Попытка создания клиента '{dto.Name}' ({dto.Email})");

        if (string.IsNullOrWhiteSpace(dto.Name))
            throw new ValidationException(nameof(dto.Name), "имя клиента обязательно для заполнения");

        if (string.IsNullOrWhiteSpace(dto.Email) || !dto.Email.Contains('@'))
            throw new ValidationException(nameof(dto.Email), "некорректный email адрес");

        var normalizedEmail = dto.Email.Trim().ToLowerInvariant();
        var exists = await _context.Customers.AnyAsync(c => c.Email == normalizedEmail);
        if (exists)
        {
            _logger?.LogWarning($"сущность 'Customer' с полем 'Email'='{dto.Email}' уже существует");
            throw new DuplicateEntityException(nameof(Customer), nameof(Customer.Email), dto.Email);
        }

        var customer = new Customer
        {
            Name = dto.Name.Trim(),
            Email = normalizedEmail,
            Phone = dto.Phone?.Trim() ?? string.Empty
        };

        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();

        _logger?.LogInfo($"Клиент успешно создан с ID={customer.Id}");
        return ToDto(customer);
    }

    public async Task<CustomerDto?> GetCustomerByIdAsync(int id)
    {
        _logger?.LogInfo($"Запрос данных клиента ID={id}");
        var customer = await _context.Customers.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id);
        return customer == null ? null : ToDto(customer);
    }

    public async Task<List<CustomerDto>> GetAllCustomersAsync()
    {
        var customers = await _context.Customers.AsNoTracking().ToListAsync();
        return customers.Select(ToDto).ToList();
    }

    public async Task<CustomerDto> UpdateCustomerAsync(int id, UpdateCustomerDto dto)
    {
        _logger?.LogInfo($"Обновление данных клиента ID={id}");
        var customer = await _context.Customers.FindAsync(id);
        if (customer == null)
        {
            _logger?.LogError($"сущность 'Customer' с ключом '{id}' не найдена");
            throw new NotFoundException(nameof(Customer), id);
        }

        if (string.IsNullOrWhiteSpace(dto.Name))
            throw new ValidationException(nameof(dto.Name), "имя клиента обязательно для заполнения");

        customer.Name = dto.Name.Trim();
        if (dto.Phone != null)
            customer.Phone = dto.Phone.Trim();

        await _context.SaveChangesAsync();
        _logger?.LogInfo($"Клиент ID={id} успешно обновлен");
        return ToDto(customer);
    }

    public async Task<bool> DeleteCustomerAsync(int id)
    {
        var customer = await _context.Customers.FindAsync(id);
        if (customer == null)
            return false;

        _context.Customers.Remove(customer);
        await _context.SaveChangesAsync();
        _logger?.LogInfo($"Клиент ID={id} удален");
        return true;
    }

    private static CustomerDto ToDto(Customer c) =>
        new(c.Id, c.Name, c.Email, c.Phone, c.CreatedAt);
}
