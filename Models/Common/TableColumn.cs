using Microsoft.AspNetCore.Components;

namespace LMS.Models.Common
{
    public class TableColumn<T>
    {
        public string Title { get; set; } = string.Empty;
        public Func<T, object?> ValueSelector { get; set; } = _ => null;
        public string? CssClass { get; set; }
        public bool IsSortable { get; set; } = false;
        public string? SortKey { get; set; }

        public static TableColumn<T> Create(string title, Func<T, object?> valueSelector, string? cssClass = null)
        {
            return new TableColumn<T>
            {
                Title = title,
                ValueSelector = valueSelector,
                CssClass = cssClass
            };
        }

        public static TableColumn<T> CreateSortable(string title, Func<T, object?> valueSelector, string sortKey, string? cssClass = null)
        {
            return new TableColumn<T>
            {
                Title = title,
                ValueSelector = valueSelector,
                CssClass = cssClass,
                IsSortable = true,
                SortKey = sortKey
            };
        }
    }
}
