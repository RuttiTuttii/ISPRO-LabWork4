using LabWork4.Models;
using Microsoft.EntityFrameworkCore;

namespace LabWork4.Filtering;

public record QueryParameters(int PageNumber = 1, int PageSize = 5, string? SortBy = null, bool SortDescending = false);

public record PagedResult<T>(List<T> Items, int TotalCount, int PageNumber, int PageSize)
{
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;
}

public record ProductFilter(
    string? SearchTerm = null,
    int? CategoryId = null,
    decimal? MinPrice = null,
    decimal? MaxPrice = null,
    bool? InStockOnly = null
);

public static class QueryableExtensions
{
    public static IQueryable<Product> ApplyFilter(this IQueryable<Product> query, ProductFilter filter)
    {
        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
        {
            var search = filter.SearchTerm.Trim();
            query = query.Where(p => EF.Functions.Like(p.Name, $"%{search}%") || EF.Functions.Like(p.Sku, $"%{search}%"));
        }

        if (filter.CategoryId.HasValue)
            query = query.Where(p => p.CategoryId == filter.CategoryId.Value);

        if (filter.MinPrice.HasValue)
            query = query.Where(p => p.Price >= filter.MinPrice.Value);

        if (filter.MaxPrice.HasValue)
            query = query.Where(p => p.Price <= filter.MaxPrice.Value);

        if (filter.InStockOnly.HasValue && filter.InStockOnly.Value)
            query = query.Where(p => p.StockQuantity > 0);

        return query;
    }

    public static IQueryable<Product> ApplySort(this IQueryable<Product> query, string? sortBy, bool sortDescending)
    {
        return (sortBy?.ToLower()) switch
        {
            "price" => sortDescending ? query.OrderByDescending(p => p.Price) : query.OrderBy(p => p.Price),
            "stock" => sortDescending ? query.OrderByDescending(p => p.StockQuantity) : query.OrderBy(p => p.StockQuantity),
            "name" => sortDescending ? query.OrderByDescending(p => p.Name) : query.OrderBy(p => p.Name),
            _ => sortDescending ? query.OrderByDescending(p => p.Id) : query.OrderBy(p => p.Id)
        };
    }

    public static async Task<PagedResult<T>> ToPagedResultAsync<T>(this IQueryable<T> query, int pageNumber, int pageSize)
    {
        var totalCount = await query.CountAsync();
        var items = await query.Skip((pageNumber - 1) * pageSize)
                               .Take(pageSize)
                               .ToListAsync();

        return new PagedResult<T>(items, totalCount, pageNumber, pageSize);
    }
}
