using System.Collections;
using System.Drawing;

using PT.APSCommon;
using PT.APSCommon.Extensions;
using PT.SchedulerDefinitions;
using PT.Transmissions;

namespace PT.ERPTransmissions;

public class PTAttributeEditT : ScenarioIdBaseT, IPTSerializable, IEnumerable<PTAttributeEdit>
{
    #region PT Serialization
    private readonly List<PTAttributeEdit> m_ptAttributeEdits = new ();
    public static int UNIQUE_ID => 993;

    public PTAttributeEditT() { }

    public PTAttributeEditT(IReader a_reader)
        : base(a_reader)
    {
        if (a_reader.VersionNumber >= 12000)
        {
            a_reader.Read(out int count);
            for (int i = 0; i < count; i++)
            {
                PTAttributeEdit node = new (a_reader);
                m_ptAttributeEdits.Add(node);
            }
        }
    }

    public override void Serialize(IWriter writer)
    {
        base.Serialize(writer);

        writer.Write(m_ptAttributeEdits);
    }

    public override int UniqueId => UNIQUE_ID;

    public PTAttributeEditT(BaseId a_scenarioId) : base(a_scenarioId) { }
    public PTAttributeEdit this[int i] => m_ptAttributeEdits[i];

    public void Validate()
    {
        foreach (PTAttributeEdit attributeEdit in m_ptAttributeEdits)
        {
            attributeEdit.Validate();
        }
    }

    public override string Description => string.Format("Customers updated ({0})".Localize(), m_ptAttributeEdits.Count);

    public IEnumerator<PTAttributeEdit> GetEnumerator()
    {
        return m_ptAttributeEdits.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public void Add(PTAttributeEdit a_pts)
    {
        m_ptAttributeEdits.Add(a_pts);
    }
    #endregion
}

public class PTAttributeEdit : PTObjectBaseEdit, IPTSerializable
{
    public const int UNIQUE_ID = 793;

    public PTAttributeEdit(IReader a_reader)
        : base(a_reader)
    {
        if (a_reader.VersionNumber >= 12303)
        {
            m_bools = new BoolVector32(a_reader);
            m_setBools = new BoolVector32(a_reader);
            a_reader.Read(out m_colorCode);
            a_reader.Read(out m_cost);
            a_reader.Read(out m_duration);
            a_reader.Read(out int incurWhenVal);
            m_attributeTrigger = (PTAttributeDefs.EAttributeTriggerOptions)incurWhenVal;
            a_reader.Read(out int attributeTypeVal);
            m_attributeType = (PTAttributeDefs.EIncurAttributeType)attributeTypeVal;
            a_reader.Read(out m_cleanoutGrade);
        }
    }

    public PTAttributeEdit(BaseId a_attributeId)
    {
        Id = a_attributeId;
        m_externalId = null; //Clear other id without triggering IsSet
    }

    public new void Serialize(IWriter a_writer)
    {
        base.Serialize(a_writer);
        m_bools.Serialize(a_writer);
        m_setBools.Serialize(a_writer);
        a_writer.Write(m_colorCode);
        a_writer.Write(m_cost);
        a_writer.Write(m_duration);
        a_writer.Write((int)m_attributeTrigger);
        a_writer.Write((int)m_attributeType);
        a_writer.Write(m_cleanoutGrade);
    }

    public int UniqueId => UNIQUE_ID;

    private BoolVector32 m_bools;
    private const short c_showInGanttIdx = 0;
    private const short c_manualUpdateIdx = 1;
    private const short c_consecutiveIdx = 2;
    private const short c_hideInGridsIdx = 3;
    private const short c_useInSequencingIdx = 4;
    private const short c_incurResourceSetupIdx = 5;

    private BoolVector32 m_setBools;
    private const short c_nameIsSetIdx = 0;
    private const short c_descriptionIsSetIdx = 1;
    private const short c_colorCodeIsSetIdx = 2;
    private const short c_costIsSetIdx = 3;
    private const short c_durationIsSetIdx = 4;
    private const short c_incurWhenIsSetIdx = 5;
    private const short c_attributeTypeIsSetIdx = 6;
    private const short c_cleanoutGradeIsSetIdx = 7;
    private const short c_showInGanttIsSetIdx = 8;
    private const short c_hideInGridsIsSetIdx = 9;
    private const short c_manualUpdateOnlyIsSetIdx = 10;
    private const short c_consecutiveIsSetIdx = 11;
    private const short c_useInSequencingIsSetIdx = 12;
    private const short c_incurResourceSetupIsSetIdx = 13;

    #region Shared Properties
    private PTAttributeDefs.EAttributeTriggerOptions m_attributeTrigger;

    /// <summary>
    /// Specifies when to include a setup time and cost for the Attribute.
    /// </summary>
    public PTAttributeDefs.EAttributeTriggerOptions AttributeTrigger
    {
        get => m_attributeTrigger;
        set
        {
            m_attributeTrigger = value;
            m_setBools[c_incurWhenIsSetIdx] = true;
        }
    }

