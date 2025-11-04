using System.Windows;
using System.Windows.Media;
using Mi5hmasH.WpfHelper;

namespace AutostrongSharpWpf;
public partial class App
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        // Set the theme accent
        var colorAccent = new ColorAccentModel(
            Color.FromRgb(104, 118, 138),
            Color.FromRgb(124, 138, 156),
            Color.FromRgb(173, 187, 197),
            Color.FromRgb(217, 230, 234),
            Color.FromRgb(88, 101, 121),
            Color.FromRgb(52, 60, 81),
            Color.FromRgb(19, 23, 45));
        WpfThemeAccent.SetThemeAccent(colorAccent);
    }
}