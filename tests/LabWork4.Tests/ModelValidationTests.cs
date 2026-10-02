using LabWork4.Models;
using Xunit;

namespace LabWork4.Tests;

public class ModelValidationTests
{
    [Fact]
    public void Product_ValidatesNameAndPrice()
    {
        var product = new Product { Name = "Монитор", Price = 25000m };
        Assert.Equal("Монитор", product.Name);
        Assert.Equal(25000m, product.Price);

        Assert.Throws<ArgumentException>(() => product.Name = "");
        Assert.Throws<ArgumentException>(() => product.Price = -50m);
    }

    [Fact]
    public void Customer_ValidatesEmail()
    {
        var customer = new Customer { Name = "Иван", Email = "ivan@test.com" };
        Assert.Equal("ivan@test.com", customer.Email);

        Assert.Throws<ArgumentException>(() => customer.Email = "invalid-email");
    }

    [Fact]
    public void Category_ValidatesName()
    {
        var cat = new Category { Name = "Электроника" };
        Assert.Equal("Электроника", cat.Name);

        Assert.Throws<ArgumentException>(() => cat.Name = "   ");
    }
}
