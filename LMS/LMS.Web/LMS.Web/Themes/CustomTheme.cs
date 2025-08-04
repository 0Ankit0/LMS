using MudBlazor;

namespace LMS.Web.Themes
{
    public class CustomTheme
    {
        public static MudTheme LightTheme = new MudTheme()
        {
            Palette = new PaletteLight()
            {
                Primary = Colors.Blue.Default,
                Secondary = Colors.Green.Accent4,
                AppbarBackground = Colors.Blue.Default,
            },
        };

        public static MudTheme DarkTheme = new MudTheme()
        {
            Palette = new PaletteDark()
            {
                Primary = Colors.Blue.Default,
                Secondary = Colors.Green.Accent4,
                AppbarBackground = Colors.Blue.Default,
            },
        };
    }
}
