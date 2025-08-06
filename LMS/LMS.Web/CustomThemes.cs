using MudBlazor;
using MudBlazor.Utilities;

namespace LMS
{
    public static class CustomThemes
    {
        public static readonly Typography DefaultTypography = new Typography()
        {
            H1 = new H1Typography()
            {
                FontFamily = ["Roboto", "Helvetica", "Arial", "sans-serif"],
                FontSize = "6rem",
                FontWeight = "300",
                LineHeight = "1.167",
                LetterSpacing = "-0.01562em"
            },
            H2 = new H2Typography()
            {
                FontFamily = ["Roboto", "Helvetica", "Arial", "sans-serif"],
                FontSize = "3.75rem",
                FontWeight = "300",
                LineHeight = "1.2",
                LetterSpacing = "-0.00833em"
            },
            H3 = new H3Typography()
            {
                FontFamily = ["Roboto", "Helvetica", "Arial", "sans-serif"],
                FontSize = "3rem",
                FontWeight = "400",
                LineHeight = "1.04",
                LetterSpacing = "0em"
            },
            H4 = new H4Typography()
            {
                FontFamily = ["Roboto", "Helvetica", "Arial", "sans-serif"],
                FontSize = "2.125rem",
                FontWeight = "400",
                LineHeight = "1.235",
                LetterSpacing = "0.00735em"
            },
            H5 = new H5Typography()
            {
                FontFamily = ["Roboto", "Helvetica", "Arial", "sans-serif"],
                FontSize = "1.5rem",
                FontWeight = "400",
                LineHeight = "1.334",
                LetterSpacing = "0em"
            },
            H6 = new H6Typography()
            {
                FontFamily = ["Roboto", "Helvetica", "Arial", "sans-serif"],
                FontSize = "1.25rem",
                FontWeight = "500",
                LineHeight = "1.6",
                LetterSpacing = "0.0075em"
            },
            Subtitle1 = new Subtitle1Typography()
            {
                FontFamily = ["Roboto", "Helvetica", "Arial", "sans-serif"],
                FontSize = "1rem",
                FontWeight = "400",
                LineHeight = "1.75",
                LetterSpacing = "0.00938em"
            },
            Subtitle2 = new Subtitle2Typography()
            {
                FontFamily = ["Roboto", "Helvetica", "Arial", "sans-serif"],
                FontSize = "0.875rem",
                FontWeight = "500",
                LineHeight = "1.57",
                LetterSpacing = "0.00714em"
            },
            Body1 = new Body1Typography()
            {
                FontFamily = ["Roboto", "Helvetica", "Arial", "sans-serif"],
                FontSize = "1rem",
                FontWeight = "400",
                LineHeight = "1.5",
                LetterSpacing = "0.00938em"
            },
            Body2 = new Body2Typography()
            {
                FontFamily = ["Roboto", "Helvetica", "Arial", "sans-serif"],
                FontSize = "0.875rem",
                FontWeight = "400",
                LineHeight = "1.5",
                LetterSpacing = "0.01071em"
            },
            Button = new ButtonTypography()
            {
                FontFamily = ["Roboto", "Helvetica", "Arial", "sans-serif"],
                FontSize = "0.875rem",
                FontWeight = "500",
                LineHeight = "1.75",
                LetterSpacing = "0.02857em",
                TextTransform = "uppercase"
            },
            Caption = new CaptionTypography()
            {
                FontFamily = ["Roboto", "Helvetica", "Arial", "sans-serif"],
                FontSize = "0.75rem",
                FontWeight = "400",
                LineHeight = "1.66",
                LetterSpacing = "0.03333em"
            },
            Overline = new OverlineTypography()
            {
                FontFamily = ["Roboto", "Helvetica", "Arial", "sans-serif"],
                FontSize = "0.75rem",
                FontWeight = "400",
                LineHeight = "2.66",
                LetterSpacing = "0.08333em",
                TextTransform = "uppercase"
            }
        };

        public static readonly LayoutProperties DefaultLayoutProperties = new LayoutProperties()
        {
            DefaultBorderRadius = "4px",
            DrawerMiniWidthLeft = "56px",
            DrawerMiniWidthRight = "56px",
            DrawerWidthLeft = "256px",
            DrawerWidthRight = "256px",
            AppbarHeight = "64px"
        };

        public static readonly ZIndex DefaultZIndex = new ZIndex()
        {
            Drawer = 1100,
            AppBar = 1200,
            Snackbar = 1300,
            Dialog = 1400,
            Tooltip = 1500,
            Popover = 1600
        };

