using System.ComponentModel;
using System.Drawing;

using PT.APSCommon;
using PT.Database;
using PT.ERPTransmissions;
using PT.SchedulerDefinitions;
using PT.Transmissions;
using static PT.ERPTransmissions.ResourceT;

namespace PT.Scheduler;

/// <summary>
/// Custom defined value that can be attached to objects  to create user defined values for optimization rules, Focus Points, etc.
/// </summary>
public class PTAttribute : BaseObject, IPTSerializable
{
    public const int UNIQUE_ID = 982;

    #region IPTSerializable Members
    public PTAttribute(IReader a_reader)
        : base(a_reader)
    {
        if (a_reader.VersionNumber >= 12303)
        {
            m_bools = new BoolVector32(a_reader);
            a_reader.Read(out m_colorCode);
            a_reader.Read(out m_consecutiveSetup);
            a_reader.Read(out m_defaultCost);
            a_reader.Read(out m_defaultDuration);
            a_reader.Read(out int incurWhenVal);
            m_attributeTrigger = (PTAttributeDefs.EAttributeTriggerOptions)incurWhenVal;
            a_reader.Read(out int changeoverType);
            m_attributeType = (PTAttributeDefs.EIncurAttributeType)changeoverType;
            a_reader.Read(out m_cleanoutGrade);
        }
    }
    #endregion

    public override void Serialize(IWriter a_writer)
    {
        base.Serialize(a_writer);
        m_bools.Serialize(a_writer);
        a_writer.Write(m_colorCode);
        a_writer.Write(m_consecutiveSetup);
        a_writer.Write(m_defaultCost);
        a_writer.Write(m_defaultDuration);
        a_writer.Write((int)m_attributeTrigger);
        a_writer.Write((int)m_attributeType);
        a_writer.Write(m_cleanoutGrade);
    }

    [Browsable(false)]
    public int UniqueId => UNIQUE_ID;

    #region Construction
    public PTAttribute(BaseId a_newId)
        : base(a_newId)
    {
        UseInSequencing = true;
    }

    public PTAttribute(PTAttributeT.PTAttribute a_ptAttributeT, BaseId a_newId)
        : base(a_newId, a_ptAttributeT)
    {
        m_colorCode = a_ptAttributeT.ColorCode;
        m_consecutiveSetup = a_ptAttributeT.ConsecutiveSetup;
        m_defaultCost = a_ptAttributeT.Cost;
        m_defaultDuration = a_ptAttributeT.Duration;
        m_attributeTrigger = a_ptAttributeT.AttributeTrigger;
        ShowInGantt = a_ptAttributeT.ShowInGantt;
        HideInGrids = a_ptAttributeT.HideInGrids;
        CleanoutGrade = a_ptAttributeT.CleanoutGrade;
        UseInSequencing = !a_ptAttributeT.UseInSequencingIsSet || a_ptAttributeT.UseInSequencing;
        m_attributeType = a_ptAttributeT.AttributeType;
        IncurResourceSetup = a_ptAttributeT.IncurResourceSetup;
        // UseInSequencing is supposed to default to true
        Name = a_ptAttributeT.Name;
        Description = a_ptAttributeT.Description;
        // Intellisense is telling me that there's a virtual call in the constructor for Name and Description
        // but I can't seem to find it. They also aren't being set when importing new PTAttributes so 
        // these two assignment statements are needed. 
    }

    public PTAttribute(PTAttribute a_sourceAttribute, BaseId a_newId) : base(a_newId)
    {
        m_colorCode = a_sourceAttribute.ColorCode;
        m_consecutiveSetup = a_sourceAttribute.ConsecutiveSetup;
        m_defaultCost = a_sourceAttribute.DefaultCost;
        m_defaultDuration = a_sourceAttribute.DefaultDuration;
        m_attributeTrigger = a_sourceAttribute.AttributeTrigger;
        ShowInGantt = a_sourceAttribute.ShowInGantt;
        HideInGrids = a_sourceAttribute.HideInGrids;
        CleanoutGrade = a_sourceAttribute.CleanoutGrade;
        UseInSequencing = a_sourceAttribute.UseInSequencing;
        m_attributeType = a_sourceAttribute.AttributeType;
        IncurResourceSetup = a_sourceAttribute.IncurResourceSetup;
    }

