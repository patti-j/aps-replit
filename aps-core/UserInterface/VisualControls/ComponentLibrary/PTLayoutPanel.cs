using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

using DevExpress.Utils.Extensions;
using DevExpress.Utils.Layout;

namespace PT.ComponentLibrary;

[ToolboxItem(true)]
public partial class PTLayoutPanel : PTBaseControl
{
    public PTLayoutPanel()
    {
        InitializeComponent();
    }

    public DockStyle DockPosition { get; set; }
    public bool HasExtensionControls { get; set; }
    public int MaxWidth { get; set; }

    public void InitPanel()
    {
        HasExtensionControls = true;

        if (DockPosition == DockStyle.Left)
        {
            stackPanel_Layout.LayoutDirection = StackPanelLayoutDirection.TopDown;
        }
        else if (DockPosition == DockStyle.Top)
        {
            stackPanel_Layout.LayoutDirection = StackPanelLayoutDirection.LeftToRight;
        }
    }

    public void AddControl(Control a_control)
    {
        if (!HasExtensionControls)
        {
            InitPanel();
        }

        if (DockPosition == DockStyle.Left)
        {
            int width = a_control.Width + a_control.Margin.Right + a_control.Margin.Left;
            Width = MaxWidth = width;
            MaximumSize = new Size(width + stackPanel_Layout.Margin.Right + stackPanel_Layout.Margin.Left, 0);
        }

        stackPanel_Layout.AddControl(a_control);
    }

    public List<Control> GetControls()
    {
        List<Control> list = new (stackPanel_Layout.Controls.Count);
        foreach (Control control in stackPanel_Layout.Controls)
        {
            list.Add(control);
        }

        return list;
    }

    public override void Localize()
    {
        UILocalizationHelper.LocalizeUserControl(this);
    }

    #region Resizing Handlers
    /// <summary>
    /// Updates the height of the LayoutControlItem extension control on the left side ptLayoutPanel
    /// </summary>
    /// <param name="a_sizeHeight"></param>
    public void UpdateHeight(int a_sizeHeight)
    {
        if (!HasExtensionControls)
        {
            return;
        }

        foreach (Control control in stackPanel_Layout.Controls)
        {
            control.Height = a_sizeHeight;
            control.MinimumSize = control.MaximumSize = new Size(control.Width, a_sizeHeight);
        }
    }

    /// <summary>
    /// Adjusts width and of the LayoutControlItem of the left side ptLayoutPanel based on the splitter position
    /// </summary>
    /// <param name="a_splitterPos"></param>
    public void AdjustWidth(int a_splitterPos)
    {
        if (!HasExtensionControls)
        {
            return;
        }

        foreach (Control control in stackPanel_Layout.Controls)
        {
            control.MinimumSize = control.MaximumSize = new Size(a_splitterPos, control.Height);
        }

        Width = a_splitterPos;
    }

    /// <summary>
    /// Handles the LayoutControlItem Visiblity property based on the changing visibility of the extension controls.
    /// </summary>
    /// <param name="a_control"></param>
    /// <param name="a_visible"></param>
    public void UpdateControlVisibility(Control a_control, bool a_visible)
    {
        if (DockPosition == DockStyle.Left)
        {
            if (a_visible)
            {
                stackPanel_Layout.Show();
            }
            else
            {
                stackPanel_Layout.Hide();
            }
        }
        else
        {
            foreach (Control control in stackPanel_Layout.Controls)
            {
                if (control == a_control)
                {
                    control.Visible = a_visible;
                }
            }
        }
    }
    #endregion
}