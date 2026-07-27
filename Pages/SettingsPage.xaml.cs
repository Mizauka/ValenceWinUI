using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using ValenceWinUI.Helpers;

namespace ValenceWinUI.Pages;

public sealed partial class SettingsPage : Page
{
    private bool _isLoaded;

    public SettingsPage()
    {
        this.InitializeComponent();
        this.NavigationCacheMode = Microsoft.UI.Xaml.Navigation.NavigationCacheMode.Required;
        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (_isLoaded) return;
        _isLoaded = true;

        // 初始化主题 RadioButton
        switch (ThemeHelper.RootTheme)
        {
            case ElementTheme.Light:  ThemeLight.IsChecked  = true; break;
            case ElementTheme.Dark:   ThemeDark.IsChecked   = true; break;
            default:                  ThemeSystem.IsChecked = true; break;
        }
    }

    private void ThemeRadioButton_Checked(object sender, RoutedEventArgs e)
    {
        if (!_isLoaded) return;
        if (sender is not RadioButton radio || radio.Tag is not string tag) return;

        var theme = tag switch
        {
            "Light" => ElementTheme.Light,
            "Dark"  => ElementTheme.Dark,
            _       => ElementTheme.Default
        };

        ThemeHelper.RootTheme = theme;

        if (WindowHelper.GetWindowForElement(this) is Window window)
        {
            var resolved = theme == ElementTheme.Default
                ? (Application.Current.RequestedTheme == ApplicationTheme.Dark ? ElementTheme.Dark : ElementTheme.Light)
                : theme;
            TitleBarHelper.ApplySystemThemeToCaptionButtons(window, resolved);
        }
    }
}
