namespace PT.APSCommon.Serialization;

/// <summary>
/// Stores settings that can be used to restore a form's layout upon reopening.
/// </summary>
public class FormLayout : IPTSerializable, ICloneable
{
    #region IPTSerializable Members
    public const int UNIQUE_ID = 448;

    public FormLayout(IReader reader)
    {
        if (reader.VersionNumber >= 607)
        {
            reader.Read(out top);
            reader.Read(out left);
            reader.Read(out width);
            reader.Read(out height);
            reader.Read(out maximized);
            m_bools = new BoolVector32(reader);
            reader.Read(out m_layoutId);
        }

        #region 292
        else if (reader.VersionNumber >= 292)
        {
            reader.Read(out top);
            reader.Read(out left);
            reader.Read(out width);
            reader.Read(out height);
            reader.Read(out maximized);
            m_bools = new BoolVector32(reader);
        }
        #endregion

        #region Version 1
        else if (reader.VersionNumber >= 1)
        {
            reader.Read(out top);
            reader.Read(out left);
            reader.Read(out width);
            reader.Read(out height);
            reader.Read(out maximized);
        }
        #endregion
    }

    public void Serialize(IWriter writer)
    {
        writer.Write(top);
        writer.Write(left);
        writer.Write(width);
        writer.Write(height);
        writer.Write(maximized);
        m_bools.Serialize(writer);
        writer.Write(m_layoutId);
    }

    public virtual int UniqueId => UNIQUE_ID;
    #endregion

    public FormLayout(string a_layoutId)
    {
        m_layoutId = a_layoutId;
        m_modified = true;
    }

    public FormLayout(FormLayout original)
    {
        top = original.Top;
        left = original.Left;
        width = original.Width;
        height = original.Height;
        maximized = original.Maximized;
        m_bools = new BoolVector32(original.m_bools);
        m_layoutId = original.LayoutId;
        m_modified = true;
    }

    public FormLayout(string a_layoutId, int a_top, int a_left, int a_width, int a_height, bool a_maximized)
    {
        m_layoutId = a_layoutId;
        top = a_top;
        left = a_left;
        width = a_width;
        height = a_height;
        maximized = a_maximized;
        m_modified = true;
    }

    private bool m_modified;
    public bool Modified => m_modified;

    private int top;
    public int Top => top;

    private int left;
    public int Left => left;

    private int width;
    public int Width => width;

    private int height;
    public int Height => height;

    private bool maximized;

    public bool Maximized
    {
        get => maximized;
        set
        {
            if (maximized != value)
            {
                maximized = value;
                m_modified = true;
            }
        }
    }

    private readonly string m_layoutId;
    public string LayoutId => m_layoutId;

    public void SetLocation(int a_left, int a_top)
    {
        if (left != a_left)
        {
            left = a_left;
            m_modified = true;
        }

        if (top != a_top)
        {
            top = a_top;
            m_modified = true;
        }
    }

    public void SetSize(int a_width, int a_height)
    {
        if (width != a_width)
        {
            width = a_width;
            m_modified = true;
        }

        if (height != a_height)
        {
            height = a_height;
            m_modified = true;
        }
    }

    private BoolVector32 m_bools;

    /// <summary>
    /// Can be used to store various settings for various forms.
    /// </summary>
    public void SetBool(int index, bool newValue)
    {
        m_bools[index] = newValue;
    }

    public bool GetBool(int index)
    {
        return m_bools[index];
    }

    /// <summary>
    /// Checks if the layout can be seen and dragged within any screen.
    /// If not, it will move and resize the layout to be visible on the primary screen.
    /// </summary>
    /// <param name="a_primaryScreen"></param>
    /// <param name="a_screens"></param>
    public void ValidateWindowOnScreen(System.Drawing.Rectangle a_primaryScreen, System.Drawing.Rectangle[] a_screens)
    {
        if (Maximized)
        {
            return;
        }

        foreach (System.Drawing.Rectangle screen in a_screens)
        {
            //Test if the top left area of the window is on a screen
            System.Drawing.Rectangle corner = new (Left, Top, 100, 100);
            if (screen.Contains(corner))
            {
                FitWindowSizeWithinScreen(screen);
                return;
            }

            //Test if the top right area of the window is on a screen
            corner = new System.Drawing.Rectangle(Left + Width - 100, Top, 100, 100);
            if (screen.Contains(corner))
            {
                FitWindowSizeWithinScreen(screen);
                return;
            }
        }

        FitWindowOnScreen(a_primaryScreen);
    }

    /// <summary>
    /// Fixes the layout so that it can be drawn within the screen.
    /// </summary>
    /// <param name="a_screen"></param>
    private void FitWindowOnScreen(System.Drawing.Rectangle a_screen)
    {
        left = a_screen.X + 10;
        top = a_screen.Y + 10;
        width = Math.Max(width, 300);
        height = Math.Max(height, 300);
        width = Math.Min(width, (int)(a_screen.Width * .85));
        height = Math.Min(height, (int)(a_screen.Height * .85));
    }

    /// <summary>
    /// Fixes the layout so that it can be drawn within the screen.
    /// </summary>
    /// <param name="a_screen"></param>
    private void FitWindowSizeWithinScreen(System.Drawing.Rectangle a_screen)
    {
        if (width > a_screen.Width)
        {
            left = a_screen.X + 10;
            width = (int)(a_screen.Width * .85);
        }

        if (height > a_screen.Height)
        {
            top = a_screen.Y + 10;
            height = (int)(a_screen.Height * .85);
        }
    }

    ///// <summary>
    ///// Indicates whether the layout will work with the current screen dimensions.
    ///// </summary>
    //public bool IsStillValid(int screenHeight, int screenWidth)
    //{
    //    return Maximized ||
    //        (
    //         PointIsOnScreen(screenWidth, screenHeight, Left, Top)
    //        && PointIsOnScreen(screenWidth, screenHeight, Left, Top + Height)
    //        && PointIsOnScreen(screenWidth, screenHeight, Left + Width, Top)
    //        && PointIsOnScreen(screenWidth, screenHeight, Left + Width, Top + Height));
    //}
    //bool PointIsOnScreen(int screenWidth, int screenHeight, int x, int y)
    //{
    //    return x > 0 && x < screenWidth && y > 0 && y < screenHeight;
    //}
    public object Clone()
    {
        return new FormLayout(this);
    }

    public override bool Equals(object obj)
    {
        return LayoutId == (obj as FormLayout)?.LayoutId;
    }

    public override int GetHashCode()
    {
        if (string.IsNullOrEmpty(LayoutId))
        {
            return base.GetHashCode();
        }

        return LayoutId.GetHashCode();
    }
}