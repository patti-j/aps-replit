using System.Drawing;
using System.Windows.Forms;

using DevExpress.LookAndFeel;
using DevExpress.XtraEditors.Controls;
using DevExpress.XtraEditors.Repository;

namespace PT.PackageDefinitionsUI.ObjectProperties;

public static class PropertyColumnProgressBarHelper
{
    private static RepositoryItemProgressBar s_defaultPositiveBar;
    private static RepositoryItemProgressBar s_defaultNegativeBar;
    private static RepositoryItemProgressBar s_defaultNeutralBar;
    private static RepositoryItemProgressBar s_defaultWarningBar;

    public static RepositoryItemProgressBar DefaultPositiveBar => s_defaultPositiveBar ?? (s_defaultPositiveBar = GeneratePositiveBar());

    public static RepositoryItemProgressBar DefaultNegativeBar => s_defaultNegativeBar ?? (s_defaultNegativeBar = GenerateNegativeBar());

    public static RepositoryItemProgressBar DefaultNeutralBar => s_defaultNeutralBar ?? (s_defaultNeutralBar = GenerateNeutralBar());

    public static RepositoryItemProgressBar DefaultWarningBar => s_defaultWarningBar ?? (s_defaultWarningBar = GenerateWarningBar());

    public static RepositoryItemProgressBar GeneratePositiveBar()
    {
        RepositoryItemProgressBar bar = GenerateBaseBar();
        bar.LookAndFeel.UseDefaultLookAndFeel = false;
        bar.LookAndFeel.Style = LookAndFeelStyle.Flat;
        bar.StartColor = Color.Green;
        bar.EndColor = Color.Green;
        return bar;
    }

    public static RepositoryItemProgressBar GenerateNeutralBar()
    {
        RepositoryItemProgressBar bar = GenerateBaseBar();
        bar.ShowTitle = true;
        bar.ProgressPadding = new Padding(3);
        return bar;
    }

    public static RepositoryItemProgressBar GenerateNegativeBar()
    {
        RepositoryItemProgressBar bar = GenerateBaseBar();
        bar.LookAndFeel.UseDefaultLookAndFeel = false;
        bar.LookAndFeel.Style = LookAndFeelStyle.Flat;
        bar.StartColor = Color.Red;
        bar.EndColor = Color.Red;
        return bar;
    }

    public static RepositoryItemProgressBar GenerateWarningBar()
    {
        RepositoryItemProgressBar bar = GenerateBaseBar();
        bar.LookAndFeel.UseDefaultLookAndFeel = false;
        bar.LookAndFeel.Style = LookAndFeelStyle.Flat;
        bar.StartColor = Color.Red;
        bar.EndColor = Color.Yellow;
        return bar;
    }

    private static RepositoryItemProgressBar GenerateBaseBar()
    {
        RepositoryItemProgressBar bar = new ();
        bar.ShowTitle = true;
        bar.ProgressPadding = new Padding(3);
        bar.ProgressViewStyle = ProgressViewStyle.Solid;
        bar.PercentView = false;
        //TODO: This does not work. Can't center text for some reason
        //bar.DisplayFormat.FormatType = FormatType.Numeric;
        //bar.DisplayFormat.FormatString = "# \\%  ";
        //bar.Appearance.GetStringFormat().Alignment = StringAlignment.Center;
        //bar.AppearanceReadOnly.TextOptions.HAlignment = HorzAlignment.Center;
        //bar.Appearance.TextOptions.HAlignment = HorzAlignment.Center;
        bar.CustomDisplayText += BarOnCustomDisplayText;
        return bar;
    }

    private static void BarOnCustomDisplayText(object a_sender, CustomDisplayTextEventArgs a_e)
    {
        object value = a_e.Value;
        if (value is int intValue)
        {
            a_e.DisplayText = $"{intValue} % ";
        }
    }
}