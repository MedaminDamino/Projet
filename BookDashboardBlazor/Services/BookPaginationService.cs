using System.Net.Http.Json;
using BookDashboardBlazor.Models;

namespace BookDashboardBlazor.Services;

/// <summary>
/// Service for paginated and sorted Book API calls.
/// Example implementation showing how to integrate pagination with your API.
/// </summary>
public class BookPaginationService
{
    private readonly HttpClient _httpClient;
    private const string ApiEndpoint = "api/Books";

    public BookPaginationService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    /// <summary>
    /// Fetches paginated and sorted books from the API.
    /// </summary>
    public async Task<PagedResult<Book>> GetBooksAsync(
        PaginationModel pagination, 
        SortModel? sortModel = null)
    {
        try
        {
            // Build query string
            var queryString = PaginationHelper.BuildQueryString(pagination, sortModel);
            var url = $"{ApiEndpoint}?{queryString}";

            // Make API call
            var response = await _httpClient.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                // Parse paginated response
                var pagedResponse = await response.Content.ReadFromJsonAsync<PagedResponseDto<Book>>();
                
                if (pagedResponse != null)
                {
                    return new PagedResult<Book>
                    {
                        Items = pagedResponse.Items,
                        TotalItems = pagedResponse.TotalItems,
                        Page = pagedResponse.Page,
                        PageSize = pagedResponse.PageSize
                    };
                }
            }

            // Fallback: return empty result
            return new PagedResult<Book>
            {
                Items = new List<Book>(),
                TotalItems = 0,
                Page = pagination.Page,
                PageSize = pagination.PageSize
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching paginated books: {ex.Message}");
            return new PagedResult<Book>
            {
                Items = new List<Book>(),
                TotalItems = 0,
                Page = pagination.Page,
                PageSize = pagination.PageSize
            };
        }
    }

    /// <summary>
    /// Alternative: Fetch all books and paginate client-side (for smaller datasets).
    /// </summary>
    public async Task<PagedResult<Book>> GetBooksClientSideAsync(
        PaginationModel pagination,
        SortModel? sortModel = null)
    {
        try
        {
            // Fetch all books
            var response = await _httpClient.GetAsync(ApiEndpoint);
            
            if (response.IsSuccessStatusCode)
            {
                var allBooks = await response.Content.ReadFromJsonAsync<List<Book>>() ?? new List<Book>();

                // Apply client-side sorting (if needed)
                if (sortModel != null && !string.IsNullOrWhiteSpace(sortModel.SortBy))
                {
                    allBooks = ApplyClientSideSorting(allBooks, sortModel);
                }

                // Apply client-side pagination using extension method
                // Ensure pagination is valid
                if (pagination == null || pagination.PageSize <= 0)
                {
                    pagination = new PaginationModel { Page = 1, PageSize = 10 };
                }
                
                return allBooks.ToPagedResult(pagination);
            }

            return new PagedResult<Book>
            {
                Items = new List<Book>(),
                TotalItems = 0,
                Page = pagination.Page,
                PageSize = pagination.PageSize
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching books: {ex.Message}");
            return new PagedResult<Book>
            {
                Items = new List<Book>(),
                TotalItems = 0,
                Page = pagination.Page,
                PageSize = pagination.PageSize
            };
        }
    }

    /// <summary>
    /// Helper method for client-side sorting.
    /// </summary>
    private List<Book> ApplyClientSideSorting(List<Book> books, SortModel sortModel)
    {
        if (books == null || sortModel == null || string.IsNullOrWhiteSpace(sortModel.SortBy))
        {
            return books ?? new List<Book>();
        }

        return sortModel.SortBy.ToLower() switch
        {
            "title" => sortModel.Direction == SortDirection.Ascending
                ? books.OrderBy(b => b.Title).ToList()
                : books.OrderByDescending(b => b.Title).ToList(),
            "publishyear" => sortModel.Direction == SortDirection.Ascending
                ? books.OrderBy(b => b.PublishYear).ToList()
                : books.OrderByDescending(b => b.PublishYear).ToList(),
            "bookid" => sortModel.Direction == SortDirection.Ascending
                ? books.OrderBy(b => b.BookId).ToList()
                : books.OrderByDescending(b => b.BookId).ToList(),
            "authorid" => sortModel.Direction == SortDirection.Ascending
                ? books.OrderBy(b => b.AuthorId).ToList()
                : books.OrderByDescending(b => b.AuthorId).ToList(),
            "genreid" => sortModel.Direction == SortDirection.Ascending
                ? books.OrderBy(b => b.GenreId).ToList()
                : books.OrderByDescending(b => b.GenreId).ToList(),
            _ => books
        };
    }
}

