namespace LMS.Models.Common
{
    public class DropdownOption<T>
    {
        public T Value { get; set; } = default!;
        public string Text { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsDisabled { get; set; } = false;
    }

    public class DropdownOption : DropdownOption<int>
    {
    }

    public class StringDropdownOption : DropdownOption<string>
    {
    }
}
