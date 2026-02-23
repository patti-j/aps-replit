namespace PT.Transmissions;

/// <summary>
/// A Resource Connector can optionally be used to specify allowable flows of material between Resources.
/// If a Resource has one or more Connectors then the system will only schedule successor Operations of Operations scheduled on the Resource to Connected Resources.
/// This can be used to constrain Operation scheduling based on physical connections such as pipes or conveyors that link machines together.
/// This can also be used to specify transit times between Resources.
/// Note that there is a related AlternatePath Node field called AllowManualConnectorViolation that specifies whether Connector constraints can be overridden.
/// </summary>
public class ConnectorBase : IPTSerializable
{
    public const int UNIQUE_ID = 667;

    #region IPTSerializable Members
    public ConnectorBase(IReader reader)
    {
        if (reader.VersionNumber >= 455)
        {
            reader.Read(out m_fixedTransitSpan);
        }

        #region 1
        else if (reader.VersionNumber >= 1)
        {
            reader.Read(out m_fixedTransitSpan);

            decimal tmp;
            reader.Read(out tmp); // flow rate

            int tempValue;
            reader.Read(out tempValue); // flowRateTimeUnit
            reader.Read(out tempValue); // transitSpanTypes
        }
        #endregion
    }

    public virtual void Serialize(IWriter writer)
    {
        writer.Write(m_fixedTransitSpan);
    }

    public virtual int UniqueId => UNIQUE_ID;
    #endregion

    public ConnectorBase() { }

    #region Shared Properties
    private TimeSpan m_fixedTransitSpan;

    /// <summary>
    /// Specified a constant value for the time for material to move to the downstream Resource.
    /// </summary>
    public TimeSpan FixedTransitSpan
    {
        get => m_fixedTransitSpan;
        set => m_fixedTransitSpan = value;
    }
    #endregion Shared Properties
}