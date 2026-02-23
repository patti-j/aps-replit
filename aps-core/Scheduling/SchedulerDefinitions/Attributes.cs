namespace PT.SchedulerDefinitions;

/// <summary>
/// Specifies how the property should be displayed in the user interface.  If hidden, then it is also omitted from reports.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class DisplayAttribute : Attribute, IPTSerializable
{
    public const int UNIQUE_ID = 171;

    #region IPTSerializable Members
    public DisplayAttribute(IReader reader)
    {
        if (reader.VersionNumber >= 1)
        {
            int val;
            reader.Read(out val);
            option = (displayOptions)val;
        }
    }

    public void Serialize(IWriter writer)
    {
        writer.Write((int)option);
    }

    public virtual int UniqueId => UNIQUE_ID;
    #endregion

    public enum displayOptions { Hide, ReadOnly, AllowEdit } //For now just using ReadOnly.  May get rid of Browsable attribute once the Properties control can omit properties based on DisplayAttribute.

    public DisplayAttribute(displayOptions option)
    {
        this.option = option;
    }

    /// <summary>
    /// If part of key then this field will be added to DataTables but may be hidden.
    /// </summary>
    protected displayOptions option;

    public displayOptions Option => option;
}