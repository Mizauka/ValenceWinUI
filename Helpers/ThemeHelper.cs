using Microsoft.UI.Xaml;

namespace ValenceWinUI.Helpers;

/// <summary>
/// 提供主题切换和恢复的功能
/// </summary>
public static class ThemeHelper
{
    private static ElementTheme _rootTheme = ElementTheme.Default;

    /// <summary>
    /// 获取当前根元素的实际主题
    /// </summary>
    public static ElementTheme RootTheme
    {
        get => _rootTheme;
        set
        {
            _rootTheme = value;
            // 应用主题到所有活动窗口的根元素
            foreach (Window window in WindowHelper.ActiveWindows)
            {
                if (window.Content is FrameworkElement rootElement)
                {
                    rootElement.RequestedTheme = value;
                }
            }
            // 持久化主题设置
            SaveThemeSetting(value);
        }
    }

    /// <summary>
    /// 初始化主题设置
    /// </summary>
    public static void Initialize()
    {
        RootTheme = LoadThemeSetting();
    }

    /// <summary>
    /// 判断当前是否为深色主题
    /// </summary>
    public static bool IsDarkTheme()
    {
        if (RootTheme == ElementTheme.Default)
        {
            return Application.Current.RequestedTheme == ApplicationTheme.Dark;
        }
        return RootTheme == ElementTheme.Dark;
    }

    private static void SaveThemeSetting(ElementTheme theme)
    {
        try
        {
            var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            localSettings.Values["AppTheme"] = theme.ToString();
        }
        catch
        {
            // 非打包应用可能无法访问 ApplicationData，忽略
        }
    }

    private static ElementTheme LoadThemeSetting()
    {
        try
        {
            var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            if (localSettings.Values.TryGetValue("AppTheme", out var value) && value is string themeStr)
            {
                return themeStr switch
                {
                    "Light" => ElementTheme.Light,
                    "Dark" => ElementTheme.Dark,
                    _ => ElementTheme.Default
                };
            }
        }
        catch
        {
            // 非打包应用可能无法访问 ApplicationData，忽略
        }
        return ElementTheme.Default;
    }
}
