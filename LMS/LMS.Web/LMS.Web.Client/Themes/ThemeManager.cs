using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using System.Threading.Tasks;

namespace LMS.Web.Client.Themes
{

    public class ThemeManager
    {
        private readonly ILocalStorageService _localStorage;
        private const string ThemeKey = "theme";

        public ThemeManager(ILocalStorageService localStorage)
        {
            _localStorage = localStorage;
            CurrentTheme = CustomTheme.LightTheme;
        }

        public MudTheme CurrentTheme { get; private set; }

        public async Task LoadThemeAsync()
        {
            var themeName = await _localStorage.GetItemAsync<string>(ThemeKey);
            if (themeName == "dark")
            {
                CurrentTheme = CustomTheme.DarkTheme;
            }
            else
            {
                CurrentTheme = CustomTheme.LightTheme;
            }
        }

        public async Task SetThemeAsync(string themeName)
        {
            if (themeName == "dark")
            {
                CurrentTheme = CustomTheme.DarkTheme;
            }
            else
            {
                CurrentTheme = CustomTheme.LightTheme;
            }
            await _localStorage.SetItemAsync(ThemeKey, themeName);
        }
    }

}
