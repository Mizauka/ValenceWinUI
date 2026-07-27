using Microsoft.UI;
using Microsoft.UI.Xaml;

namespace ValenceWinUI.Helpers;

/// <summary>
/// 提供自定义标题栏按钮颜色适配系统主题的功能
/// </summary>
internal static class TitleBarHelper
{
    // workaround: AppWindow TitleBar 在运行时更改主题时不会正确更新标题按钮颜色
    public static void ApplySystemThemeToCaptionButtons(Window window, ElementTheme currentTheme)
    {
        if (window.AppWindow is null) return;

        var foregroundColor = currentTheme == ElementTheme.Dark
            ? Colors.White
            : Colors.Black;
        window.AppWindow.TitleBar.ButtonForegroundColor = foregroundColor;
        window.AppWindow.TitleBar.ButtonHoverForegroundColor = foregroundColor;

        var backgroundHoverColor = currentTheme == ElementTheme.Dark
            ? Windows.UI.Color.FromArgb(24, 255, 255, 255)
            : Windows.UI.Color.FromArgb(24, 0, 0, 0);
        window.AppWindow.TitleBar.ButtonHoverBackgroundColor = backgroundHoverColor;
    }
}