        public static readonly Shadow DefaultShadows = new Shadow()
        {
            Elevation = new string[] {
                        "none",
                        "0 5px 5px -3px rgba(0,0,0,.06), 0 8px 10px 1px rgba(0,0,0,.042), 0 3px 14px 2px rgba(0,0,0,.036)",
                        "0px 3px 1px -2px rgba(0,0,0,0.2),0px 2px 2px 0px rgba(0,0,0,0.14),0px 1px 5px 0px rgba(0,0,0,0.12)",
                        "0px 3px 3px -2px rgba(0,0,0,0.2),0px 3px 4px 0px rgba(0,0,0,0.14),0px 1px 8px 0px rgba(0,0,0,0.12)",
                        "0px 2px 4px -1px rgba(0,0,0,0.2),0px 4px 5px 0px rgba(0,0,0,0.14),0px 1px 10px 0px rgba(0,0,0,0.12)",
                        "0px 3px 5px -1px rgba(0,0,0,0.2),0px 5px 8px 0px rgba(0,0,0,0.14),0px 1px 14px 0px rgba(0,0,0,0.12)",
                        "0px 3px 5px -1px rgba(0,0,0,0.2),0px 6px 10px 0px rgba(0,0,0,0.14),0px 1px 18px 0px rgba(0,0,0,0.12)",
                        "0px 4px 5px -2px rgba(0,0,0,0.2),0px 7px 10px 1px rgba(0,0,0,0.14),0px 2px 16px 1px rgba(0,0,0,0.12)",
                        "0px 5px 5px -3px rgba(0,0,0,0.2),0px 8px 10px 1px rgba(0,0,0,0.14),0px 3px 14px 2px rgba(0,0,0,0.12)",
                        "0px 5px 6px -3px rgba(0,0,0,0.2),0px 9px 12px 1px rgba(0,0,0,0.14),0px 3px 16px 2px rgba(0,0,0,0.12)",
                        "0px 6px 6px -3px rgba(0,0,0,0.2),0px 10px 14px 1px rgba(0,0,0,0.14),0px 4px 18px 3px rgba(0,0,0,0.12)",
                        "0px 6px 7px -4px rgba(0,0,0,0.2),0px 11px 15px 1px rgba(0,0,0,0.14),0px 4px 20px 3px rgba(0,0,0,0.12)",
                        "0px 7px 8px -4px rgba(0,0,0,0.2),0px 12px 17px 2px rgba(0,0,0,0.14),0px 5px 22px 4px rgba(0,0,0,0.12)",
                        "0px 7px 8px -4px rgba(0,0,0,0.2),0px 13px 19px 2px rgba(0,0,0,0.14),0px 5px 24px 4px rgba(0,0,0,0.12)",
                        "0px 7px 9px -4px rgba(0,0,0,0.2),0px 14px 21px 2px rgba(0,0,0,0.14),0px 5px 26px 4px rgba(0,0,0,0.12)",
                        "0px 8px 9px -5px rgba(0,0,0,0.2),0px 15px 22px 2px rgba(0,0,0,0.14),0px 6px 28px 5px rgba(0,0,0,0.12)",
                        "0px 8px 10px -5px rgba(0,0,0,0.2),0px 16px 24px 2px rgba(0,0,0,0.14),0px 6px 30px 5px rgba(0,0,0,0.12)",
                        "0px 8px 11px -5px rgba(0,0,0,0.2),0px 17px 26px 2px rgba(0,0,0,0.14),0px 6px 32px 5px rgba(0,0,0,0.12)",
                        "0px 9px 11px -5px rgba(0,0,0,0.2),0px 18px 28px 2px rgba(0,0,0,0.14),0px 7px 34px 6px rgba(0,0,0,0.12)",
                        "0px 9px 12px -6px rgba(0,0,0,0.2),0px 19px 29px 2px rgba(0,0,0,0.14),0px 7px 36px 6px rgba(0,0,0,0.12)",
                        "0px 10px 13px -6px rgba(0,0,0,0.2),0px 20px 31px 3px rgba(0,0,0,0.14),0px 8px 38px 7px rgba(0,0,0,0.12)",
                        "0px 10px 13px -6px rgba(0,0,0,0.2),0px 21px 33px 3px rgba(0,0,0,0.14),0px 8px 40px 7px rgba(0,0,0,0.12)",
                        "0px 10px 14px -6px rgba(0,0,0,0.2),0px 22px 35px 3px rgba(0,0,0,0.14),0px 8px 42px 7px rgba(0,0,0,0.12)",
                        "0px 11px 14px -7px rgba(0,0,0,0.2),0px 23px 36px 3px rgba(0,0,0,0.14),0px 9px 44px 8px rgba(0,0,0,0.12)",
                        "0px 11px 15px -7px rgba(0,0,0,0.2),0px 24px 38px 3px rgba(0,0,0,0.14),0px 9px 46px 8px rgba(0,0,0,0.12)",
                        "0 5px 5px -3px rgba(0,0,0,.06), 0 8px 10px 1px rgba(0,0,0,.042), 0 3px 14px 2px rgba(0,0,0,.036)" 
            }
        
        };

