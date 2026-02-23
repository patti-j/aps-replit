using System.Collections;

namespace PT.Transmissions;

/// <summary>
/// Used to Create PtAttributes in base objects.
/// </summary>
public class OperationAttribute : IPTSerializable
{
    #region IPTSerializable Members
    public const int UNIQUE_ID = 548;

    public OperationAttribute(IReader a_reader)
    {
        if (a_reader.VersionNumber >= 12413)
        {
            m_bools = new BoolVector32(a_reader);
            a_reader.Read(out m_ptAttributeExternalId);
            a_reader.Read(out m_code);
            a_reader.Read(out m_number);
            a_reader.Read(out m_cost);
            a_reader.Read(out m_duration);
            a_reader.Read(out m_colorCode);
        }
        else if (a_reader.VersionNumber >= 12400)
        {
            // For the versions of Neptune (12.2.1.x) that still had the faulty serialization
            m_bools = new BoolVector32(a_reader);
            a_reader.Read(out m_ptAttributeExternalId);
            a_reader.Read(out string externalId);
            a_reader.Read(out m_code);
            a_reader.Read(out m_number);
            a_reader.Read(out m_cost);
            a_reader.Read(out m_duration);
            a_reader.Read(out m_colorCode);
        }
        else if (a_reader.VersionNumber >= 12329)
        {

            // The serialization changes were merged back into the 12.2.0 branch
            // and this block corresponds to the version number in that branch
            m_bools = new BoolVector32(a_reader);
            a_reader.Read(out m_ptAttributeExternalId);
            a_reader.Read(out m_code);
            a_reader.Read(out m_number);
            a_reader.Read(out m_cost);
            a_reader.Read(out m_duration);
            a_reader.Read(out m_colorCode);
        }
        else if (a_reader.VersionNumber >= 12321)
        {
            m_bools = new BoolVector32(a_reader);
            a_reader.Read(out m_ptAttributeExternalId);
            a_reader.Read(out string externalId);
            a_reader.Read(out m_code);
            a_reader.Read(out m_number);
            a_reader.Read(out m_cost);
            a_reader.Read(out m_duration);
            a_reader.Read(out m_colorCode);
        }
        else if (a_reader.VersionNumber >= 12303)
        {
            m_bools = new BoolVector32(a_reader);
            a_reader.Read(out m_ptAttributeExternalId);
            a_reader.Read(out string externalId);
            a_reader.Read(out m_code);
            a_reader.Read(out m_number);
            a_reader.Read(out m_cost);
            a_reader.Read(out m_duration);
        }
        else if (a_reader.VersionNumber >= 473)
        {
            a_reader.Read(out string name);
            a_reader.Read(out string description);
            a_reader.Read(out m_code);
            a_reader.Read(out m_number);
            a_reader.Read(out int colorCodeAlpha);
            a_reader.Read(out int colorCodeRed);
            a_reader.Read(out int colorCodeGreen);
            a_reader.Read(out int colorCodeBlue);
            a_reader.Read(out int consecutiveSetup);
            a_reader.Read(out m_cost);
            a_reader.Read(out m_duration);
            a_reader.Read(out int incurSetup);
            m_bools = new BoolVector32(a_reader);
        }
    }

    public virtual void Serialize(IWriter a_writer)
    {
        #if DEBUG
        a_writer.DuplicateErrorCheck(this);
        #endif

        m_bools.Serialize(a_writer);
        a_writer.Write(m_ptAttributeExternalId);
        a_writer.Write(m_code);
        a_writer.Write(m_number);
        a_writer.Write(m_cost);
        a_writer.Write(m_duration);
        a_writer.Write(m_colorCode);
    }

    public virtual int UniqueId => UNIQUE_ID;
    #endregion

    public OperationAttribute(string a_ptAttributeExternalId)
    {
        m_ptAttributeExternalId = a_ptAttributeExternalId;
    }

    #region Shared Properties
    private string m_ptAttributeExternalId;

    /// <summary>
    /// The External Id of the PTAttribute to map
    /// </summary>
    public string PTAttributeExternalId
    {
        get => m_ptAttributeExternalId;
        set => m_ptAttributeExternalId = value;
    }

    private string m_code;

    /// <summary>
    /// The data that defines the Attribute.  Can be used by Resource Attribute setup tables.
    /// </summary>
    public string Code
    {
        get => m_code;
        set => m_code = value;
    }

    public bool CodeManualUpdateOnly
    {
        get => m_bools[c_codeManualUpdateOnlyIdx];
        set => m_bools[c_codeManualUpdateOnlyIdx] = value;
    }

    private decimal m_number;

    /// <summary>
    /// A numeric value that defines the Attribute.  Can be used by Resource Attribute setup tables.
    /// </summary>
    public decimal Number
    {
        get => m_number;
        set => m_number = value;
    }

    public bool NumberManualUpdateOnly
    {
        get => m_bools[c_numberManualUpdateOnlyIdx];
        set => m_bools[c_numberManualUpdateOnlyIdx] = value;
    }

    private decimal m_cost;

