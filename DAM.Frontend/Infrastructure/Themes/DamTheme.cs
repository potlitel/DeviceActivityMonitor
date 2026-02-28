//// 📁 DAM.Frontend/Infrastructure/Themes/DamTheme.cs
//using MudBlazor;
//using System;
//using static MudBlazor.CategoryTypes;

//namespace DAM.Frontend.Infrastructure.Themes;

///// <summary>
///// 🎨 Tema personalizado para DAM con colores corporativos
///// </summary>
//public static class DamTheme
//{
//    /// <summary>
//    /// ☀️ Tema claro (Light Mode)
//    /// </summary>
//    public static MudTheme LightTheme => new()
//    {
//        PaletteLight = new PaletteLight
//        {
//            Primary = "#2c3e50",      // Azul oscuro corporativo
//            Secondary = "#3498db",     // Azul claro
//            Tertiary = "#e74c3c",      // Rojo para warnings
//            Success = "#27ae60",       // Verde éxito
//            Warning = "#f39c12",       // Naranja advertencia
//            Error = "#e74c3c",        // Rojo error
//            Info = "#3498db",         // Azul información

//            Background = "#ffffff",    // Blanco puro
//            Surface = "#f8f9fa",      // Gris muy claro
//            AppbarBackground = "#2c3e50", // Azul oscuro
//            DrawerBackground = "#ffffff",

//            TextPrimary = "#2c3e50",
//            TextSecondary = "#7f8c8d",
//            TextDisabled = "#bdc3c7",

//            ActionDefault = "#7f8c8d",
//            ActionDisabled = "#bdc3c7",
//            ActionDisabledBackground = "#ecf0f1",

//            Divider = "#ecf0f1",
//            DividerLight = "#f8f9fa",

//            TableLines = "#ecf0f1",
//            TableStriped = "#f8f9fa",
//            TableHover = "#ecf0f1",

//            Lines = "#ecf0f1",
//            LinesInputs = "#bdc3c7",

//            PrimaryDarken = "#1e2b38",
//            PrimaryLighten = "#34495e",

//            HoverOpacity = 0.06
//        },
//        Typography = DefaultTypography,
//        LayoutProperties = DefaultLayout,
//        Shadows = DefaultShadows,
//        ZIndex = DefaultZIndex
//    };

//    /// <summary>
//    /// 🌙 Tema oscuro (Dark Mode)
//    /// </summary>
//    public static MudTheme DarkTheme => new()
//    {
//        PaletteDark = new PaletteDark
//        {
//            Primary = "#3498db",       // Azul claro (brilla en oscuro)
//            Secondary = "#2ecc71",     // Verde menta
//            Tertiary = "#e74c3c",      // Rojo
//            Success = "#27ae60",
//            Warning = "#f39c12",
//            Error = "#e74c3c",
//            Info = "#3498db",

//            Background = "#1a1a1a",    // Negro suave
//            Surface = "#2d2d2d",       // Gris oscuro
//            AppbarBackground = "#000000",
//            DrawerBackground = "#2d2d2d",

//            TextPrimary = "#ecf0f1",   // Blanco suave
//            TextSecondary = "#bdc3c7", // Gris claro
//            TextDisabled = "#7f8c8d",

//            ActionDefault = "#bdc3c7",
//            ActionDisabled = "#7f8c8d",
//            ActionDisabledBackground = "#34495e",

//            Divider = "#34495e",
//            DividerLight = "#2d2d2d",

//            TableLines = "#34495e",
//            TableStriped = "#2d2d2d",
//            TableHover = "#34495e",

//            Lines = "#34495e",
//            LinesInputs = "#7f8c8d",

//            PrimaryDarken = "#2980b9",
//            PrimaryLighten = "#5dade2",

//            HoverOpacity = 0.08,
//            GrayDark = "#ecf0f1",
//            GrayLight = "#7f8c8d",

//            OverlayDark = "rgba(0,0,0,0.7)",
//            OverlayLight = "rgba(255,255,255,0.1)"
//        },
//        Typography = DefaultTypography,
//        LayoutProperties = DefaultLayout,
//        Shadows = DefaultShadows,
//        ZIndex = DefaultZIndex
//    };

//    private static Typography DefaultTypography => new()
//    {
//        Default = new Default
//        {
//            FontFamily = new[] { "Inter", "Roboto", "Helvetica", "Arial", "sans-serif" },
//            FontSize = ".875rem",
//            FontWeight = 400,
//            LineHeight = 1.43,
//            LetterSpacing = ".01071em"
//        },
//        H1 = new H1
//        {
//            FontFamily = new[] { "Inter", "Roboto", "Helvetica", "Arial", "sans-serif" },
//            FontSize = "3rem",
//            FontWeight = 300,
//            LineHeight = 1.167,
//            LetterSpacing = "-.01562em"
//        },
//        H2 = new H2
//        {
//            FontSize = "2.5rem",
//            FontWeight = 300,
//            LineHeight = 1.2,
//            LetterSpacing = "-.00833em"
//        },
//        H3 = new H3
//        {
//            FontSize = "2rem",
//            FontWeight = 400,
//            LineHeight = 1.167,
//            LetterSpacing = "0"
//        },
//        H4 = new H4
//        {
//            FontSize = "1.5rem",
//            FontWeight = 400,
//            LineHeight = 1.235,
//            LetterSpacing = ".00735em"
//        },
//        H5 = new H5
//        {
//            FontSize = "1.25rem",
//            FontWeight = 400,
//            LineHeight = 1.334,
//            LetterSpacing = "0"
//        },
//        H6 = new H6
//        {
//            FontSize = "1rem",
//            FontWeight = 500,
//            LineHeight = 1.6,
//            LetterSpacing = ".0075em"
//        },
//        Button = new Button
//        {
//            FontSize = ".875rem",
//            FontWeight = 500,
//            LineHeight = 1.75,
//            LetterSpacing = ".02857em",
//            TextTransform = "none"
//        },
//        Caption = new Caption
//        {
//            FontSize = ".75rem",
//            FontWeight = 400,
//            LineHeight = 1.66,
//            LetterSpacing = ".03333em"
//        }
//    };

//    private static LayoutProperties DefaultLayout => new()
//    {
//        DefaultBorderRadius = "6px",
//        DrawerWidthLeft = "260px",
//        DrawerWidthRight = "260px",
//        AppbarHeight = "64px"
//    };

//    private static Shadow DefaultShadows => new();

//    private static ZIndex DefaultZIndex => new()
//    {
//        Appbar = 1200,
//        Drawer = 1300,
//        Dialog = 1400,
//        Popover = 1500,
//        Snackbar = 1600,
//        Tooltip = 1700
//    };
//}