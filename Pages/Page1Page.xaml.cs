using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView.WinUI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace ValenceWinUI.Pages;

public sealed partial class Page1Page : Page
{
    public record DrugScheme(string Name, string[] Drugs, string DisplayMode);

    private static readonly List<DrugScheme> _schemes = new()
    {
        new("方案A - 全药物叠加", new[] { "药物A", "药物B", "药物C" }, "叠加显示"),
        new("方案B - 仅A/B对比", new[] { "药物A", "药物B" }, "独立纵轴"),
        new("方案C - 仅C",       new[] { "药物C" }, "单药"),
        new("方案D - 三药独立",  new[] { "药物A", "药物B", "药物C" }, "独立纵轴"),
        new("方案E - A+C",       new[] { "药物A", "药物C" }, "叠加显示"),
    };

    private DrugScheme? _currentScheme;

    public Page1Page()
    {
        this.InitializeComponent();
        this.NavigationCacheMode = Microsoft.UI.Xaml.Navigation.NavigationCacheMode.Required;
        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e) => ApplyScheme(_schemes[0]);

    private async void SchemeButton_Click(object sender, RoutedEventArgs e)
    {
        var searchBox = new AutoSuggestBox { PlaceholderText = "搜索方案...", Width = 320, Margin = new Thickness(0, 0, 0, 8) };
        var listView = new ListView { ItemsSource = new ObservableCollection<DrugScheme>(_schemes), DisplayMemberPath = "Name", MaxHeight = 300 };

        searchBox.TextChanged += (s, args) =>
        {
            var q = searchBox.Text?.Trim() ?? "";
            listView.ItemsSource = string.IsNullOrEmpty(q)
                ? new ObservableCollection<DrugScheme>(_schemes)
                : new ObservableCollection<DrugScheme>(_schemes.Where(s => s.Name.Contains(q, StringComparison.OrdinalIgnoreCase)));
        };

        var dialog = new ContentDialog
        {
            XamlRoot = this.XamlRoot, Title = "选择方案", PrimaryButtonText = "确定", CloseButtonText = "取消",
            DefaultButton = ContentDialogButton.Primary, Content = new StackPanel { Children = { searchBox, listView } }
        };

        var result = await dialog.ShowAsync();
        if (result == ContentDialogResult.Primary && listView.SelectedItem is DrugScheme scheme)
            ApplyScheme(scheme);
    }

    private void ApplyScheme(DrugScheme scheme)
    {
        _currentScheme = scheme;
        SchemeButtonLabel.Text = $"方案: {scheme.Name}";
        CurrentSchemeText.Text = $"{scheme.Drugs.Length} 种药物 · {scheme.DisplayMode}";

        var colors = new[] { SKColors.DodgerBlue, SKColors.OrangeRed, SKColors.ForestGreen };
        var newSeries = new LiveChartsCore.SkiaSharpView.WinUI.SeriesCollection();

        for (int i = 0; i < scheme.Drugs.Length; i++)
        {
            var pts = GenerateCurve(i);
            newSeries.Add(new LineSeries<DateTimePoint>
            {
                Values = pts,
                Name = scheme.Drugs[i],
                Stroke = new SolidColorPaint(colors[i % colors.Length], 2),
                GeometryFill = new SolidColorPaint(colors[i % colors.Length]),
                GeometryStroke = new SolidColorPaint(colors[i % colors.Length], 2),
                GeometrySize = 6,
                Fill = null,
                LineSmoothness = 0.3
            });
        }

        DrugChart.XAxes = new Axis[]
        {
            new Axis
            {
                Labeler = v => new DateTime((long)v).ToString("HH:mm"),
                UnitWidth = TimeSpan.FromHours(1).Ticks,
            }
        };
        DrugChart.YAxes = new Axis[] { new Axis { Name = "浓度 (ng/mL)", MinLimit = 0 } };

        DrugChart.Series = newSeries;

        BuildFeaturePanel(scheme);
    }

    private static ObservableCollection<DateTimePoint> GenerateCurve(int seed)
    {
        var rng = new Random(42 + seed);
        var pts = new ObservableCollection<DateTimePoint>();
        var baseTime = new DateTime(2026, 7, 30, 8, 0, 0);
        double a = 30 + seed * 25 + rng.NextDouble() * 20;

        for (int h = 0; h <= 24; h++)
        {
            double t = h / 24.0;
            double conc = a * (1.2 * Math.Exp(-3 * t) * (1 - Math.Exp(-12 * t)) +
                0.6 * Math.Exp(-2 * (t - 0.35)) * (t > 0.35 ? 1 : 0) * (1 - Math.Exp(-8 * (t - 0.35))))
                * (1 + 0.05 * Math.Sin(t * 20));
            pts.Add(new DateTimePoint(baseTime.AddHours(h), Math.Max(0, conc)));
        }
        return pts;
    }

    private void BuildFeaturePanel(DrugScheme scheme)
    {
        FeaturePanel.Children.Clear();
        FeaturePanel.Children.Add(new TextBlock { Text = "曲线特征", Style = (Style)Application.Current.Resources["BodyStrongTextBlockStyle"], Margin = new Thickness(0, 0, 0, 8) });

        for (int i = 0; i < scheme.Drugs.Length; i++)
        {
            var rng = new Random(42 + i);
            var card = new Border { Style = (Style)this.Resources["FeatureCardStyle"], Child = new StackPanel { Spacing = 4 } };
            var st = (StackPanel)card.Child;
            st.Children.Add(new TextBlock { Text = scheme.Drugs[i], Style = (Style)Application.Current.Resources["BodyStrongTextBlockStyle"] });
            st.Children.Add(new TextBlock { Text = $"Cmax: {40 + i * 30 + rng.NextDouble() * 25:F1} ng/mL" });
            st.Children.Add(new TextBlock { Text = $"Tmax: {1.5 + i * 0.8 + rng.NextDouble():F1} h" });
            st.Children.Add(new TextBlock { Text = $"AUC₀₋₂₄: {200 + i * 180 + rng.NextDouble() * 100:F1} ng·h/mL" });
            st.Children.Add(new TextBlock { Text = $"t½: {3 + i * 1.5 + rng.NextDouble() * 2:F1} h" });
            FeaturePanel.Children.Add(card);
        }
    }
}
