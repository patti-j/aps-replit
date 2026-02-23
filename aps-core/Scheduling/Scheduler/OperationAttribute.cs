using PT.APSCommon;
using PT.Common.Exceptions;
using PT.Common.Text;
using System.Collections;
using System.ComponentModel;
using System.Drawing;

using PT.Common.Debugging;

namespace PT.Scheduler;

/// <summary>
/// OperationAttribute and PTAttribute use to be the same class (called PTAttribute), and its
/// purpose was to provide a way for users to define custom values on objects that could be used
/// for optimization rules (often called Sequencing Factors/Plans), Focus Points, and others.
/// </summary>
/// They've been split into two classes, and PTAttribute is now (April 2024) a template and
/// handled by an implementation of ExternalBaseIdObjectManager that exists on the
/// ScenarioDetail level. PTAttribute use to just exist as in collection on a BaseObject,
/// and now OperationAttribute is its replacement. Each OperationAttribute will hold some
/// information specific to the operation (attributes are currently only used by operations despite
/// existing on the BaseObject) and a reference to its source PTAttribute. Values on the OperationAttribute
/// will provide a way for the user to override the default values from the source PTAttribute at the
/// operation level.
/// OperationAttributes within an Operation must reference unique PTAttributes, but
/// OperationAttributes across multiple Operations can share PTAttribute references
public class OperationAttribute : IPTSerializable
{
    public const int UNIQUE_ID = 16;

