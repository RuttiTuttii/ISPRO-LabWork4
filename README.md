# isrpo лаба 4 — команда чётных

лаба по мдк.02.02 исрпо. тема — модули проекта, командная работа.

## кто что делает (чётные задачи, вариант 1)

- **участник 2:** crud в сервисах — `CustomerService`, `ProductService`, `OrderService`. плюс валидация входных данных и чтобы база не разъехалась.
- **участник 4:** свои исключения (`NotFoundException`, `ValidationException`, `DuplicateEntityException`), логирование через `AppLogger` и консольный cli.

## что надо

- .net sdk 10.0+ (`net10.0`)
- ef core sqlite / inmemory 10.0.x
- xunit

## как собрать и запустить

```bash
dotnet restore
dotnet build
dotnet test
dotnet run --project src/LabWork4.Even/LabWork4.Even.csproj
```