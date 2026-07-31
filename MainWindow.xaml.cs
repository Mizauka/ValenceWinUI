using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using ValenceWinUI.Helpers;
using ValenceWinUI.Pages;

namespace ValenceWinUI;

public sealed partial class MainWindow : Window
{
    private bool _isInitialNavigationDone;
    private bool _isSyncingSelection;
    private string? _currentPageKey;

    private readonly Dictionary<string, Type> _pageMap = new()
    {
        ["Page1"] = typeof(Page1Page),
        ["Page2"] = typeof(Page2Page),
        ["Page3"] = typeof(Page3Page),
        ["Page4"] = typeof(Page4Page),
        ["Settings"] = typeof(SettingsPage),
    };

    public MainWindow()
    {
        this.InitializeComponent();
        SetWindowProperties();
    }

    private void SetWindowProperties()
    {
        this.ExtendsContentIntoTitleBar = true;
        this.SetTitleBar(AppTitleBar);
        this.AppWindow.TitleBar.PreferredHeightOption = Microsoft.UI.Windowing.TitleBarHeightOption.Tall;

        RootGrid.ActualThemeChanged += (_, _) =>
            TitleBarHelper.ApplySystemThemeToCaptionButtons(this, RootGrid.ActualTheme);
    }

    private void RootGrid_Loaded(object sender, RoutedEventArgs e)
    {
        WindowHelper.SetWindowMinSize(this, 640, 500);
        TitleBarHelper.ApplySystemThemeToCaptionButtons(this, RootGrid.ActualTheme);

        if (!_isInitialNavigationDone)
        {
            _isInitialNavigationDone = true;
            NavigateByDirection("Page1");
            NavPage1.IsSelected = true;
        }
    }

    private void NavigationView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
    {
        if (!_isInitialNavigationDone) return;
        if (_isSyncingSelection) return;  // 程序化同步侧边栏时不触发导航

        if (args.SelectedItem is NavigationViewItem item && item.Tag is string tag)
        {
            NavigateByDirection(tag);
        }
    }

    private void NavigationView_DisplayModeChanged(NavigationView sender, NavigationViewDisplayModeChangedEventArgs args)
    {
        AppTitleBar.IsPaneToggleButtonVisible = sender.PaneDisplayMode != NavigationViewPaneDisplayMode.Top;
    }

    private void NavigationView_Loaded(object sender, RoutedEventArgs e)
    {
    }

    private void ContentFrame_Navigated(object sender, NavigationEventArgs e)
    {
        AppTitleBar.IsBackButtonVisible = ContentFrame.CanGoBack;
    }

    /// <summary>
    /// 侧边栏导航统一使用入场动画。返回按钮使用 GoBack 反向动画。
    /// </summary>
    private void NavigateByDirection(string pageKey)
    {
        if (!_pageMap.TryGetValue(pageKey, out var pageType))
            return;

        if (ContentFrame.Content?.GetType() == pageType)
            return;

        // 侧边栏点击统一用入场动画
        ContentFrame.Navigate(pageType, null, new EntranceNavigationTransitionInfo());
        _currentPageKey = pageKey;
    }

    private void TitleBar_BackRequested(TitleBar sender, object args)
    {
        if (!ContentFrame.CanGoBack) return;

        // 获取回退目标页面 ID
        var backEntry = ContentFrame.BackStack[ContentFrame.BackStackDepth - 1];
        string? backPageKey = null;
        foreach (var kv in _pageMap)
        {
            if (kv.Value == backEntry.SourcePageType)
            {
                backPageKey = kv.Key;
                break;
            }
        }

        ContentFrame.GoBack();

        // 同步侧边栏选中状态
        if (backPageKey != null)
        {
            _currentPageKey = backPageKey;
            SelectNavigationItem(backPageKey);
        }
    }

    /// <summary>同步侧边栏选中项（不触发 SelectionChanged 导航）</summary>
    private void SelectNavigationItem(string pageKey)
    {
        _isSyncingSelection = true;
        foreach (NavigationViewItem item in NavigationViewControl.MenuItems)
        {
            if (item.Tag is string tag && tag == pageKey)
            {
                item.IsSelected = true;
                break;
            }
        }
        _isSyncingSelection = false;
    }

    private void TitleBar_PaneToggleRequested(TitleBar sender, object args)
    {
        NavigationViewControl.IsPaneOpen = !NavigationViewControl.IsPaneOpen;
    }
}
