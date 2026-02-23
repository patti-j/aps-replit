using System.Drawing;

using PT.APSCommon;

namespace PT.SystemDefinitions.Flags;

/// <summary>
/// Stores a list of conditions that the user will be alerted to.
/// </summary>
public class Flag : IPTSerializable
{
    #region IPTSerializable Members
    public Flag(IReader reader)
    {
        if (reader.VersionNumber >= 1)
        {
            reader.Read(out description);
            reader.Read(out category);
            reader.Read(out colorCode);
            reader.Read(out priority);
            id = new BaseId(reader);
        }
    }

    public void Serialize(IWriter writer)
    {
        writer.Write(description);
        writer.Write(category);
        writer.Write(colorCode);
        writer.Write(priority);
        id.Serialize(writer);
    }

    public int UniqueId => 579;
    #endregion

    public Flag(string description)
    {
        this.description = description;
    }

    private BaseId id;

    public BaseId Id => id;

    internal void SetId(BaseId id)
    {
        this.id = id;
    }

    private string description;

    /// <summary>
    /// Describes the nature of the flag.
    /// </summary>
    public string Description
    {
        get => description;
        set => description = value;
    }

    private string category;

    /// <summary>
    /// Can be used for grouping/sorting Flags.
    /// </summary>
    public string Category
    {
        get => category;
        set => category = value;
    }

    private Color colorCode;

    /// <summary>
    /// Visual indicator of the nature or priority of the Flag.
    /// </summary>
    public Color ColorCode
    {
        get => colorCode;
        set => colorCode = value;
    }

    private int priority;

    /// <summary>
    /// Can be used to sort the Flags by their importance.
    /// </summary>
    public int Priority
    {
        get => priority;
        set => priority = value;
    }

    private FlagShape m_flagShape = FlagShape.Diamond;

    /// <summary>
    /// determines the fill shape of the flag segment
    /// </summary>
    public FlagShape FlagShape
    {
        get => m_flagShape;
        set => m_flagShape = value;
    }
}

public enum FlagShape { Rectangle, Diamond }