    #region IPTSerializable Members
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
            m_bools = new BoolVector32(a_reader);
            a_reader.Read(out m_oldExternalId);
            m_ptAttributeId = new BaseId(a_reader);
            a_reader.Read(out m_code);
            a_reader.Read(out m_number);
            a_reader.Read(out m_cost);
            a_reader.Read(out m_duration);
            a_reader.Read(out m_colorCode);
        }
        else if (a_reader.VersionNumber >= 12329)
        {
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
            a_reader.Read(out m_oldExternalId);
            m_ptAttributeId = new BaseId(a_reader);
            a_reader.Read(out m_code);
            a_reader.Read(out m_number);
            a_reader.Read(out m_cost);
            a_reader.Read(out m_duration);
            a_reader.Read(out m_colorCode);
        }
        else if (a_reader.VersionNumber >= 12312)
        {
            m_bools = new BoolVector32(a_reader);
            a_reader.Read(out m_oldExternalId);
            m_ptAttributeId = new BaseId(a_reader);
            a_reader.Read(out m_code);
            a_reader.Read(out m_number);
            a_reader.Read(out m_cost);
            a_reader.Read(out m_duration);
        }
        else if (a_reader.VersionNumber >= 12311)
        {
            m_bools = new BoolVector32(a_reader);
            a_reader.Read(out m_oldExternalId);
            a_reader.Read(out m_ptAttributeExternalId);
            a_reader.Read(out m_code);
            a_reader.Read(out m_number);
            a_reader.Read(out m_cost);
            a_reader.Read(out m_duration);
        }
        else if (a_reader.VersionNumber >= 12303)
        {
            m_bools = new BoolVector32(a_reader);
            a_reader.Read(out m_oldExternalId);
            m_ptAttributeId = new BaseId(a_reader);
            a_reader.Read(out m_code);
            a_reader.Read(out m_number);
            a_reader.Read(out m_cost);
            a_reader.Read(out m_duration);
        }
        else if (a_reader.VersionNumber >= 470)
        {
            a_reader.Read(out string name);
            m_oldExternalId = name; //For backwards compatibility
            a_reader.Read(out string description);
            a_reader.Read(out m_code);
            a_reader.Read(out m_number);
            a_reader.Read(out Color colorCode);
            a_reader.Read(out bool consecutiveSetup);
            a_reader.Read(out m_cost);
            a_reader.Read(out m_duration);
            a_reader.Read(out int incurSetup);
            m_bools = new BoolVector32(a_reader);
        }
    }

    public void Serialize(IWriter a_writer)
    {
        m_bools.Serialize(a_writer);

        a_writer.Write(PTAttributeExternalId);
        a_writer.Write(m_code);
        a_writer.Write(m_number);
        a_writer.Write(m_cost);
        a_writer.Write(m_duration);
        a_writer.Write(m_colorCode);
    }

    [Browsable(false)]
    public int UniqueId => UNIQUE_ID;
    #endregion

    #region Construction
    public PTAttribute PTAttribute
    {
        get => m_ptAttribute;
        private set
        {
            m_ptAttributeId = value.Id;
            m_ptAttribute = value;
        }
    }

    private PTAttribute m_ptAttribute;

    internal void RestoreReferences(AttributeManager a_attributeManager)
    {
        PTAttribute ptAttribute;
        if (m_ptAttributeExternalId != null)
        {
            ptAttribute = a_attributeManager.GetByExternalId(m_ptAttributeExternalId);
        }
        else if (m_ptAttributeId != BaseId.NULL_ID && a_attributeManager.GetById(m_ptAttributeId) is PTAttribute attribute)
        {
            ptAttribute = attribute;
        }
        else
        {
            ptAttribute = a_attributeManager.GetByExternalId(m_oldExternalId);
        }

        m_ptAttributeExternalId = null;
        PTAttribute = ptAttribute;
    }

    /// <summary>
    /// Create a new Attribute by copying an existing one.
    /// </summary>
    //public OperationAttribute(OperationAttribute a_source)
    //{
    //    Update(a_source);
    //}

    private BaseId m_ptAttributeId;

    private string m_ptAttributeExternalId;

    /// <summary>
    /// The ExternalId that links this operation attribute to a PTAttribute. This is also the key for this object in its collection
    /// </summary>
    internal string PTAttributeExternalId
    {
        get
        {
            if (PTAttribute != null)
            {
                return PTAttribute.ExternalId;
            }

            return m_ptAttributeExternalId;
        }
    }

    // Used for restoring references and the creation of the OperationAttribute dictionary within
    // the AttributeCollection of a BaseOperation.
    //
    // When adding entries to the AttributeCollection, the key is OperationAttribute.PTAttributeExternalId,
    // which means the value is needed when de-serializing Operations.
    // Operations are de-serialized before the PTAttributes are so the PTAttribute
    // that the the OperationAttribute is supposed to reference does not exist
    // when de-serializing Operations. 

    /*  ^^^^^^ INFO ABOUT ABOVE ^^^^^^
     *  We swapped back to using OperationAttribute.ExternalId in order to add entries into the AttributesCollection
     *  m_ptAttributeExternalId is just so Chelsea does not have to start over with OperationAttributes. 
     */

    /// <summary>
    /// Returns true if any changes were made.
    /// </summary>
    /// <param name="a_source"></param>
    /// <param name="a_erpUpdate"></param>
    /// <returns></returns>
    public bool Update(OperationAttribute a_source, bool a_erpUpdate)
    {
        bool changed = false;

        CodeManualUpdateOnly = a_source.CodeManualUpdateOnly;
        CostManualUpdateOnly = a_source.CostManualUpdateOnly;
        NumberManualUpdateOnly = a_source.NumberManualUpdateOnly;
        DurationManualUpdateOnly = a_source.DurationManualUpdateOnly;
        ColorCodeManualUpdateOnly = a_source.ColorCodeManualUpdateOnly;
        ShowInGanttManualUpdateOnly = a_source.ShowInGanttManualUpdateOnly;
        
        CostOverride = a_source.CostOverride;
        DurationOverride = a_source.DurationOverride;
        ColorOverride = a_source.ColorOverride;
        ShowInGanttOverride = a_source.ShowInGanttOverride;

        if (!(CodeManualUpdateOnly && a_erpUpdate) && m_code != a_source.Code)
        {
            m_code = a_source.Code;
            changed = true;
        }

        if (!(NumberManualUpdateOnly && a_erpUpdate) && m_number != a_source.m_number)
        {
            m_number = a_source.m_number;
            changed = true;
        }

        if (!(CostManualUpdateOnly && a_erpUpdate) && m_cost != a_source.Cost)
        {
            m_cost = a_source.Cost;
            changed = true;
        }

        if (!(DurationManualUpdateOnly && a_erpUpdate) && m_duration != a_source.Duration)
        {
            m_duration = a_source.Duration;
            changed = true;
        }
        
        if (!(ColorCodeManualUpdateOnly && a_erpUpdate) && m_colorCode != a_source.ColorCode)
        {
            m_colorCode = a_source.ColorCode;
            changed = true;
        }

        if (!(ShowInGanttManualUpdateOnly && a_erpUpdate) && ShowInGantt != a_source.ShowInGantt)
        {
            ShowInGantt = a_source.ShowInGantt;
            changed = true;
        }

        return changed;
    }

    public OperationAttribute(Transmissions.OperationAttribute a_attributeT, PTAttribute a_ptAttribute)
    {
        PTAttribute = a_ptAttribute;
        m_code = a_attributeT.Code;
        m_number = a_attributeT.Number;
        m_cost = a_attributeT.Cost;
        m_duration = a_attributeT.Duration;
        m_colorCode = a_attributeT.ColorCode;
        ShowInGantt = a_attributeT.ShowInGantt;

        CodeManualUpdateOnly = a_attributeT.CodeManualUpdateOnly;
        NumberManualUpdateOnly = a_attributeT.NumberManualUpdateOnly;
        CostManualUpdateOnly = a_attributeT.CostManualUpdateOnly;
        DurationManualUpdateOnly = a_attributeT.DurationManualUpdateOnly;
        ColorCodeManualUpdateOnly = a_attributeT.ColorCodeManualUpdateOnly;
        ShowInGanttManualUpdateOnly = a_attributeT.ShowInGanttOverride;

        CostOverride = a_attributeT.CostOverride;
        DurationOverride = a_attributeT.DurationOverride;
        ColorOverride = a_attributeT.ColorOverride;
        ShowInGanttOverride = a_attributeT.ShowInGanttOverride;
    }

    public OperationAttribute(PTAttribute a_ptAttribute)
    {
        PTAttribute = a_ptAttribute;
    }

    public class PTAttributeException : PTException
    {
        public PTAttributeException(string a_e)
            : base(a_e) { }
    }
    #endregion

    public bool CanRemove => !CodeManualUpdateOnly && !NumberManualUpdateOnly && !CostManualUpdateOnly && !DurationManualUpdateOnly && !ShowInGanttManualUpdateOnly;

    #region Shared Properties
    [Obsolete]
    private string m_oldExternalId;
    
    private string m_code;
    /// <summary>
    /// The data that defines the Attribute.  Can be used by Resource Attribute setup tables.
    /// </summary>
    public string Code
    {
        get => m_code;
        set => m_code = value;
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

    private decimal m_cost;
    /// <summary>
    /// The cost to perform the setup related to this Attribute.
    /// This is only used if not using Lookup tables.
    /// </summary>
    public decimal Cost
    {
        get
        {
            if (CostOverride)
            {
                return m_cost;
            }

            return PTAttribute.DefaultCost;
        }
        set => m_cost = value;
    }

    private TimeSpan m_duration;
    /// <summary>
    /// The time to perform the setup related to this Attribute.
    /// This is only used if not using Lookup tables.
    /// </summary>
    public TimeSpan Duration
    {
        get
        {
            if (DurationOverride)
            {
                return m_duration;
            }

            return PTAttribute.DefaultDuration;
        }
        set => m_duration = value;
    }

    private Color m_colorCode;
    /// <summary>
    /// Color code override for display
    /// </summary>
    public Color ColorCode
    {
        get
        {
            if (ColorOverride)
            {
                return m_colorCode;
            }

            return PTAttribute.ColorCode;
        }
        set { m_colorCode = value; }
    }

    public bool ShowInGantt
    {
        get
        {
            if (ShowInGanttOverride)
            {
                return m_bools[c_showInGanttIdx];
            }

            return PTAttribute.ShowInGantt;
        }

        set => m_bools[c_showInGanttIdx] = value;
    }

    private BoolVector32 m_bools; 
    private const short c_codeManualUpdateOnlyIdx = 0;
    private const short c_numberManualUpdateOnlyIdx = 1;
    private const short c_durationManualUpdateOnlyIdx = 2;
    private const short c_costManualUpdateOnlyIdx = 3;
    private const short c_colorManualUpdateOnlyIdx = 4;
    private const short c_durationOverrideIdx = 5;
    private const short c_costOverrideIdx = 6;
    private const short c_colorOverrideIdx = 7;
    private const short c_showInGanttOverrideIdx = 8;
    private const short c_showInGanttIdx = 9;
    private const short c_showInGanttManualUpdateOnlyIdx = 10;

    public bool ColorCodeManualUpdateOnly
    {
        get => m_bools[c_colorManualUpdateOnlyIdx];
        set => m_bools[c_colorManualUpdateOnlyIdx] = value;
    }

    public bool DurationManualUpdateOnly
    {
        get => m_bools[c_durationManualUpdateOnlyIdx];
        set => m_bools[c_durationManualUpdateOnlyIdx] = value;
    }

    public bool CostManualUpdateOnly
    {
        get => m_bools[c_costManualUpdateOnlyIdx];
        set => m_bools[c_costManualUpdateOnlyIdx] = value;
    }

    public bool NumberManualUpdateOnly
    {
        get => m_bools[c_numberManualUpdateOnlyIdx];
        set => m_bools[c_numberManualUpdateOnlyIdx] = value;
    }

    public bool CodeManualUpdateOnly
    {
        get => m_bools[c_codeManualUpdateOnlyIdx];
        set => m_bools[c_codeManualUpdateOnlyIdx] = value;
    }

    public bool ShowInGanttManualUpdateOnly
    {
        get => m_bools[c_showInGanttManualUpdateOnlyIdx];
        set => m_bools[c_showInGanttManualUpdateOnlyIdx] = value;
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
        if (PTAttribute.Description != null && PTAttribute.Description.Trim() != PTAttribute.Name.Trim()) //no sense showing the description if it's the same as the Name.
        {
            descStr = string.Format("({0}) ", PTAttribute.Description.Trim());
        }

        string cdeStr = "";
        if (Code != null)
        {
            cdeStr = string.Format("{0} ", Code.Trim());
        }

        string nbrStr = "";
        if (Number != -1 && Number.ToString().Trim() != cdeStr.Trim()) //no sense showing the description if it's the same as the Name.
        {
            nbrStr = Number.ToString();
        }

        return string.Format("{0} {1}: {2}{3}", PTAttribute.Name.Trim(), descStr, cdeStr, nbrStr);
    }

    /// <summary>
    /// Compare two PTAttribute.Code values for equivalence and having a length greater than 0.
    /// If either attribute is null false is returned.
    /// </summary>
    /// <param name="a_pta1">Can be null. The Code is examined.</param>
    /// <param name="a_pta2">Can be null. The Code is examined.</param>
    /// <returns></returns>
    public static bool CodesEqualNotNullAndLengthGreaterThanZero(OperationAttribute a_pta1, OperationAttribute a_pta2)
    {
        if (a_pta1 != null && a_pta2 != null)
        {
            return TextUtil.EqualAndLengthsGreaterThanZero(a_pta1.Code, a_pta2.Code);
        }

        return false;
    }

    public void SetDataValueFromSerializedData(string a_propertyName, byte[] a_value)
    {
        using BinaryMemoryReader reader = new(a_value);
        string propertyName = a_propertyName.ToLower().Trim();

        if (propertyName.EndsWith("code"))
        {
            reader.Read(out string code);
            Code = code;
        }
        else if (propertyName.EndsWith("color"))
        {
            reader.Read(out Color color);
            ColorOverride = true;
            ColorCode = color;
        }
        else if (propertyName.EndsWith("number"))
        {
            reader.Read(out decimal number);
            Number = number;
        }
        else
        {
            DebugException.ThrowInDebug("Unsupported Attribute Property Type.");
        }
    }
}

