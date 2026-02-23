using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

using DevExpress.LookAndFeel;
using DevExpress.Skins;
using DevExpress.Utils;
using DevExpress.Utils.Drawing;
using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Controls;
using DevExpress.XtraEditors.Drawing;
using DevExpress.XtraEditors.ViewInfo;

using PT.PackageDefinitionsUI.Interfaces;

namespace PT.ComponentLibrary.Controls;

[ToolboxItem(true)]
public class HighImpactButton : SimpleButton
{
    public HighImpactButton()
    {
        base.AutoSize = true;
        ImageOptions.SvgImageSize = new Size(20, 20);
        ImageOptions.ImageToTextAlignment = ImageAlignToText.LeftCenter;
        ImageOptions.Location = ImageLocation.MiddleCenter;
        base.MinimumSize = new Size(0, 20);
        ImageOptions.ImageToTextIndent = 10;
        base.AllowFocus = false;
        ShowFocusRectangle = DefaultBoolean.False;
    }

    public enum EHighImpactButtonType
    {
        Save,
        Delete,
        Web,
        Default,
        None
    }

    /// <summary>
    /// Modify properties to correctly redraw a high impact button with no SVG image
    /// </summary>
    [Obsolete]
    public void SetNoImageProperties()
    {
        base.AutoSize = false;
        ImageOptions.ImageToTextAlignment = ImageAlignToText.None;
        ImageOptions.Location = ImageLocation.Default;
        ImageOptions.ImageToTextIndent = -1;
        Padding = new Padding(3, 0, 3, 0);
        Size = new Size(base.CalcBestSize().Width, 20);
    }

    /// <summary>
    /// Modify properties to correctly redraw a high impact button with no SVG image
    /// </summary>
    public void SetNoImagePropertiesLarge()
    {
        base.AutoSize = false;
        ImageOptions.ImageToTextAlignment = ImageAlignToText.None;
        ImageOptions.Location = ImageLocation.Default;
        ImageOptions.ImageToTextIndent = -1;
        Padding = new Padding(3, 0, 3, 0);
        Size = new Size(base.CalcBestSize().Width, 30);
    }

    public void SetWebStyleCaption(IDynamicSkin a_skin)
    {
        // Converting to this method to cease using the Obsolete Version. 
        SetNoImagePropertiesLarge();

        AppearanceHovered.ForeColor = a_skin.TextColor;
        AppearancePressed.ForeColor = Color.Red;

        Appearance.FontStyleDelta = FontStyle.Underline;

        AppearanceHovered.FontStyleDelta = FontStyle.Underline;
        AppearancePressed.FontStyleDelta = FontStyle.Underline;

        Cursor = Cursors.Hand;
    }

    public EHighImpactButtonType ButtonSkinElement { get; set; } = EHighImpactButtonType.Default;

    public string GetSkinElementName()
    {
        string skinElement;
        switch (ButtonSkinElement)
        {
            case EHighImpactButtonType.Save:
                skinElement = "HighImpactSave";
                break;
            case EHighImpactButtonType.Delete:
                skinElement = "HighImpactDelete";
                break;
            case EHighImpactButtonType.Web:
            case EHighImpactButtonType.Default:
                skinElement = "HighImpactDefault";
                break;
            case EHighImpactButtonType.None:
                skinElement = CommonSkins.SkinButton;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        return skinElement;
    }

    protected override BaseStyleControlViewInfo CreateViewInfo()
    {
        if (RuntimeStatus.IsRuntime && ButtonSkinElement != EHighImpactButtonType.None)
        {
            return new HighImpactButtonViewInfo(this);
        }

        return base.CreateViewInfo();
    }
}

public class HighImpactButtonViewInfo : SimpleButtonViewInfo
{
    public HighImpactButtonViewInfo(SimpleButton a_button) : base(a_button) { }

    protected override EditorButtonPainter GetButtonPainter()
    {
        if (OwnerControl.BorderStyle == BorderStyles.Default && OwnerControl.LookAndFeel.ActiveStyle == ActiveLookAndFeelStyle.Skin)
        {
            return new HighImpactButtonPainter(OwnerControl.LookAndFeel.ActiveLookAndFeel, OwnerControl);
        }

        return base.GetButtonPainter();
    }

    protected override Font GetDefaultFont()
    {
        if (!IsSkinLookAndFeel || OwnerControl.IsDesignMode)
        {
            return base.GetDefaultFont();
        }

        HighImpactButton ownerControl = (HighImpactButton)OwnerControl;
        SkinElement element = CommonSkins.GetSkin(LookAndFeel.ActiveLookAndFeel)[ownerControl.GetSkinElementName()];

        return element.GetFont(ownerControl.Font, OwnerControl.LookAndFeel.ActiveLookAndFeel);
    }

    protected override Color GetForeColor()
    {
        if (OwnerControl.IsDesignMode)
        {
            return base.GetForeColor();
        }

        if (!IsSkinLookAndFeel)
        {
            return GetSystemColor(SystemColors.ControlText);
        }

        if (ShouldUseSystemColors)
        {
            return GetSystemForeColor();
        }

        ObjectState currentState = State;

        //if (State != ObjectState.Disabled)
        //{
        //    currentState |= ObjectState.Selected;
        //}

        HighImpactButton ownerControl = (HighImpactButton)OwnerControl;
        SkinElement element = CommonSkins.GetSkin(LookAndFeel.ActiveLookAndFeel)[ownerControl.GetSkinElementName()];

        return element.GetForeColor(currentState);
    }
}

public class HighImpactButtonPainter : SkinEditorButtonPainter
{
    private readonly Control m_ownerControl;

    public HighImpactButtonPainter(ISkinProvider a_provider, Control a_ownerControl) : base(a_provider)
    {
        m_ownerControl = a_ownerControl;
    }

    protected override SkinElement GetSkinElement(EditorButtonObjectInfoArgs a_e, ButtonPredefines a_kind)
    {
        if (m_ownerControl is HighImpactButton simpleButton && simpleButton.ButtonSkinElement != HighImpactButton.EHighImpactButtonType.Default)
        {
            SkinElement skinElement = CommonSkins.GetSkin(Provider)[simpleButton.GetSkinElementName()];
            return skinElement;
        }

        return base.GetSkinElement(a_e, a_kind);
    }
}