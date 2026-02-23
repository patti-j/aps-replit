using System.Windows.Forms;

using DevExpress.LookAndFeel;
using DevExpress.Skins;
using DevExpress.Utils.Drawing;
using DevExpress.XtraLayout;
using DevExpress.XtraLayout.HitInfo;
using DevExpress.XtraLayout.Registrator;

namespace PT.ComponentLibrary.Controls;

public class CleanLayoutControl : LayoutControl
{
    public CleanLayoutControl()
    {
        MouseUp += layoutControl_Main_MouseUp;
        Root.GroupBordersVisible = false;
        OptionsView.HighlightFocusedItem = true;
    }

    protected override LayoutControlImplementor CreateILayoutControlImplementorCore()
    {
        return new MyLayoutControlImplementor(this);
    }

    private void layoutControl_Main_MouseUp(object sender, MouseEventArgs e)
    {
        BaseLayoutItemHitInfo hitInfo = CalcHitInfo(e.Location);
        if (hitInfo.HitType != LayoutItemHitTest.Item || e.Clicks != 1)
        {
            return;
        }

        LayoutGroup group = hitInfo.Item as LayoutGroup;
        if (group != null)
        {
            if (group.ViewInfo.BorderInfo.CaptionBounds.Contains(e.Location) && !group.ViewInfo.BorderInfo.ButtonBounds.Contains(e.Location))
            {
                group.Expanded = !group.Expanded;
            }
        }
    }
}

public class NoGroupBorderLayoutSkinPaintStyle : LayoutSkinPaintStyle
{
    private CleanLayoutControl _LayoutControl;

    public NoGroupBorderLayoutSkinPaintStyle(ISupportLookAndFeel lookAndFeelOwner)
        : base(lookAndFeelOwner)
    {
        _LayoutControl = lookAndFeelOwner as CleanLayoutControl;
    }

    public override GroupObjectPainter CreateGroupPainter(IPanelControlOwner owner)
    {
        return new NoBorderSkinGroupObjectPainter(owner, LookAndFeel);
    }
}

public class MyLayoutControlImplementor : LayoutControlImplementor
{
    public MyLayoutControlImplementor(ILayoutControlOwner owner) : base(owner) { }

    protected override void InitializePaintStyles()
    {
        ISupportLookAndFeel lookAndFeelOwner = owner.GetISupportLookAndFeel();
        if (lookAndFeelOwner != null)
        {
            PaintStyles.Add(new LayoutOffice2003PaintStyle(lookAndFeelOwner));
            PaintStyles.Add(new LayoutWindowsXPPaintStyle(lookAndFeelOwner));
            PaintStyles.Add(new NoGroupBorderLayoutSkinPaintStyle(lookAndFeelOwner));
            PaintStyles.Add(new Style3DPaintStyle(lookAndFeelOwner));
            PaintStyles.Add(new UltraFlatPaintStyle(lookAndFeelOwner));
            PaintStyles.Add(new FlatPaintStyle(lookAndFeelOwner));
        }

        lookAndFeelOwner = null;
    }
}

public class NoBorderSkinGroupObjectPainter : SkinGroupObjectPainter
{
    public NoBorderSkinGroupObjectPainter(IPanelControlOwner owner, ISkinProvider provider)
        : base(owner, provider) { }

    protected override void DrawCaption(GroupObjectInfoArgs info)
    {
        if (info.Expanded)
        {
            base.DrawCaption(info); //If expanded, draws the 
        }
        else
        {
            base.DrawCaptionText(info); //Draws the group text without border
        }

        base.DrawButtonsPanel(info); //Draws the collapse button
    }

    protected override void DrawCaptionText(GroupObjectInfoArgs info)
    {
        base.DrawCaptionText(info);
    }

    protected override void DrawBorder(GroupObjectInfoArgs info)
    {
        if (info.Expanded)
        {
            base.DrawBorder(info); //draws the bottom border.
        }
    }
}