    public bool IncurWhenIsSet => m_setBools[c_incurWhenIsSetIdx];

    private Color m_colorCode = Color.Empty;

    /// <summary>
    /// A Color that can be used to represent the value of the Attribute in the Gantt.  For example, if the Attribute represents paint color then the ColorCode can be the color of the paint.
    /// </summary>
    public Color ColorCode
    {
        get => m_colorCode;
        set
        {
            m_colorCode = value;
            m_setBools[c_colorCodeIsSetIdx] = true;
        }
    }

    public bool ColorCodeIsSet => m_setBools[c_colorCodeIsSetIdx];

    /// <summary>
    /// The total setup time for an Operation is calculated as the sum of the maximum non-Consecutive setup time
    /// and the maximum of the various Consecutive setup times.
    /// </summary>
    public bool Consecutive
    {
        get => m_bools[c_consecutiveIdx];
        set
        {
            m_bools[c_consecutiveIdx] = value;
            m_setBools[c_consecutiveIsSetIdx] = true;
        }
    }

    public bool ConsecutiveIsSet => m_setBools[c_consecutiveIsSetIdx];

    private decimal m_cost;

    /// <summary>
    /// The cost to perform the setup related to this Attribute.
    /// This is only used if not using Lookup tables.
    /// </summary>
    public decimal Cost
    {
        get => m_cost;
        set
        {
            m_cost = value;
            m_setBools[c_costIsSetIdx] = true;
        }
    }

    public bool CostIsSet => m_setBools[c_costIsSetIdx];

    private TimeSpan m_duration;

    /// <summary>
    /// The time to perform the setup related to this Attribute.
    /// This is only used if not using Lookup tables.
    /// </summary>
    public TimeSpan Duration
    {
        get => m_duration;
        set
        {
            m_duration = value;
            m_setBools[c_durationIsSetIdx] = true;
        }
    }

    public bool DurationIsSet => m_setBools[c_durationIsSetIdx];

    /// <summary>
    /// Set to true to show the Attribute in the Attributes Segment in the Gantt.
    /// </summary>
    public bool ShowInGantt
    {
        get => m_bools[c_showInGanttIdx];
        set
        {
            m_bools[c_showInGanttIdx] = value;
            m_setBools[c_showInGanttIsSetIdx] = true;
        }
    }

    public bool ShowInGanttIsSet => m_setBools[c_showInGanttIsSetIdx];

    /// <summary>
    /// Set to true to prevent imports from deleting or updating this Attribute.
    /// </summary>
    public bool ManualUpdateOnly
    {
        get => m_bools[c_manualUpdateIdx];
        set
        {
            m_bools[c_manualUpdateIdx] = value;
            m_setBools[c_manualUpdateOnlyIsSetIdx] = true;
        }
    }

    public bool ManualUpdateOnlyIsSet => m_setBools[c_manualUpdateOnlyIsSetIdx];

    /// <summary>
    /// Set to true to hide the Attribute in the Grids.
    /// </summary>
    public bool HideInGrids
    {
        get => m_bools[c_hideInGridsIdx];
        set
        {
            m_bools[c_hideInGridsIdx] = value;
            m_setBools[c_hideInGridsIsSetIdx] = true;
        }
    }

    public bool HideInGridsIsSet => m_setBools[c_hideInGridsIsSetIdx];

    private int m_cleanoutGrade;

    /// <summary>
    /// Whether to incur this attribute changeover as Clean or Setup
    /// </summary>
    public int CleanoutGrade
    {
        get => m_cleanoutGrade;
        set
        {
            m_cleanoutGrade = value;
            m_setBools[c_cleanoutGradeIsSetIdx] = true;
        }
    }

    public bool CleanoutGradeIsSet => m_setBools[c_cleanoutGradeIsSetIdx];

    public bool UseInSequencing
    {
        get => m_bools[c_useInSequencingIdx];
        set
        {
            m_bools[c_useInSequencingIdx] = value;
            m_setBools[c_useInSequencingIsSetIdx] = true;
        }
    }

    public bool UseInSequencingIsSet => m_setBools[c_useInSequencingIsSetIdx];

    private PTAttributeDefs.EIncurAttributeType m_attributeType;

    /// <summary>
    /// Whether to incur this attribute changeover as Clean or Setup
    /// </summary>
    public PTAttributeDefs.EIncurAttributeType AttributeType
    {
        get => m_attributeType;
        set
        {
            m_attributeType = value;
            m_setBools[c_attributeTypeIsSetIdx] = true;
        }
    }

    public bool AttributeTypeIsSet => m_setBools[c_attributeTypeIsSetIdx];

    public bool IncurResourceSetup
    {
        get => m_bools[c_incurResourceSetupIdx];
        set
        {
            m_bools[c_incurResourceSetupIdx] = value;
            m_setBools[c_incurResourceSetupIsSetIdx] = true;
        }
    }

    public bool IncurResourceSetupIsSet => m_setBools[c_incurResourceSetupIsSetIdx];
    #endregion

    public void Validate() { }
}