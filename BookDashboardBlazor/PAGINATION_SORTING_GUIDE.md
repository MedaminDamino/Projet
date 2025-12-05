# 📚 Pagination & Sorting System Guide

A complete, reusable pagination and sorting system for Blazor WebAssembly applications.

---

## 📁 File Structure

```
BookDashboardBlazor/
├── Models/
│   └── PaginationModels.cs          # DTOs and models
├── Services/
│   ├── PaginationHelper.cs          # Helper extension methods
│   └── BookPaginationService.cs     # Example service implementation
├── Shared/
│   └── Components/
│       ├── Pagination.razor         # Pagination component
│       ├── Pagination.razor.css     # Pagination styles
│       ├── SortableHeader.razor     # Sortable column header
│       └── SortableHeader.razor.css # Sortable header styles
├── Pages/
│   └── BooksPaged.razor             # Example page using pagination
└── wwwroot/
    └── css/
        └── pagination.css           # Additional table styles
```

---

## 🚀 Quick Start

### 1. Register Services

In `Program.cs`, the service is already registered:

```csharp
builder.Services.AddScoped<BookPaginationService>();
```

### 2. Use in Your Page

```razor
@page "/my-page"
@using BookDashboardBlazor.Shared.Components
@using BookDashboardBlazor.Models

<Pagination TotalItems="@pagedResult.TotalItems"
            PageSize="@pagination.PageSize"
            CurrentPage="@pagination.Page"
            OnPageChanged="HandlePageChanged"
            OnPageSizeChanged="HandlePageSizeChanged" />
```

### 3. Use Sortable Headers

```razor
<SortableHeader Label="Title" 
                SortBy="title" 
                CurrentSortBy="@currentSort.SortBy"
                CurrentSortDirection="@currentSort.Direction"
                OnSortChanged="HandleSortChanged" />
```

---

## 📦 Components

### `<Pagination />`

A fully-featured pagination component with:
- Page size selector (10/20/50/100)
- Previous/Next buttons
- Smart page number compression (1 ... 4 5 6 ... 10)
- Mobile-responsive design
- Modern UI with animations

#### Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| `TotalItems` | `int` | Total number of items across all pages |
| `PageSize` | `int` | Number of items per page |
| `CurrentPage` | `int` | Current active page (1-based) |
| `OnPageChanged` | `EventCallback<int>` | Fired when user changes page |
| `OnPageSizeChanged` | `EventCallback<int>` | Fired when user changes page size |

#### Example

```razor
<Pagination TotalItems="100"
            PageSize="10"
            CurrentPage="1"
            OnPageChanged="HandlePageChanged"
            OnPageSizeChanged="HandlePageSizeChanged" />
```

---

### `<SortableHeader />`

A clickable table header that supports sorting with visual indicators.

#### Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| `Label` | `string` | Display text for the header |
| `SortBy` | `string` | Sort field identifier (e.g., "title", "date") |
| `CurrentSortBy` | `string?` | Currently active sort field |
| `CurrentSortDirection` | `SortDirection` | Current sort direction (None/Ascending/Descending) |
| `OnSortChanged` | `EventCallback<(string, SortDirection)>` | Fired when user clicks to sort |

#### Example

```razor
<SortableHeader Label="Title" 
                SortBy="title" 
                CurrentSortBy="@currentSort.SortBy"
                CurrentSortDirection="@currentSort.Direction"
                OnSortChanged="HandleSortChanged" />
```

---

## 🎨 Models

### `PaginationModel`

Represents pagination parameters:

```csharp
var pagination = new PaginationModel
{
    Page = 1,        // Current page (1-based)
    PageSize = 10    // Items per page
};

// Computed properties:
int skip = pagination.Skip;  // Calculated skip value
int take = pagination.Take;  // Calculated take value
```

### `SortModel`

Represents sorting parameters:

```csharp
var sort = new SortModel
{
    SortBy = "title",                    // Field to sort by
    Direction = SortDirection.Ascending  // Sort direction
};
```

