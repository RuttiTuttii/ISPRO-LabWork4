using LabWork4.Even.Data;
using LabWork4.Even.DTOs;
using LabWork4.Even.Exceptions;
using LabWork4.Even.Logging;
using LabWork4.Even.Models;
using Microsoft.EntityFrameworkCore;

namespace LabWork4.Even.Services;

public class CustomerService : ICustomerService
{
    private readonly AppDbContext _context;
    private readonly IAppLogger _logger;

    public CustomerService(AppDbContext context, IAppLogger? logger = null)
    {
        _context = context;
        _logger = logger ?? new ConsoleAppLogger(nameof(CustomerService));
    }

    public async Task<CustomerResponseDto> CreateCustomerAsync(CreateCustomerDto dto)
    {
        _logger.LogInfo($"Попытка создания клиента '{dto.Name}' ({dto.Email})");

        if (string.IsNullOrWhiteSpace(dto.Name))
            throw new ValidationException(nameof(dto.Name), "имя обязательно для заполнения");

        if (string.IsNullOrWhiteSpace(dto.Email) || !dto.Email.Contains('@'))
            throw new ValidationException(nameof(dto.Email), "некорректный адрес электронной почты");

        var normalizedEmail = dto.Email.Trim().ToLowerInvariant();
        var exists = await _context.Customers.AnyAsync(c => c.Email == normalizedEmail);
        if (exists)
        {
            var ex = new DuplicateEntityException("Customer", nameof(dto.Email), normalizedEmail);
            _logger.LogWarning(ex.Message);
            throw ex;
        }

        var customer = new Customer
        {
            Name = dto.Name.Trim(),
            Email = normalizedEmail,
            Phone = dto.Phone.Trim()
        };

        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();

        _logger.LogInfo($"Клиент успешно создан с ID={customer.Id}");
        return new CustomerResponseDto(customer.Id, customer.Name, customer.Email, customer.Phone, customer.CreatedAt);
    }

    public async Task<CustomerResponseDto?> GetCustomerByIdAsync(int id)
    {
        _logger.LogInfo($"Запрос данных клиента ID={id}");
        var c = await _context.Customers.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        if (c == null)
        {
            _logger.LogWarning($"Клиент ID={id} не найден");
            return null;
        }

        return new CustomerResponseDto(c.Id, c.Name, c.Email, c.Phone, c.CreatedAt);
    }

    public async Task<List<CustomerResponseDto>> GetAllCustomersAsync()
    {
        _logger.LogInfo("Запрос списка всех клиентов");
        return await _context.Customers
            .AsNoTracking()
            .Select(c => new CustomerResponseDto(c.Id, c.Name, c.Email, c.Phone, c.CreatedAt))
            .ToListAsync();
    }

    public async Task<CustomerResponseDto> UpdateCustomerAsync(int id, UpdateCustomerDto dto)
    {
        _logger.LogInfo($"Обновление данных клиента ID={id}");
        var customer = await _context.Customers.FindAsync(id);
        if (customer == null)
        {
            var ex = new NotFoundException("Customer", id);
            _logger.LogError(ex.Message);
            throw ex;
        }

        if (string.IsNullOrWhiteSpace(dto.Name))
            throw new ValidationException(nameof(dto.Name), "имя клиента не может быть пустым");

        customer.Name = dto.Name.Trim();
        customer.Phone = dto.Phone.Trim();

        await _context.SaveChangesAsync();
        _logger.LogInfo($"Данные клиента ID={id} успешно обновлены");

        return new CustomerResponseDto(customer.Id, customer.Name, customer.Email, customer.Phone, customer.CreatedAt);
    }

    public async Task<bool> DeleteCustomerAsync(int id)
    {
        _logger.LogInfo($"Удаление клиента ID={id}");
        var customer = await _context.Customers.FindAsync(id);
        if (customer == null)
        {
            _logger.LogWarning($"Попытка удаления несуществующего клиента ID={id}");
            return false;
        }

        _context.Customers.Remove(customer);
        await _context.SaveChangesAsync();
        _logger.LogInfo($"Клиент ID={id} успешно удален");
        return true;
    }
}