        public static MudTheme LightTheme = new MudTheme()
        {
            PaletteLight = new PaletteLight()
            {
                Black = "rgba(39,44,52,1)",
                White = "rgba(255,255,255,1)",
                Primary = "rgba(89,74,226,1)",
                PrimaryContrastText = "rgba(255,255,255,1)",
                Secondary = "rgba(255,64,129,1)",
                SecondaryContrastText = "rgba(255,255,255,1)",
                Tertiary = "rgba(30,200,165,1)",
                TertiaryContrastText = "rgba(255,255,255,1)",
                Info = "rgba(33,150,243,1)",
                InfoContrastText = "rgba(255,255,255,1)",
                Success = "rgba(0,200,83,1)",
                SuccessContrastText = "rgba(255,255,255,1)",
                Warning = "rgba(255,152,0,1)",
                WarningContrastText = "rgba(255,255,255,1)",
                Error = "rgba(244,67,54,1)",
                ErrorContrastText = "rgba(255,255,255,1)",
                Dark = "rgba(66,66,66,1)",
                DarkContrastText = "rgba(255,255,255,1)",
                TextPrimary = "rgba(66,66,66,1)",
                TextSecondary = "rgba(0,0,0,0.5372549019607843)",
                TextDisabled = "rgba(0,0,0,0.3764705882352941)",
                ActionDefault = "rgba(0,0,0,0.5372549019607843)",
                ActionDisabled = "rgba(0,0,0,0.25882352941176473)",
                ActionDisabledBackground = "rgba(0,0,0,0.11764705882352941)",
                Background = "rgba(255,255,255,1)",
                BackgroundGray = "rgba(245,245,245,1)",
                Surface = "rgba(255,255,255,1)",
                DrawerBackground = "rgba(255,255,255,1)",
                DrawerText = "rgba(66,66,66,1)",
                DrawerIcon = "rgba(97,97,97,1)",
                AppbarBackground = "rgba(89,74,226,1)",
                AppbarText = "rgba(255,255,255,1)",
                LinesDefault = "rgba(0,0,0,0.11764705882352941)",
                LinesInputs = "rgba(189,189,189,1)",
                TableLines = "rgba(224,224,224,1)",
                TableStriped = "rgba(0,0,0,0.0196078431372549)",
                TableHover = "rgba(0,0,0,0.0392156862745098)",
                Divider = "rgba(224,224,224,1)",
                DividerLight = "rgba(0,0,0,0.8)",
                Skeleton = "rgba(0,0,0,0.10980392156862745)",
                PrimaryDarken = "rgb(62,44,221)",
                PrimaryLighten = "rgb(118,106,231)",
                SecondaryDarken = "rgb(255,31,105)",
                SecondaryLighten = "rgb(255,102,153)",
                TertiaryDarken = "rgb(25,169,140)",
                TertiaryLighten = "rgb(42,223,187)",
                InfoDarken = "rgb(12,128,223)",
                InfoLighten = "rgb(71,167,245)",
                SuccessDarken = "rgb(0,163,68)",
                SuccessLighten = "rgb(0,235,98)",
                WarningDarken = "rgb(214,129,0)",
                WarningLighten = "rgb(255,167,36)",
                ErrorDarken = "rgb(242,28,13)",
                ErrorLighten = "rgb(246,96,85)",
                DarkDarken = "rgb(46,46,46)",
                DarkLighten = "rgb(87,87,87)",
                BorderOpacity = 1,
                HoverOpacity = 0.06f,
                RippleOpacity = 0.1f,
                RippleOpacitySecondary = 0.2f,
                GrayDefault = "#9E9E9E",
                GrayLight = "#BDBDBD",
                GrayLighter = "#E0E0E0",
                GrayDark = "#757575",
                GrayDarker = "#616161",
                OverlayDark = "rgba(33,33,33,0.4980392156862745)",
                OverlayLight = "rgba(255,255,255,0.4980392156862745)"
            },
            ZIndex = DefaultZIndex,
            LayoutProperties = DefaultLayoutProperties,
            Shadows = DefaultShadows,
            Typography = DefaultTypography
        };