    /// <summary>
    /// The time to perform the setup related to this Attribute.
    /// This is only used if not using Lookup tables.
    /// </summary>
    public decimal Cost
    {
        get => m_cost;
        set => m_cost = value;
    }

    public bool CostManualUpdateOnly
    {
        get => m_bools[c_costManualUpdateOnlyIdx];
        set => m_bools[c_costManualUpdateOnlyIdx] = value;
    }

    private TimeSpan m_duration;

    /// <summary>
    /// The time to perform the setup related to this Attribute.
    /// This is only used if not using Lookup tables.
    /// </summary>
    public TimeSpan Duration
    {
        get => m_duration;
        set => m_duration = value;
    }

    public bool DurationManualUpdateOnly
    {
        get => m_bools[c_durationManualUpdateOnlyIdx];
        set => m_bools[c_durationManualUpdateOnlyIdx] = value;
    }

    private System.Drawing.Color m_colorCode;

    /// <summary>
    /// The time to perform the setup related to this Attribute.
    /// This is only used if not using Lookup tables.
    /// </summary>
    public System.Drawing.Color ColorCode
    {
        get => m_colorCode;
        set => m_colorCode = value;
    }

    public bool ShowInGantt
    {
        get => m_bools[c_showInGanttIdx];
        set => m_bools[c_showInGanttIdx] = value;
    }

    public bool ShowInGanttManualUpdateOnly
    {
        get => m_bools[c_showInGanttManualUpdateOnlyIdx];
        set => m_bools[c_showInGanttManualUpdateOnlyIdx] = value;
    }

    public bool ColorCodeManualUpdateOnly
    {
        get => m_bools[c_colorCodeManualUpdateOnlyIdx];
        set => m_bools[c_colorCodeManualUpdateOnlyIdx] = value;
    }

    public bool DurationOverride
    {
        get => m_bools[c_durationOverrideIdx];
        set => m_bools[c_durationOverrideIdx] = value;
    }

    public bool CostOverride
    {
        get => m_bools[c_costOverrideIdx];
        set => m_bools[c_costOverrideIdx] = value;
    }

    public bool ColorOverride
    {
        get => m_bools[c_colorOverrideIdx];
        set => m_bools[c_colorOverrideIdx] = value;
    }

    public bool ShowInGanttOverride
    {
        get => m_bools[c_showInGanttOverrideIdx];
        set => m_bools[c_showInGanttOverrideIdx] = value;
    }

    private BoolVector32 m_bools;
    private const short c_codeManualUpdateOnlyIdx = 0;
    private const short c_numberManualUpdateOnlyIdx = 1;
    private const short c_durationManualUpdateOnlyIdx = 2;
    private const short c_costManualUpdateOnlyIdx = 3;
    private const short c_colorCodeManualUpdateOnlyIdx = 4;
    private const short c_durationOverrideIdx = 5;
    private const short c_costOverrideIdx = 6;
    private const short c_colorOverrideIdx = 7;
    private const short c_showInGanttIdx = 8;
    private const short c_showInGanttOverrideIdx = 9;
    private const short c_showInGanttManualUpdateOnlyIdx = 10;
    #endregion

    public void Validate() { }
}

public class OperationAttributeList : IPTSerializable, IEnumerable<OperationAttribute>
{
    #region IPTSerializable Members
    public OperationAttributeList(IReader a_reader)
    {
        if (a_reader.VersionNumber >= 1)
        {
            a_reader.Read(out int count);
            for (int i = 0; i < count; i++)
            {
                m_operationAttributes.Add(new OperationAttribute(a_reader));
            }
        }
    }

    public void Serialize(IWriter a_writer)
    {
        a_writer.Write(m_operationAttributes.Count);
        for (int i = 0; i < m_operationAttributes.Count; i++)
        {
            m_operationAttributes[i].Serialize(a_writer);
        }
    }

    public const int UNIQUE_ID = 549;

    // TODO:  Add PtAttributeList.UniqueId getter implementation
    public int UniqueId => UNIQUE_ID;
    #endregion

    private readonly List<OperationAttribute> m_operationAttributes = new ();

    public OperationAttributeList() { }

    public void Add(OperationAttribute a_operationAttribute)
    {
        m_operationAttributes.Add(a_operationAttribute);
    }

    public int Count => m_operationAttributes.Count;

    public OperationAttribute this[int a_index] => m_operationAttributes[a_index];

    public void Clear()
    {
        m_operationAttributes.Clear();
    }

    /// <summary>
    /// Returns the PTAttribute that has the specified name or null if it doesn't exist.
    /// </summary>
    /// <param name="a_externalId"></param>
    /// <returns></returns>
    public OperationAttribute FindByExternalId(string a_externalId)
    {
        for (int i = 0; i < Count; ++i)
        {
            OperationAttribute opAttribute = m_operationAttributes[i];

            if (opAttribute.PTAttributeExternalId == a_externalId)
            {
                return opAttribute;
            }
        }

        return null;
    }

    public IEnumerator<OperationAttribute> GetEnumerator()
    {
        foreach (OperationAttribute opAttribute in m_operationAttributes)
        {
            yield return opAttribute;
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}