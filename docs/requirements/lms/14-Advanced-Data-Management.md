# 14. Advanced Data Management Patterns

This document outlines specific data management patterns used in the application to improve code quality, maintainability, and performance.

## 1. Centralized Dropdown Repository

Throughout the UI, many forms require data to populate dropdown lists (e.g., selecting a category for a course, an instructor, etc.). To avoid duplicating the logic for fetching this data in multiple places, we use a centralized `DropdownRepository`.

### Purpose

-   **Single Source of Truth**: Provides one place to get all data required for UI select lists.
-   **Consistency**: Ensures that all dropdowns for the same data type (e.g., Categories) look and behave the same.
-   **Efficiency**: The repository can be configured to cache dropdown data that does not change often, reducing database queries.
-   **Clean Code**: Blazor components that need dropdown data can simply inject the `DropdownRepository` instead of multiple other repositories, cleaning up the `@inject` section and component logic.

### Structure

**`DropdownRepository.cs`**

This repository is responsible for fetching data and converting it into a simple, consistent format suitable for a dropdown.

```csharp
public class DropdownRepository
{
    // ... constructor and injected dependencies ...

    public async Task<List<DropdownOption>> GetCategoriesAsync();
    public async Task<List<DropdownOption>> GetTagsAsync();
    public async Task<List<DropdownOption>> GetUsersByRoleAsync(string roleName);
    public async Task<List<DropdownOption>> GetCoursesAsync();
    public List<DropdownOption> GetEnumValues<T>() where T : Enum;
}
```

**`DropdownOption.cs` DTO**

The repository methods all return a list of this simple Data Transfer Object.

```csharp
public class DropdownOption
{
    public object Value { get; set; } // The ID or value of the item (e.g., 1, "GUID", "Admin")
    public string Text { get; set; }  // The display text (e.g., "Science Fiction", "John Doe")
}
```

### Example Usage in a Blazor Component

```csharp
@page "/admin/courses/create"
@inject DropdownRepository DropdownRepo

<MudSelect T="int" Label="Category" @bind-Value="_course.CategoryId">
    @foreach (var category in _categories)
    {
        <MudSelectItem T="int" Value="(int)category.Value">@category.Text</MudSelectItem>
    }
</MudSelect>

@code {
    private CreateCourseRequest _course = new();
    private List<DropdownOption> _categories = new();

    protected override async Task OnInitializedAsync()
    {
        _categories = await DropdownRepo.GetCategoriesAsync();
    }
}
```