#region Attributes Collection
/// <summary>
/// Stores a list of OperationAttributes.
/// </summary>
public class AttributesCollection : ICopyTable, IPTSerializable, IEnumerable<OperationAttribute>
{
    public const int UNIQUE_ID = 17;

    #region IPTSerializable Members
    public AttributesCollection(IReader a_reader)
    {
        if (a_reader.VersionNumber >= 12413)
        {
            a_reader.Read(out int count);
            for (int i = 0; i < count; i++)
            {
                OperationAttribute a = new(a_reader);
                Add(a);
            }
        }
        else if (a_reader.VersionNumber >= 12400)
        {
            // For the versions of Neptune (12.2.1.x) that still had the faulty serialization
            m_backwardCompatibleList = new List<OperationAttribute>();
            a_reader.Read(out int count);
            for (int i = 0; i < count; i++)
            {
                OperationAttribute a = new(a_reader);
                //We can't add the attribute yet, because it needs to restore references to get the PTAttribute ExternalId
                m_backwardCompatibleList.Add(a);
            }
        }
        else if (a_reader.VersionNumber >= 12329)
        {
            // The serialization changes were merged back into the 12.2.0 branch
            // and this block corresponds to the version number in that branch
            a_reader.Read(out int count);
            for (int i = 0; i < count; i++)
            {
                OperationAttribute a = new(a_reader);
                Add(a);
            }
        }
        else if (a_reader.VersionNumber >= 12300)
        {
            m_backwardCompatibleList = new List<OperationAttribute>();
            a_reader.Read(out int count);
            for (int i = 0; i < count; i++)
            {
                OperationAttribute a = new (a_reader);
                //We can't add the attribute yet, because it needs to restore references to get the PTAttribute ExternalId
                m_backwardCompatibleList.Add(a);
            }
        }
        else if (a_reader.VersionNumber >= 1)
        {
            a_reader.Read(out int count);
            for (int i = 0; i < count; i++)
            {
                OperationAttribute a = new (a_reader);
            }
        }
    }