### `PagedResult<T>`

Generic paginated result:

```csharp
var result = new PagedResult<Book>
{
    Items = books,          // List of items for current page
    TotalItems = 100,       // Total items across all pages
    Page = 1,               // Current page
    PageSize = 10,          // Items per page
    TotalPages = 10,        // Computed: total pages
    HasPreviousPage = false,// Computed: has previous page
    HasNextPage = true      // Computed: has next page
};
```

### `PagedResponseDto<T>`

DTO for API responses (with JsonPropertyName attributes):

```csharp
public class PagedResponseDto<Book>
{
    public List<Book> Items { get; set; }
    public int TotalItems { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}
```

---

## 🔧 Helper Methods

### Client-Side Pagination

```csharp
var allItems = GetItems(); // IEnumerable<T>
var pagination = new PaginationModel { Page = 1, PageSize = 10 };

// Apply pagination
var pagedResult = allItems.ToPagedResult(pagination);
```

### Server-Side Pagination (EF Core)

```csharp
var query = _context.Books.AsQueryable();

// Apply sorting
var sortMap = new Dictionary<string, Expression<Func<Book, object>>>
{
    { "title", b => b.Title },
    { "publishyear", b => b.PublishYear }
};
query = query.ApplySorting(sortModel, sortMap);

// Apply pagination
var result = await query.ToPagedResultAsync(pagination);
```

### Build Query String for API

```csharp
var queryString = PaginationHelper.BuildQueryString(pagination, sortModel);
// Result: "page=1&pageSize=10&sortBy=title&sortDirection=asc"
```

---

## 📝 Complete Example

See `Pages/BooksPaged.razor` for a complete working example.

### Key Implementation Points:

1. **State Management**:
```csharp
private PaginationModel pagination = new() { Page = 1, PageSize = 10 };
private SortModel currentSort = new();
```

2. **Load Data**:
```csharp
private async Task LoadBooks()
{
    pagedResult = await BookPaginationService.GetBooksClientSideAsync(
        pagination, 
        currentSort
    );
}
```

3. **Handle Events**:
```csharp
private async Task HandlePageChanged(int newPage)
{
    pagination.Page = newPage;
    await LoadBooks();
}

private async Task HandleSortChanged((string SortBy, SortDirection Direction) sortInfo)
{
    currentSort.SortBy = sortInfo.SortBy;
    currentSort.Direction = sortInfo.Direction;
    pagination.Page = 1; // Reset to first page
    await LoadBooks();
}
```

---

## 🌐 Backend API Example

### Controller Endpoint

```csharp
[HttpGet]
public async Task<ActionResult<PagedResponseDto<BookDto>>> GetBooks(
    [FromQuery] int page = 1,
    [FromQuery] int pageSize = 10,
    [FromQuery] string? sortBy = null,
    [FromQuery] string? sortDirection = "asc")
{
    var pagination = new PaginationModel { Page = page, PageSize = pageSize };
    var sortModel = new SortModel 
    { 
        SortBy = sortBy, 
        Direction = sortDirection == "desc" ? SortDirection.Descending : SortDirection.Ascending 
    };

    var query = _context.Books.AsQueryable();
    
    // Apply sorting
    var sortMap = new Dictionary<string, Expression<Func<Book, object>>>
    {
        { "title", b => b.Title },
        { "publishyear", b => b.PublishYear }
    };
    query = query.ApplySorting(sortModel, sortMap);

    // Get total count
    var totalItems = await query.CountAsync();

    // Apply pagination
    var items = await query
        .Skip(pagination.Skip)
        .Take(pagination.Take)
        .Select(b => new BookDto { /* map properties */ })
        .ToListAsync();

    return Ok(new PagedResponseDto<BookDto>
    {
        Items = items,
        TotalItems = totalItems,
        Page = pagination.Page,
        PageSize = pagination.PageSize,
        TotalPages = (int)Math.Ceiling((double)totalItems / pagination.PageSize)
    });
}
```

### API Response Format