    /// <summary>
    /// Returns true if any changes were made.
    /// </summary>
    /// <param name="a_source"></param>
    /// <returns></returns>
    public bool Update(PTAttributeT.PTAttribute a_source)
    {
        bool changed = false;

        if (a_source.NameIsSet && a_source.Name != Name)
        {
            Name = a_source.Name;
            changed = true;
        }

        if (a_source.ColorCodeIsSet && m_colorCode != a_source.ColorCode)
        {
            m_colorCode = a_source.ColorCode;
            changed = true;
        }

        if (a_source.ConsecutiveSetupIsSet && m_consecutiveSetup != a_source.ConsecutiveSetup)
        {
            m_consecutiveSetup = a_source.ConsecutiveSetup;
            changed = true;
        }

        if (a_source.IncurWhenIsSet && m_attributeTrigger != a_source.AttributeTrigger)
        {
            m_attributeTrigger = a_source.AttributeTrigger;
            changed = true;
        }

        if (a_source.AttributeTypeIsSet && m_attributeType != a_source.AttributeType)
        {
            m_attributeType = a_source.AttributeType;
            changed = true;
        }

        if (a_source.CostIsSet && m_defaultCost != a_source.Cost)
        {
            m_defaultCost = a_source.Cost;
            changed = true;
        }

        if (a_source.DurationIsSet && m_defaultDuration != a_source.Duration)
        {
            m_defaultDuration = a_source.Duration;
            changed = true;
        }

        if (a_source.ShowInGanttIsSet && ShowInGantt != a_source.ShowInGantt)
        {
            ShowInGantt = a_source.ShowInGantt;
            changed = true;
        }

        if (a_source.HideInGridsIsSet && HideInGrids != a_source.HideInGrids)
        {
            HideInGrids = a_source.HideInGrids;
            changed = true;
        }

        if (a_source.CleanoutGradeIsSet && CleanoutGrade != a_source.CleanoutGrade)
        {
            CleanoutGrade = a_source.CleanoutGrade;
            changed = true;
        }

        if (a_source.ManualUpdateOnlyIsSet && ManualUpdateOnly != a_source.ManualUpdateOnly)
        {
            ManualUpdateOnly = a_source.ManualUpdateOnly;
            changed = true;
        }

        if (a_source.AttributeTypeIsSet && AttributeType != a_source.AttributeType)
        {
            AttributeType = a_source.AttributeType;
            changed = true;
        }

        if (a_source.UseInSequencingIsSet && UseInSequencing != a_source.UseInSequencing)
        {
            UseInSequencing = a_source.UseInSequencing;
            changed = true;
        }

        if (a_source.IncurResourceSetupIsSet && IncurResourceSetup != a_source.IncurResourceSetup)
        {
            IncurResourceSetup = a_source.IncurResourceSetup;
            changed = true;
        }

        return changed;
    }

    public bool Edit(PTAttributeEdit a_edit)
    {
        bool changed = false;

        if (a_edit.ColorCodeIsSet && m_colorCode != a_edit.ColorCode)
        {
            m_colorCode = a_edit.ColorCode;
            changed = true;
        }

        if (a_edit.ConsecutiveIsSet && m_consecutiveSetup != a_edit.Consecutive)
        {
            m_consecutiveSetup = a_edit.Consecutive;
            changed = true;
        }

        if (a_edit.IncurWhenIsSet && m_attributeTrigger != a_edit.AttributeTrigger)
        {
            m_attributeTrigger = a_edit.AttributeTrigger;
            changed = true;
        }

        if (a_edit.CostIsSet && m_defaultCost != a_edit.Cost)
        {
            m_defaultCost = a_edit.Cost;
            changed = true;
        }

        if (a_edit.DurationIsSet && m_defaultDuration != a_edit.Duration)
        {
            m_defaultDuration = a_edit.Duration;
            changed = true;
        }

        if (a_edit.ShowInGanttIsSet && ShowInGantt != a_edit.ShowInGantt)
        {
            ShowInGantt = a_edit.ShowInGantt;
            changed = true;
        }

        if (a_edit.HideInGridsIsSet && HideInGrids != a_edit.HideInGrids)
        {
            HideInGrids = a_edit.HideInGrids;
            changed = true;
        }

        if (a_edit.AttributeTypeIsSet && AttributeType != a_edit.AttributeType)
        {
            AttributeType = a_edit.AttributeType;
            changed = true;
        }

        if (a_edit.ManualUpdateOnlyIsSet && ManualUpdateOnly != a_edit.ManualUpdateOnly)
        {
            ManualUpdateOnly = a_edit.ManualUpdateOnly;
            changed = true;
        }

        if (a_edit.CleanoutGradeIsSet && CleanoutGrade != a_edit.CleanoutGrade)
        {
            CleanoutGrade = a_edit.CleanoutGrade;
            changed = true;
        }

        if (a_edit.UseInSequencingIsSet && UseInSequencing != a_edit.UseInSequencing)
        {
            UseInSequencing = a_edit.UseInSequencing;
            changed = true;
        }

        if (a_edit.IncurResourceSetupIsSet && IncurResourceSetup != a_edit.IncurResourceSetup)
        {
            IncurResourceSetup = a_edit.IncurResourceSetup;
            changed = true;
        }

        return changed;
    }

    #endregion

    #region Shared Properties
    private PTAttributeDefs.EAttributeTriggerOptions m_attributeTrigger;

    /// <summary>
    /// Specifies when to include a setup time and cost for the Attribute.
    /// </summary>
    public PTAttributeDefs.EAttributeTriggerOptions AttributeTrigger
    {
        get => m_attributeTrigger;
        set => m_attributeTrigger = value;
    }