    public void Serialize(IWriter a_writer)
    {
        #if DEBUG
        a_writer.DuplicateErrorCheck(this);
        #endif
        a_writer.Write(Count);
        for (int i = 0; i < Count; i++)
        {
            this[i].Serialize(a_writer);
        }
    }

    public virtual int UniqueId => UNIQUE_ID;

    public void RestoreReferences(AttributeManager a_attributeManager)
    {
        foreach (OperationAttribute opAttribute in m_attributes.Values)
        {
            opAttribute.RestoreReferences(a_attributeManager);
        }

        //For backward compatibility before 12413
        if (m_backwardCompatibleList != null)
        {
            foreach (OperationAttribute operationAttribute in m_backwardCompatibleList)
            {
                operationAttribute.RestoreReferences(a_attributeManager);
                Add(operationAttribute);
            }
        }

        //Temporary for broken scenarios created during the development of PTAttributes
        for (int i = m_attributes.Values.Count - 1; i >= 0; i--)
        {
            if (m_attributes.Values[i].PTAttribute == null)
            {
                m_attributes.RemoveAt(i);
            }
        }
    }
    #endregion

    #region Declarations
    private readonly SortedList<string, OperationAttribute> m_attributes = new ();
    private List<OperationAttribute> m_backwardCompatibleList;
    public int Count => m_attributes.Count;
    public string AttributesSummary => m_attributes != null ? GetSummary() : "";
    public string AttributeSummaryGantt => m_attributes != null ? GetGanttSummary() : "";

