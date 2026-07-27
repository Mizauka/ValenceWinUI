using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using System.Collections.Generic;

namespace ValenceWinUI.Helpers;

/// <summary>
/// 窗口管理辅助类，跟踪所有活动窗口
/// </summary>
public static class WindowHelper
{
    private static readonly List<Window> _activeWindows = new();

    public static List<Window> ActiveWindows => _activeWindows;

    public static void TrackWindow(Window window)
    {
        window.Closed += (_, _) => _activeWindows.Remove(window);
        _activeWindows.Add(window);
    }

    public static Window? GetWindowForElement(UIElement element)
    {
        if (element.XamlRoot != null)
        {
            foreach (Window window in _activeWindows)
            {
                if (element.XamlRoot == window.Content.XamlRoot)
                {
                    return window;
                }
            }
        }
        return null;
    }

    public static void SetWindowMinSize(Window window, double width, double height)
    {
        if (window.Content is not FrameworkElement windowContent) return;
        if (windowContent.XamlRoot is null) return;
        if (window.AppWindow.Presenter is not OverlappedPresenter presenter) return;

        var scale = windowContent.XamlRoot.RasterizationScale;
        presenter.PreferredMinimumWidth = (int)(width * scale);
        presenter.PreferredMinimumHeight = (int)(height * scale);
    }
}
