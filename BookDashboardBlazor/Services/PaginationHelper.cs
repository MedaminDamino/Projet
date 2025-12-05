using System.Linq;
using System.Linq.Expressions;
using System.Text;
using BookDashboardBlazor.Models;

namespace BookDashboardBlazor.Services;

/// <summary>
/// Helper class for pagination and sorting operations.
/// </summary>
public static class PaginationHelper
{
    /// <summary>
    /// Applies pagination to an IQueryable collection (Server-side).
    /// </summary>
    public static IQueryable<T> ApplyPagination<T>(this IQueryable<T> query, PaginationModel pagination)
    {
        return query.Skip(pagination.Skip).Take(pagination.Take);
    }

    /// <summary>
    /// Applies sorting to an IQueryable collection (Server-side).
    /// </summary>
    public static IQueryable<T> ApplySorting<T>(this IQueryable<T> query, SortModel sortModel, 
        Dictionary<string, Expression<Func<T, object>>> sortMap)
    {
        if (string.IsNullOrWhiteSpace(sortModel.SortBy) || sortModel.Direction == SortDirection.None)
        {
            return query;
        }

        if (!sortMap.TryGetValue(sortModel.SortBy, out var sortExpression))
        {
            return query; // Invalid sort field, return unsorted
        }

        return sortModel.Direction == SortDirection.Ascending
            ? query.OrderBy(sortExpression)
            : query.OrderByDescending(sortExpression);
    }

    /// <summary>
    /// Creates a PagedResult from an IQueryable collection (Server-side with count).
    /// </summary>
    public static async Task<PagedResult<T>> ToPagedResultAsync<T>(
        this IQueryable<T> query, 
        PaginationModel pagination)
    {
        var totalItems = query.Count();
        var items = query.Skip(pagination.Skip).Take(pagination.Take).ToList();

        return new PagedResult<T>
        {
            Items = items,
            TotalItems = totalItems,
            Page = pagination.Page,
            PageSize = pagination.PageSize
        };
    }

    /// <summary>
    /// Applies pagination to an IEnumerable collection (Client-side).
    /// </summary>
    public static IEnumerable<T> ApplyPagination<T>(this IEnumerable<T> collection, PaginationModel pagination)
    {
        return collection.Skip(pagination.Skip).Take(pagination.Take);
    }

    /// <summary>
    /// Creates a PagedResult from an IEnumerable collection (Client-side).
    /// </summary>
    public static PagedResult<T> ToPagedResult<T>(this IEnumerable<T> collection, PaginationModel pagination)
    {
        if (collection == null)
        {
            return new PagedResult<T>
            {
                Items = new List<T>(),
                TotalItems = 0,
                Page = pagination?.Page ?? 1,
                PageSize = pagination?.PageSize > 0 ? pagination.PageSize : 10
            };
        }
        
        if (pagination == null)
        {
            pagination = new PaginationModel { Page = 1, PageSize = 10 };
        }
        
        var items = collection.ToList();
        var totalItems = items.Count;
        
        // Ensure pagination values are valid
        if (pagination.PageSize <= 0)
        {
            pagination.PageSize = 10;
        }
        if (pagination.Page < 1)
        {
            pagination.Page = 1;
        }
        
        var pagedItems = items.Skip(pagination.Skip).Take(pagination.Take).ToList();

        return new PagedResult<T>
        {
            Items = pagedItems ?? new List<T>(),
            TotalItems = totalItems,
            Page = pagination.Page,
            PageSize = pagination.PageSize
        };
    }

    /// <summary>
    /// Builds query string parameters for API calls.
    /// </summary>
    public static string BuildQueryString(PaginationModel pagination, SortModel? sortModel = null)
    {
        var queryParams = new List<string>
        {
            $"page={pagination.Page}",
            $"pageSize={pagination.PageSize}"
        };

        if (sortModel != null && !string.IsNullOrWhiteSpace(sortModel.SortBy))
        {
            queryParams.Add($"sortBy={Uri.EscapeDataString(sortModel.SortBy)}");
            queryParams.Add($"sortDirection={(sortModel.Direction == SortDirection.Ascending ? "asc" : "desc")}");
        }

        return string.Join("&", queryParams);
    }

    /// <summary>
    /// Parses query string parameters into PaginationModel and SortModel.
    /// For Blazor WebAssembly, use NavigationManager.Uri and parse manually.
    /// </summary>
    public static (PaginationModel Pagination, SortModel Sort) ParseQueryString(string queryString)
    {
        var queryParams = new Dictionary<string, string>();
        if (!string.IsNullOrEmpty(queryString))
        {
            var pairs = queryString.TrimStart('?').Split('&');
            foreach (var pair in pairs)
            {
                var keyValue = pair.Split('=', 2);
                if (keyValue.Length == 2)
                {
                    queryParams[Uri.UnescapeDataString(keyValue[0])] = Uri.UnescapeDataString(keyValue[1]);
                }
            }
        }

        var pagination = new PaginationModel
        {
            Page = queryParams.TryGetValue("page", out var pageStr) && int.TryParse(pageStr, out var page) 
                ? Math.Max(1, page) : 1,
            PageSize = queryParams.TryGetValue("pageSize", out var pageSizeStr) && int.TryParse(pageSizeStr, out var pageSize) 
                ? Math.Max(1, pageSize) : 10
        };

        var sort = new SortModel
        {
            SortBy = queryParams.TryGetValue("sortBy", out var sortBy) ? sortBy : null,
            Direction = queryParams.TryGetValue("sortDirection", out var sortDir) && sortDir?.ToLower() == "desc" 
                ? SortDirection.Descending 
                : SortDirection.Ascending
        };

        return (pagination, sort);
    }
}

