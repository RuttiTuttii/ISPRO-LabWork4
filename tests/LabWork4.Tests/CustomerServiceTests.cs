using LabWork4.Data;
using LabWork4.DTOs;
using LabWork4.Exceptions;
using LabWork4.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace LabWork4.Tests;

public class CustomerServiceTests
{
    private AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    [Fact]
    public async Task CreateCustomerAsync_CreatesAndReturnsCustomer_WhenValid()
    {
        using var db = CreateContext();
        var service = new CustomerService(db);

        var dto = new CreateCustomerDto("Сергей Попов", "sergey@test.com", "+7 999 123-45-67");
        var result = await service.CreateCustomerAsync(dto);

        Assert.True(result.Id > 0);
        Assert.Equal("Сергей Попов", result.Name);
        Assert.Equal("sergey@test.com", result.Email);
    }

    [Fact]
    public async Task CreateCustomerAsync_ThrowsDuplicateEntityException_WhenEmailExists()
    {
        using var db = CreateContext();
        var service = new CustomerService(db);

        var dto1 = new CreateCustomerDto("Пользователь 1", "same@test.com", "+1");
        await service.CreateCustomerAsync(dto1);

        var dto2 = new CreateCustomerDto("Пользователь 2", "same@test.com", "+2");
        await Assert.ThrowsAsync<DuplicateEntityException>(() => service.CreateCustomerAsync(dto2));
    }

    [Theory]
    [InlineData("", "valid@test.com")]
    [InlineData("Иван", "invalid-email")]
    public async Task CreateCustomerAsync_ThrowsValidationException_OnInvalidData(string name, string email)
    {
        using var db = CreateContext();
        var service = new CustomerService(db);

        var dto = new CreateCustomerDto(name, email, "+7");
        await Assert.ThrowsAsync<ValidationException>(() => service.CreateCustomerAsync(dto));
    }

    [Fact]
    public async Task UpdateCustomerAsync_ThrowsNotFoundException_WhenDoesNotExist()
    {
        using var db = CreateContext();
        var service = new CustomerService(db);

        var dto = new UpdateCustomerDto("Новое Имя", "+7");
        await Assert.ThrowsAsync<NotFoundException>(() => service.UpdateCustomerAsync(9999, dto));
    }
}
