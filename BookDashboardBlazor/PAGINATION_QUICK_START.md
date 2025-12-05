# 🚀 Pagination & Sorting - Quick Start Guide

## ✅ What Was Created

### Components
- ✅ **`Pagination.razor`** - Full-featured pagination component
- ✅ **`SortableHeader.razor`** - Clickable sortable column headers

### Models & DTOs
- ✅ **`PaginationModel`** - Pagination parameters
- ✅ **`SortModel`** - Sorting parameters  
- ✅ **`PagedResult<T>`** - Generic paginated result
- ✅ **`PagedResponseDto<T>`** - API response DTO

### Services & Helpers
- ✅ **`PaginationHelper`** - Extension methods for pagination/sorting
- ✅ **`BookPaginationService`** - Example service implementation

### Examples
- ✅ **`BooksPaged.razor`** - Complete working example page
- ✅ **`BackendPaginationExample.cs`** - Backend API examples

### Styling
- ✅ **`Pagination.razor.css`** - Component styles
- ✅ **`SortableHeader.razor.css`** - Header styles
- ✅ **`pagination.css`** - Additional table styles

---

## 📝 Basic Usage (3 Steps)

### Step 1: Add State Variables

```csharp
private PaginationModel pagination = new() { Page = 1, PageSize = 10 };
private SortModel currentSort = new();
private PagedResult<Book>? pagedResult;
```

### Step 2: Add Pagination Component

```razor
<Pagination TotalItems="@pagedResult.TotalItems"
            PageSize="@pagination.PageSize"
            CurrentPage="@pagination.Page"
            OnPageChanged="HandlePageChanged"
            OnPageSizeChanged="HandlePageSizeChanged" />
```

### Step 3: Add Sortable Headers

```razor
<SortableHeader Label="Title" 
                SortBy="title" 
                CurrentSortBy="@currentSort.SortBy"
                CurrentSortDirection="@currentSort.Direction"
                OnSortChanged="HandleSortChanged" />
```

---

## 🎯 Event Handlers

```csharp
private async Task HandlePageChanged(int newPage)
{
    pagination.Page = newPage;
    await LoadData();
}

private async Task HandlePageSizeChanged(int newPageSize)
{
    pagination.PageSize = newPageSize;
    pagination.Page = 1;
    await LoadData();
}

private async Task HandleSortChanged((string SortBy, SortDirection Direction) sortInfo)
{
    currentSort.SortBy = sortInfo.SortBy;
    currentSort.Direction = sortInfo.Direction;
    pagination.Page = 1;
    await LoadData();
}
```

---

## 📍 File Locations

```
Models/
  └── PaginationModels.cs

Services/
  ├── PaginationHelper.cs
  └── BookPaginationService.cs

Shared/Components/
  ├── Pagination.razor (+ .css)
  └── SortableHeader.razor (+ .css)

Pages/
  └── BooksPaged.razor (example)

wwwroot/css/
  └── pagination.css
```

---

## 🔗 Next Steps

1. See `Pages/BooksPaged.razor` for complete example
2. Read `PAGINATION_SORTING_GUIDE.md` for detailed documentation
3. Customize styles in component `.css` files

**Ready to use!** 🎉