    public class AttributesCollectionException : PTException
    {
        public AttributesCollectionException(string a_message)
            : base(a_message) { }
    }

    public class AttributesCollectionValidationException : PTValidationException
    {
        public AttributesCollectionValidationException(string a_msg, object[] a_params = null)
            : base(a_msg, a_params) { }
    }
    #endregion

    #region Construction
    public AttributesCollection() { }
    #endregion

    #region List Maintenance
    public Type ElementType => typeof(OperationAttribute);

    public OperationAttribute Add(OperationAttribute a_operationAttribute)
    {
        if (m_attributes.ContainsKey(a_operationAttribute.PTAttributeExternalId))
        { 
            throw new AttributesCollectionValidationException("2855", new object[] { a_operationAttribute.PTAttributeExternalId });
        }

        m_attributes.Add(a_operationAttribute.PTAttributeExternalId, a_operationAttribute);
        return a_operationAttribute;
    }

    public void Remove(int a_index)
    {
        m_attributes.RemoveAt(a_index);
    }

    public void Remove(string a_ptAttributeExternalId)
    {
        m_attributes.Remove(a_ptAttributeExternalId);
    }

    public object GetRow(int a_index)
    {
        return m_attributes.Values[a_index];
    }

    public OperationAttribute this[int a_index] => m_attributes.Values[a_index];