```json
{
  "items": [
    { "bookId": 1, "title": "Book 1", ... },
    { "bookId": 2, "title": "Book 2", ... }
  ],
  "totalItems": 100,
  "page": 1,
  "pageSize": 10,
  "totalPages": 10
}
```

---

## 🎨 Customization

### CSS Variables

The components use CSS variables for theming. Customize in your global CSS:

```css
:root {
    --primary: #4f46e5;
    --primary-light: #e0e7ff;
    --bg-card: #ffffff;
    --border-color: #e2e8f0;
    --text-primary: #1e293b;
    --text-secondary: #64748b;
}
```

### Styling

Components include scoped CSS files:
- `Pagination.razor.css` - Pagination styles
- `SortableHeader.razor.css` - Sortable header styles

Both support dark mode via `prefers-color-scheme`.

---

## 🔒 Security Notes

1. **Sort Mapping**: Always use a dictionary mapping for server-side sorting to prevent SQL injection:
```csharp
var sortMap = new Dictionary<string, Expression<Func<T, object>>>
{
    { "title", b => b.Title },  // ✅ Safe
    // Never use: sortBy directly in OrderBy() ❌
};
```

2. **Page Size Limits**: Validate page size on backend:
```csharp
PageSize = Math.Max(1, Math.Min(100, pageSize)) // Limit to 100 max
```

---

## ✅ Features

- ✅ Client-side and server-side pagination
- ✅ Multi-column sorting
- ✅ Modern, responsive UI
- ✅ Mobile-friendly
- ✅ Dark mode support
- ✅ Accessible (ARIA labels)
- ✅ Type-safe
- ✅ Reusable across pages
- ✅ Performance optimized

---

## 🌐 Backend API Implementation Examples

### ASP.NET Core Controller Example

```csharp
[ApiController]
[Route("api/[controller]")]
public class BooksController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public BooksController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResponseDto<BookDto>>> GetBooks(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? sortBy = null,
        [FromQuery] string? sortDirection = "asc")
    {
        var pagination = new PaginationModel
        {
            Page = Math.Max(1, page),
            PageSize = Math.Max(1, Math.Min(100, pageSize))
        };

        var sortModel = new SortModel
        {
            SortBy = sortBy,
            Direction = sortDirection?.ToLower() == "desc" 
                ? SortDirection.Descending 
                : SortDirection.Ascending
        };

        // Define sort mapping for security (prevents SQL injection)
        var sortMap = new Dictionary<string, Expression<Func<Book, object>>>
        {
            { "title", b => b.Title },
            { "publishyear", b => b.PublishYear },
            { "bookid", b => b.BookId }
        };

        var query = _context.Books.AsQueryable();
        
        // Apply sorting
        query = query.ApplySorting(sortModel, sortMap);

        // Get total count
        var totalItems = await query.CountAsync();

        // Apply pagination
        var items = await query
            .Skip(pagination.Skip)
            .Take(pagination.Take)
            .Select(b => new BookDto { /* map properties */ })
            .ToListAsync();

        return Ok(new PagedResponseDto<BookDto>
        {
            Items = items,
            TotalItems = totalItems,
            Page = pagination.Page,
            PageSize = pagination.PageSize,
            TotalPages = (int)Math.Ceiling((double)totalItems / pagination.PageSize)
        });
    }
}
```

### EF Core Service Example

```csharp
public async Task<PagedResult<Book>> GetBooksAsync(
    PaginationModel pagination, 
    SortModel sortModel)
{
    var sortMap = new Dictionary<string, Expression<Func<Book, object>>>
    {
        { "title", b => b.Title },
        { "publishyear", b => b.PublishYear }
    };

    var query = _context.Books.AsQueryable();
    query = query.ApplySorting(sortModel, sortMap);
    
    return await query.ToPagedResultAsync(pagination);
}
```

---

## 📖 Additional Resources

- See `Pages/BooksPaged.razor` for complete example
- Components are located in `Shared/Components/`

---

**Happy coding! 🚀**

