using System.ComponentModel;
using System.Drawing;

using DevExpress.Utils;
using DevExpress.XtraEditors;

using ColorUtils = PT.Common.ColorUtils;

namespace PT.ComponentLibrary.Controls;

public partial class ColorPickerBoxControl : PTBaseControl
{
    public ColorPickerBoxControl()
    {
        InitializeComponent();
    }

    [Browsable(true)]
    [Category("ColoBox")]
    public event Action ColorChanged;

    [Browsable(true)]
    [Category("ColoBox")]
    public Color Color
    {
        get => colorPickEdit_Color.Color;
        set
        {
            colorPickEdit_Color.Color = value;
            SetLabelColor(value);
        }
    }

    [Browsable(true)]
    [Category("ColoBox")]
    public override string Text
    {
        set => labelControl_Main.Text = value;
        get => labelControl_Main.Text;
    }

    [Browsable(true)]
    [Category("ColoBox")]
    public SuperToolTip Tooltip
    {
        set => labelControl_Main.SuperTip = value;
        get => labelControl_Main.SuperTip;
    }

    [Browsable(true)]
    [Category("ColoBox")]
    public LabelControl LabelControl => labelControl_Main;

    private void LabelControl_Main_Click(object sender, EventArgs e)
    {
        colorPickEdit_Color.ShowPopup();
    }

    private void ColorPickEdit_Color_ColorChanged(object sender, EventArgs e)
    {
        SetLabelColor(colorPickEdit_Color.Color);
        ColorChanged?.Invoke();
    }

    private void SetLabelColor(Color a_newColor)
    {
        labelControl_Main.Appearance.BackColor = a_newColor;
        labelControl_Main.Appearance.ForeColor = ColorUtils.GetVisibleDrawColor(a_newColor);
    }

    public override void Localize()
    {
        UILocalizationHelper.LocalizeControlsRecursively(Controls);
    }
}