    private Color m_colorCode = Color.Empty;

    /// <summary>
    /// A Color that can be used to represent the value of the Attribute in the Gantt.  For example, if the Attribute represents paint color then the ColorCode can be the color of the paint.
    /// </summary>
    public Color ColorCode
    {
        get => m_colorCode;
        set => m_colorCode = value;
    }

    //TODO: Move this bool into the bool vector when we need to make serialization changes
    // Make sure corresponding changes are made to PTAttributeT.Attribute!
    private bool m_consecutiveSetup;

    /// <summary>
    /// The total setup time for an Operation is calculated as the sum of the maximum non-Consecutive setup time
    /// and the maximum of the various Consecutive setup times.
    /// </summary>
    public bool ConsecutiveSetup
    {
        get => m_consecutiveSetup;
        set => m_consecutiveSetup = value;
    }

    private decimal m_defaultCost;

    /// <summary>
    /// The cost to perform the setup related to this Attribute.
    /// This is only used if not using Lookup tables.
    /// </summary>
    public decimal DefaultCost
    {
        get => m_defaultCost;
        set => m_defaultCost = value;
    }

    private TimeSpan m_defaultDuration;

    /// <summary>
    /// The time to perform the setup related to this Attribute.
    /// This is only used if not using Lookup tables.
    /// </summary>
    public TimeSpan DefaultDuration
    {
        get => m_defaultDuration;
        set => m_defaultDuration = value;
    }

    private BoolVector32 m_bools;
    private const short c_showInGanttIdx = 0;
    private const short c_useInSequencing = 1;
    private const short c_showInGridsIdx = 3;
    private const short c_manualUpdateOnlyIdx = 4;
    private const short c_incurResourceSetupIdx = 5;

    /// <summary>
    /// Set to true to show the Attribute in the Attributes Segment in the Gantt.
    /// </summary>
    public bool ShowInGantt
    {
        get => m_bools[c_showInGanttIdx];
        set => m_bools[c_showInGanttIdx] = value;
    }

    public bool UseInSequencing
    {
        get => m_bools[c_useInSequencing];
        set => m_bools[c_useInSequencing] = value;
    }

    public bool ManualUpdateOnly
    {
        get => m_bools[c_manualUpdateOnlyIdx];
        set => m_bools[c_manualUpdateOnlyIdx] = value;
    }

    /// <summary>
    /// Set to true to show the Attribute in the Attributes Segment in the Gantt.
    /// </summary>
    public bool HideInGrids
    {
        get => m_bools[c_showInGridsIdx];
        set => m_bools[c_showInGridsIdx] = value;
    }

    private int m_cleanoutGrade;

    /// <summary>
    /// Whether to incur this attribute changeover as Clean or Setup
    /// </summary>
    public int CleanoutGrade
    {
        get => m_cleanoutGrade;
        set => m_cleanoutGrade = value;
    }

    private PTAttributeDefs.EIncurAttributeType m_attributeType;

    /// <summary>
    /// Whether to incur this attribute changeover as Clean or Setup
    /// </summary>
    public PTAttributeDefs.EIncurAttributeType AttributeType
    {
        get => m_attributeType;
        set => m_attributeType = value;
    }

    /// <summary>
    /// Set to true to incur the standard cost specified on the Resource.
    /// </summary>
    public bool IncurResourceSetup
    {
        get => m_bools[c_incurResourceSetupIdx];
        set => m_bools[c_incurResourceSetupIdx] = value;
    }
    #endregion

    /// <summary>
    /// Returns a string for reports, etc.
    /// Examples: Name: Desc(if used) Code(if not null) Number(if not -1)
    /// Color (part color): Yellow
    /// Width: 5.2
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        string descStr = "";
        if (Description != null && Description.Trim() != Name.Trim()) //no sense showing the description if it's the same as the Name.
        {
            descStr = string.Format("({0}) ", Description.Trim());
        }

        return string.Format("{0} {1}", Name.Trim(), descStr);
    }

    #region Overrides
    /// <summary>
    /// Used as a prefix for generating default names
    /// </summary>
    [Browsable(false)]
    public override string DefaultNamePrefix => "PTAttribute";
    #endregion

    public void PtDbPopulate(ref PtDbDataSet a_ds, PTDatabaseHelper a_dbHelper, PtDbDataSet.SchedulesRow a_schedulesRow)
    {
        a_ds.PTAttributes.AddPTAttributesRow(
            a_schedulesRow,
            a_schedulesRow.InstanceId,
            Name,
            Description,
            ExternalId,
            ColorUtils.ConvertColorToHexString(ColorCode),
            ConsecutiveSetup,
            IncurResourceSetup,
            DefaultCost,
            DefaultDuration.TotalHours,
            AttributeTrigger.ToString(),
            AttributeType.ToString(),
            CleanoutGrade,
            ShowInGantt,
            HideInGrids);
    }
}