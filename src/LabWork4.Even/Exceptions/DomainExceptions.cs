namespace LabWork4.Even.Exceptions;

public class DomainException : Exception
{
    public DomainException(string message) : base(message) { }
}

public class NotFoundException : DomainException
{
    public string EntityName { get; }
    public object Key { get; }

    public NotFoundException(string entityName, object key)
        : base($"сущность '{entityName}' с ключом '{key}' не найдена")
    {
        EntityName = entityName;
        Key = key;
    }
}

public class ValidationException : DomainException
{
    public string PropertyName { get; }

    public ValidationException(string propertyName, string message)
        : base($"ошибка валидации поля '{propertyName}': {message}")
    {
        PropertyName = propertyName;
    }
}

public class DuplicateEntityException : DomainException
{
    public string EntityName { get; }
    public string FieldName { get; }
    public object Value { get; }

    public DuplicateEntityException(string entityName, string fieldName, object value)
        : base($"сущность '{entityName}' с полем '{fieldName}'='{value}' уже существует")
    {
        EntityName = entityName;
        FieldName = fieldName;
        Value = value;
    }
}