    /// <summary>
    /// If the Attribute doesn't exist an error is thrown.  Use Contains() function first.
    /// </summary>
    /// <param name="a_ptAttributeExternalId"></param>
    /// <returns></returns>
    public OperationAttribute GetByExternalId(string a_ptAttributeExternalId)
    {
        if (m_attributes.TryGetValue(a_ptAttributeExternalId, out OperationAttribute opAttribute))
        {
            return opAttribute;
        }

        return null;
    }

    public bool TryGetValue(string a_attributeId, out OperationAttribute a_operationAttr)
    {
        return m_attributes.TryGetValue(a_attributeId, out a_operationAttr);
    }

    public void Clear()
    {
        m_attributes.Clear();
    }
    #endregion

    /// <summary>
    /// Returns a comma separated list of Attribute values to use for Reports.  Sorted by Name.
    /// Example: Color ([description]): Red/3, Width...  (Name(Description) Code/Nbr)
    /// </summary>
    /// <returns></returns>
    public string GetSummary()
    {
        System.Text.StringBuilder builder = new ();

        for (int i = 0; i < Count; i++)
        {
            OperationAttribute operationAttribute = this[i];
            if (i == 0)
            {
                builder.Append(operationAttribute);
            }
            else
            {
                builder.Append(", ");
                builder.Append(operationAttribute);
            }
        }

        return builder.ToString();
    }

    /// <summary>
    /// Returns attribute descriptions only for attributes shown in gantt
    /// </summary>
    public string GetGanttSummary()
    {
        System.Text.StringBuilder builder = new ();

        for (int i = 0; i < Count; i++)
        {
            OperationAttribute operationAttribute = this[i];
            if (!operationAttribute.PTAttribute.ShowInGantt)
            {
                continue;
            }

            if (builder.Length == 0)
            {
                builder.Append(operationAttribute);
            }
            else
            {
                builder.Append(", ");
                builder.Append(operationAttribute);
            }
        }

        return builder.ToString();
    }

    public List<OperationAttribute> GetAttributesWhoseNamesStartWith(string startWith)
    {
        List<OperationAttribute> atts = new ();
        for (int i = 0; i < Count; ++i)
        {
            OperationAttribute att = this[i];
            if (att.PTAttribute.Name.StartsWith(startWith, true, null))
            {
                atts.Add(att);
            }
        }

        return atts;
    }

    public List<OperationAttribute> GetAttributesByNamePattern(System.Text.RegularExpressions.Regex a_pattern)
    {
        List<OperationAttribute> atts = new ();
        for (int i = 0; i < Count; ++i)
        {
            OperationAttribute att = this[i];
            if (a_pattern.IsMatch(att.PTAttribute.Name))
            {
                atts.Add(att);
            }
        }

        return atts;
    }

    public IEnumerator<OperationAttribute> GetEnumerator()
    {
        foreach (OperationAttribute attribute in m_attributes.Values)
        {
            yield return attribute;
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

}
#endregion