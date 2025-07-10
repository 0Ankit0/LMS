# DataTableComponent Documentation

## Overview

The `DataTableComponent` is a reusable, generic Blazor component that provides a standardized way to display paginated data in tables across the LMS application. It integrates seamlessly with the existing `PaginationComponent` and offers two approaches for defining table columns.

## Features

- **Generic Type Support**: Works with any data type through the `TItem` type parameter
- **Two Column Definition Approaches**: Supports both simple column configuration and advanced QuickGrid columns
- **Built-in Pagination**: Integrates `PaginationComponent` automatically
- **Consistent Styling**: Uses Bootstrap classes for consistent appearance
- **Loading and Empty States**: Handles loading spinners and empty data messages
- **Action Buttons**: Supports custom action buttons for each row
- **Responsive Design**: Includes responsive table wrapper

## Usage

### Approach 1: Simple Column Configuration (Recommended)

This approach is ideal for most scenarios and avoids QuickGrid type inference issues.

```razor
@page "/admin/achievements"
@using LMS.Services
@inject AchievementService AchievementService

<DataTableComponent TItem="AchievementModel"
                   PaginatedResult="paginatedAchievements"
                   TableColumns="achievementColumns"
                   Actions="GetActions"
                   OnPageChanged="OnPageChanged"
                   OnPageSizeChanged="OnPageSizeChanged" />

@code {
    private PaginatedResult<AchievementModel>? paginatedAchievements;
    private int currentPage = 1;
    private int pageSize = 10;

    // Column configuration
    private List<TableColumn<AchievementModel>> achievementColumns = new()
    {
        TableColumn<AchievementModel>.Create("Name", a => a.Name),
        TableColumn<AchievementModel>.Create("Description", a => a.Description),
        TableColumn<AchievementModel>.Create("Points", a => a.Points),
        TableColumn<AchievementModel>.Create("Type", a => a.Type),
        TableColumn<AchievementModel>.Create("Active", a => a.IsActive ? "Yes" : "No"),
        TableColumn<AchievementModel>.Create("Users Earned", a => a.UsersEarnedCount)
    };

    // Actions for each row
    private RenderFragment<AchievementModel> GetActions => achievement =>
    @<div class="btn-group" role="group">
        <a href="@($"/admin/achievements/edit?id={achievement.Id}")" class="btn btn-sm btn-outline-primary">
            <i class="bi bi-pencil"></i> Edit
        </a>
        <a href="@($"/admin/achievements/details?id={achievement.Id}")" class="btn btn-sm btn-outline-info">
            <i class="bi bi-eye"></i> Details
        </a>
        <a href="@($"/admin/achievements/delete?id={achievement.Id}")" class="btn btn-sm btn-outline-danger">
            <i class="bi bi-trash"></i> Delete
        </a>
    </div>;

    protected override async Task OnInitializedAsync()
    {
        await LoadDataAsync();
    }

    private async Task LoadDataAsync()
    {
        var request = new PaginationRequest
        {
            PageNumber = currentPage,
            PageSize = pageSize
        };
        paginatedAchievements = await AchievementService.GetAllAchievementsPaginatedAsync(request);
    }

    private async Task OnPageChanged(int page)
    {
        currentPage = page;
        await LoadDataAsync();
    }

    private async Task OnPageSizeChanged(int size)
    {
        pageSize = size;
        currentPage = 1;
        await LoadDataAsync();
    }
}
```

### Approach 2: QuickGrid Column Configuration

This approach provides more advanced features but may encounter type inference issues in some scenarios.

```razor
<DataTableComponent TItem="CategoryModel"
                   PaginatedResult="paginatedCategories"
                   OnPageChanged="OnPageChanged"
                   OnPageSizeChanged="OnPageSizeChanged">
    <Columns>
        <PropertyColumn Property="category => category.Name" Title="Name" />
        <PropertyColumn Property="category => category.Description" Title="Description" />
        <PropertyColumn Property="category => category.IsActive" Title="Active" />
        <TemplateColumn Title="Course Count" Context="category">
            <span class="badge bg-primary">@category.CourseCount</span>
        </TemplateColumn>
    </Columns>
    <Actions Context="category">
        <div class="btn-group" role="group">
            <a href="@($"/admin/categories/edit?id={category.Id}")" class="btn btn-sm btn-outline-primary">
                <i class="bi bi-pencil"></i> Edit
            </a>
        </div>
    </Actions>
</DataTableComponent>
```