        public static MudTheme DarkTheme = new MudTheme()
        {
            PaletteDark = new PaletteDark()
            {
                Black = "rgba(39,39,47,1)",
                White = "rgba(255,255,255,1)",
                Primary = "rgba(119,107,231,1)",
                PrimaryContrastText = "rgba(255,255,255,1)",
                Secondary = "rgba(255,64,129,1)",
                SecondaryContrastText = "rgba(255,255,255,1)",
                Tertiary = "rgba(30,200,165,1)",
                TertiaryContrastText = "rgba(255,255,255,1)",
                Info = "rgba(50,153,255,1)",
                InfoContrastText = "rgba(255,255,255,1)",
                Success = "rgba(11,186,131,1)",
                SuccessContrastText = "rgba(255,255,255,1)",
                Warning = "rgba(255,168,0,1)",
                WarningContrastText = "rgba(255,255,255,1)",
                Error = "rgba(246,78,98,1)",
                ErrorContrastText = "rgba(255,255,255,1)",
                Dark = "rgba(39,39,47,1)",
                DarkContrastText = "rgba(255,255,255,1)",
                TextPrimary = "rgba(255,255,255,0.6980392156862745)",
                TextSecondary = "rgba(255,255,255,0.4980392156862745)",
                TextDisabled = "rgba(255,255,255,0.2)",
                ActionDefault = "rgba(173,173,177,1)",
                ActionDisabled = "rgba(255,255,255,0.25882352941176473)",
                ActionDisabledBackground = "rgba(255,255,255,0.11764705882352941)",
                Background = "rgba(50,51,61,1)",
                BackgroundGray = "rgba(39,39,47,1)",
                Surface = "rgba(55,55,64,1)",
                DrawerBackground = "rgba(39,39,47,1)",
                DrawerText = "rgba(255,255,255,0.4980392156862745)",
                DrawerIcon = "rgba(255,255,255,0.4980392156862745)",
                AppbarBackground = "rgba(39,39,47,1)",
                AppbarText = "rgba(255,255,255,0.6980392156862745)",
                LinesDefault = "rgba(255,255,255,0.11764705882352941)",
                LinesInputs = "rgba(255,255,255,0.2980392156862745)",
                TableLines = "rgba(255,255,255,0.11764705882352941)",
                TableStriped = "rgba(255,255,255,0.2)",
                TableHover = "rgba(255,255,255,0.2)",
                Divider = "rgba(255,255,255,0.11764705882352941)",
                DividerLight = "rgba(255,255,255,0.058823529411764705)",
                Skeleton = "rgba(255,255,255,0.10980392156862745)",
                PrimaryDarken = "rgb(90,75,226)",
                PrimaryLighten = "rgb(151,141,236)",
                SecondaryDarken = "rgb(255,31,105)",
                SecondaryLighten = "rgb(255,102,153)",
                TertiaryDarken = "rgb(25,169,140)",
                TertiaryLighten = "rgb(42,223,187)",
                InfoDarken = "rgb(10,133,255)",
                InfoLighten = "rgb(92,173,255)",
                SuccessDarken = "rgb(9,154,108)",
                SuccessLighten = "rgb(13,222,156)",
                WarningDarken = "rgb(214,143,0)",
                WarningLighten = "rgb(255,182,36)",
                ErrorDarken = "rgb(244,47,70)",
                ErrorLighten = "rgb(248,119,134)",
                DarkDarken = "rgb(23,23,28)",
                DarkLighten = "rgb(56,56,67)",
                BorderOpacity = 1,
                HoverOpacity = 0.06f,
                RippleOpacity = 0.1f,
                RippleOpacitySecondary = 0.2f,
                GrayDefault = "#9E9E9E",
                GrayLight = "#BDBDBD",
                GrayLighter = "#E0E0E0",
                GrayDark = "#757575",
                GrayDarker = "#616161",
                OverlayDark = "rgba(33,33,33,0.4980392156862745)",
                OverlayLight = "rgba(255,255,255,0.4980392156862745)"
            },
            Typography = DefaultTypography,
            LayoutProperties = DefaultLayoutProperties,
            ZIndex = DefaultZIndex,
            Shadows = DefaultShadows
        };
    }
}