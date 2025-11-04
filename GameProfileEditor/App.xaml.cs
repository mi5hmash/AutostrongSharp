using System.Windows;
using System.Windows.Media;
using Mi5hmasH.WpfHelper;

namespace GameProfileEditor;

public partial class App
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        // Set the theme accent
        var colorAccent = new ColorAccentModel(
            Color.FromRgb(132, 117, 69),
            Color.FromRgb(157, 141, 82),
            Color.FromRgb(194, 185, 134),
            Color.FromRgb(233, 229, 176),
            Color.FromRgb(116, 99, 59),
            Color.FromRgb(78, 59, 35),
            Color.FromRgb(43, 21, 12));
        WpfThemeAccent.SetThemeAccent(colorAccent);
    }
}