## TableColumn Helper Class

The `TableColumn<T>` class provides a simple way to define columns:

### Properties

- `Title`: Column header text
- `ValueSelector`: Function to extract the value from the data item
- `CssClass`: Optional CSS class for the column
- `IsSortable`: Whether the column supports sorting (not implemented yet)
- `SortKey`: Sort key for the column (not implemented yet)

### Static Factory Methods

```csharp
// Simple column
TableColumn<T>.Create("Name", item => item.Name)

// Column with CSS class
TableColumn<T>.Create("Status", item => item.IsActive ? "Active" : "Inactive", "text-center")

// Sortable column (for future use)
TableColumn<T>.CreateSortable("Name", item => item.Name, "name", "text-start")
```

## Component Parameters

### Required Parameters

- `PaginatedResult<TItem>`: The paginated data to display

### Optional Parameters

- `TableColumns`: List of `TableColumn<TItem>` for simple column configuration
- `Columns`: RenderFragment for QuickGrid column configuration
- `Actions`: RenderFragment for row action buttons
- `EmptyMessage`: Custom message when no data is available (default: "No items found.")
- `OnPageChanged`: Event callback when page changes
- `OnPageSizeChanged`: Event callback when page size changes

## Styling and Customization

The component uses Bootstrap classes for consistent styling:

- Tables use `table table-striped table-hover` classes
- Cards use `card` and `card-body` classes
- Responsive wrapper uses `table-responsive`
- Loading spinner uses `spinner-border`

## Migration from Existing Tables

To migrate existing admin index pages:

1. Replace the existing table HTML with `DataTableComponent`
2. Convert your column definitions to `TableColumn<T>` configuration
3. Move action buttons to the `Actions` parameter
4. Keep existing pagination event handlers

### Before (CategoryPages/Index.razor):

```razor
<QuickGrid Class="table table-striped table-hover" Items="paginatedResult.Items.AsQueryable()">
    <PropertyColumn Property="category => category.Name" Title="Name" />
    <PropertyColumn Property="category => category.Description" Title="Description" />
    <TemplateColumn Title="Actions" Context="category">
        <!-- Action buttons -->
    </TemplateColumn>
</QuickGrid>

<PaginationComponent
    PaginatedResult="paginatedResult"
    OnPageChanged="OnPageChanged"
    OnPageSizeChanged="OnPageSizeChanged" />
```

### After:

```razor
<DataTableComponent TItem="CategoryModel"
                   PaginatedResult="paginatedResult"
                   TableColumns="categoryColumns"
                   Actions="GetActions"
                   OnPageChanged="OnPageChanged"
                   OnPageSizeChanged="OnPageSizeChanged" />
```

## Best Practices

1. **Use Approach 1** for most scenarios to avoid type inference issues
2. **Keep column definitions simple** and let the component handle the rendering
3. **Use consistent action button patterns** across different admin pages
4. **Handle loading states** by ensuring `PaginatedResult` is null during data loading
5. **Provide meaningful empty messages** when appropriate

## Known Limitations

1. **QuickGrid Type Inference**: PropertyColumn with lambda expressions may not work properly inside generic components in some scenarios
2. **Sorting**: Not yet implemented but planned for future enhancement
3. **Filtering**: Not included but can be implemented at the page level

## Future Enhancements

- Built-in sorting support
- Column visibility toggle
- Export functionality
- Advanced filtering
- Column reordering
- Responsive column priority

## Files Created/Modified

- `Components/Shared/DataTableComponent.razor` - Main component
- `Models/Common/TableColumn.cs` - Column configuration helper
- `Components/Admin/AchievementPages/Index.razor` - Example implementation
- `docs/DataTableComponent.md` - This documentation
