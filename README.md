# isrpo лаба 4 - разработка и интеграция модулей проекта

лаба по мдк.02.02 исрпо. тема - модули проекта, командная работа. вариант 1.

## кто что делает

нас двое в команде:

- **RuttiTuttii (чётные задачи):**
  - задача 2: crud в сервисах (`CustomerService`, `ProductService`, `OrderService`), валидация входных данных, сохранение целостности бд
  - задача 4: свои исключения (`NotFoundException`, `ValidationException`, `DuplicateEntityException`), логирование через `AppLogger` и консольный cli

- **Skverno-Slov (нечётные задачи):**
  - задача 1: модели данных (`Category`, `Product`, `Customer`, `Order`, `OrderItem`), контекст базы данных (`AppDbContext`) и сидирование (`DbInitializer`)
  - задача 3: динамическая фильтрация, сортировка и пагинация (`ProductFilter`, `PagedResult<T>`, `QueryableExtensions`)

## что надо

- .net sdk 10.0+ (`net10.0`)
- ef core sqlite / inmemory 10.0.x
- xunit

## как собрать и запустить

```bash
dotnet restore
dotnet build
dotnet test
dotnet run --project src/LabWork4/LabWork4.csproj
```
