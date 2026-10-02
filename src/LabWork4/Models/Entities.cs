namespace LabWork4.Models;

public class Category
{
    private string _name = string.Empty;

    public int Id { get; set; }

    public string Name
    {
        get => _name;
        set
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("название категории не может быть пустым", nameof(value));
            _name = value.Trim();
        }
    }

    public string? Description { get; set; }

    public List<Product> Products { get; set; } = new();
}

public class Product
{
    private string _name = string.Empty;
    private decimal _price;

    public int Id { get; set; }

    public string Name
    {
        get => _name;
        set
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("название товара не может быть пустым", nameof(value));
            _name = value.Trim();
        }
    }

    public string Sku { get; set; } = string.Empty;

    public decimal Price
    {
        get => _price;
        set
        {
            if (value < 0)
                throw new ArgumentException("цена товара не может быть отрицательной", nameof(value));
            _price = value;
        }
    }

    public int StockQuantity { get; set; }

    public int? CategoryId { get; set; }
    public Category? Category { get; set; }
}

public class Customer
{
    private string _name = string.Empty;
    private string _email = string.Empty;

    public int Id { get; set; }

    public string Name
    {
        get => _name;
        set
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("имя клиента не может быть пустым", nameof(value));
            _name = value.Trim();
        }
    }

    public string Email
    {
        get => _email;
        set
        {
            if (string.IsNullOrWhiteSpace(value) || !value.Contains('@'))
                throw new ArgumentException("некорректный email адрес", nameof(value));
            _email = value.Trim().ToLowerInvariant();
        }
    }

    public string Phone { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public List<Order> Orders { get; set; } = new();
}

public enum OrderStatus
{
    Pending,
    Processing,
    Confirmed,
    Shipped,
    Completed,
    Cancelled
}

public class Order
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;
    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    public DateTime OrderDate { get; set; } = DateTime.UtcNow;

    public List<OrderItem> Items { get; set; } = new();
    public decimal TotalAmount => Items.Sum(i => i.UnitPrice * i.Quantity);
}

public class OrderItem
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public Order Order { get; set; } = null!;
    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}
