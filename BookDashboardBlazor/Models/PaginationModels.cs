namespace BookDashboardBlazor.Models;

/// <summary>
/// Represents pagination parameters for queries.
/// </summary>
public class PaginationModel
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    
    public int Skip => (Page - 1) * PageSize;
    public int Take => PageSize;
}

/// <summary>
/// Represents sorting parameters for queries.
/// </summary>
public class SortModel
{
    public string? SortBy { get; set; }
    public SortDirection Direction { get; set; } = SortDirection.Ascending;
    
    public string? QueryString => string.IsNullOrWhiteSpace(SortBy) 
        ? null 
        : $"{SortBy}:{(Direction == SortDirection.Ascending ? "asc" : "desc")}";
}

/// <summary>
/// Represents sort direction.
/// </summary>
public enum SortDirection
{
    None = 0,
    Ascending = 1,
    Descending = 2
}

/// <summary>
/// Generic paginated result containing items and metadata.
/// </summary>
/// <typeparam name="T">Type of items in the result</typeparam>
public class PagedResult<T>
{
    public List<T> Items { get; set; } = new();
    public int TotalItems { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => PageSize > 0 ? (int)Math.Ceiling((double)TotalItems / PageSize) : 0;
    public bool HasPreviousPage => Page > 1;
    public bool HasNextPage => Page < TotalPages;
}

/// <summary>
/// DTO for paginated API response from backend.
/// </summary>
/// <typeparam name="T">Type of items in the result</typeparam>
public class PagedResponseDto<T>
{
    [System.Text.Json.Serialization.JsonPropertyName("items")]
    public List<T> Items { get; set; } = new();
    
    [System.Text.Json.Serialization.JsonPropertyName("totalItems")]
    public int TotalItems { get; set; }
    
    [System.Text.Json.Serialization.JsonPropertyName("page")]
    public int Page { get; set; }
    
    [System.Text.Json.Serialization.JsonPropertyName("pageSize")]
    public int PageSize { get; set; }
    
    [System.Text.Json.Serialization.JsonPropertyName("totalPages")]
    public int TotalPages { get; set; }
}

