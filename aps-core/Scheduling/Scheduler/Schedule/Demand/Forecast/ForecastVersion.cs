using System.Collections;
using System.ComponentModel;

using PT.APSCommon;
using PT.APSCommon.Extensions;
using PT.SchedulerDefinitions;
using PT.Transmissions;

namespace PT.Scheduler.Demand;

/// <summary>
/// Contains a list of forecasts that are based on a particular set of planning assumptions.
/// Various versions can be created to compare possibilities to evaluate "what-ifs" such as running a special sale to increase demand for a product.
/// </summary>
public class ForecastVersion : BaseIdObject
{
    #region IPTSerializable Members
    public new const int UNIQUE_ID = 631;

    internal ForecastVersion(IReader reader, IIdGenerator aIdGen)
        : base(reader)
    {
        m_idGen = aIdGen;

        reader.Read(out version);
        int forecastCount;
        reader.Read(out forecastCount);
        for (int i = 0; i < forecastCount; i++)
        {
            Forecast forecast = new (reader);
            m_forecasts.Add(forecast);
        }
        #if TEST
            CompareDesyncResults();
        #endif
    }

    internal ForecastVersion(IReader reader, IIdGenerator aIdGen, object BACKWARDS_COMPATIBILITY)
        : base(BaseId.NULL_ID) // Before switch to BaseIdObject
    {
        m_idGen = aIdGen;

        reader.Read(out version);
        int forecastCount;
        reader.Read(out forecastCount);
        for (int i = 0; i < forecastCount; i++)
        {
            Forecast forecast = new (reader);
            m_forecasts.Add(forecast);
        }
        #if TEST
            CompareDesyncResults();
        #endif
    }

    internal void RestoreReferences(CustomerManager a_cm, ForecastVersions a_forecastVersions)
    {
        m_forecastVersions = a_forecastVersions;
        for (int i = 0; i < m_forecasts.Count; ++i)
        {
            Forecast f = m_forecasts[i];
            f.RestoreReferences(a_cm, this);
        }
    }

    internal void RestoreReferences(UserFieldDefinitionManager a_udfManager)
    {
        for (int i = 0; i < m_forecasts.Count; ++i)
        {
            Forecast f = m_forecasts[i];
            a_udfManager.RestoreReferences(f, UserField.EUDFObjectType.Forecasts);
        }
    }

    public override void Serialize(IWriter writer)
    {
        base.Serialize(writer);

        writer.Write(version);
        writer.Write(m_forecasts.Count);
        for (int i = 0; i < m_forecasts.Count; i++)
        {
            m_forecasts[i].Serialize(writer);
        }
    }

    [Browsable(false)]
    public override int UniqueId => UNIQUE_ID;
    #endregion IPTSerializable

    internal ForecastVersion(ForecastVersions a_forecastVersions, CustomerManager a_customerManager, ERPTransmissions.ForecastT.ForecastVersion tForecastVersion, IIdGenerator aIdGen, BaseId aId, PTTransmission t, UserFieldDefinitionManager a_udfManager)
        : base(aId)
    {
        m_idGen = aIdGen;
        m_forecastVersions = a_forecastVersions;

        version = tForecastVersion.Version;
        foreach (PT.ERPTransmissions.ForecastT.Forecast tForecast in tForecastVersion.Forecasts.Values)
        {
            Customer customer = a_customerManager.GetByExternalId(tForecast.Customer);
            m_forecasts.Add(new Forecast(this, tForecast, customer, m_idGen.NextID(), t, a_udfManager));
        }
    }

    internal ForecastVersion(ForecastVersions a_forecastVersions, IIdGenerator aIdGen, BaseId aId, string aVersion)
        : base(aId)
    {
        m_idGen = aIdGen;
        m_forecastVersions = a_forecastVersions;

        version = aVersion;
    }

    internal ForecastVersion(Inventory a_inv, BaseIdGenerator a_idGen, BaseId aId, string aVersion)
        : base(aId)
    {
        m_idGen = a_idGen;
        m_forecastVersions = new ForecastVersions(a_inv, a_idGen);

        version = aVersion;
    }

    private ForecastVersions m_forecastVersions;

    /// <summary>
    /// The ForecastVersions to which this Forecast Version belongs.
    /// </summary>
    public ForecastVersions ForecastVersions => m_forecastVersions;

    internal void Update(UserFieldDefinitionManager a_udfManager, ERPTransmissions.ForecastT t, ERPTransmissions.ForecastT.ForecastVersion tForecastVersion, out bool mrpNetChangeCriticalUpdates, ScenarioDetail a_sd)
    {
        mrpNetChangeCriticalUpdates = false;

        Hashtable updatedForecasts = new ();
        foreach (PT.ERPTransmissions.ForecastT.Forecast tForecast in tForecastVersion.Forecasts.Values)
        {
            Forecast forecast = GetByExternalId(tForecast.ExternalId);
            if (forecast == null)
            {
                Customer cust = a_sd.CustomerManager.GetByExternalId(tForecast.Customer);
                Forecast fcast = new (this, tForecast, cust, m_idGen.NextID(), t, a_udfManager);
                m_forecasts.Add(fcast);
                mrpNetChangeCriticalUpdates = true;
            }
            else
            {
                forecast.Update(a_udfManager, tForecast, out mrpNetChangeCriticalUpdates, a_sd, t);
            }

            if (!updatedForecasts.Contains(tForecast.ExternalId))
            {
                updatedForecasts.Add(tForecast.ExternalId, null);
            }
        }

        if (t.AutoDeleteMode)
        {
            for (int i = m_forecasts.Count - 1; i >= 0; i--)
            {
                Forecast f = m_forecasts[i];
                if (!updatedForecasts.ContainsKey(f.ExternalId))
                {
                    f.DeletingOrClearingShipments(a_sd, t);
                    m_forecasts.Remove(f);
                    mrpNetChangeCriticalUpdates = true;
                }
            }
        }
    }

    private readonly IIdGenerator m_idGen;

    internal IIdGenerator IdGen => m_idGen;

    private string version;

    /// <summary>
    /// Uniquue identifier.
    /// </summary>
    public string Version
    {
        get => version;
        set => version = value;
    }

    /// <summary>
    /// Returns the Forecast or null if not found. (Iterative search). Should be very low number of these -- 1 to five maybe.
    /// </summary>
    /// <param name="externalId"></param>
    /// <returns></returns>
    internal Forecast GetByExternalId(string externalId)
    {
        for (int i = 0; i < m_forecasts.Count; i++)
        {
            if (m_forecasts[i].ExternalId == externalId)
            {
                return m_forecasts[i];
            }
        }

        return null;
    }

    /// <summary>
    /// Returns the Forecast or null if not found. (Iterative search). Should be very low number of these -- 1 to five maybe.
    /// </summary>
    /// <param name="externalId"></param>
    /// <returns></returns>
    public Forecast GetById(BaseId a_id)
    {
        for (int i = 0; i < m_forecasts.Count; i++)
        {
            if (m_forecasts[i].Id == a_id)
            {
                return m_forecasts[i];
            }
        }

        return null;
    }

    private readonly List<Forecast> m_forecasts = new ();

    public List<Forecast> Forecasts => m_forecasts;

    #region IAfterRestoreReferences Members
    public override void AfterRestoreReferences_1(int serializationVersionNbr, HashSet<object> processedAfterRestoreReferences_1, HashSet<object> processedAfterRestoreReferences_2)
    {
        AfterRestoreReferences.Helpers.IEnumerableHelperFor_AfterRestoreReferences_1(serializationVersionNbr, m_idGen, m_forecasts, this, processedAfterRestoreReferences_1, processedAfterRestoreReferences_2);
    }

    public override void AfterRestoreReferences_2(int serializationVersionNbr, HashSet<object> processedAfterRestoreReferences_1, HashSet<object> processedAfterRestoreReferences_2)
    {
        AfterRestoreReferences.Helpers.IEnumerableHelperFor_AfterRestoreReferences_2(serializationVersionNbr, m_forecasts, this, processedAfterRestoreReferences_1, processedAfterRestoreReferences_2);
    }
    #endregion

    internal void Deleting(ScenarioDetail a_sd, PTTransmissionBase a_t)
    {
        for (int i = Forecasts.Count - 1; i >= 0; i--)
        {
            Forecasts[i].DeletingOrClearingShipments(a_sd, a_t);
            Forecasts.RemoveAt(i);
        }
    }

    /// <summary>
    /// Clears shipments and deletes forecast
    /// </summary>
    internal void DeleteForecast(ScenarioDetail a_sd, Forecast a_forecast, PTTransmissionBase a_t)
    {
        a_forecast.DeletingOrClearingShipments(a_sd, a_t);
        Forecasts.Remove(a_forecast);
    }

    private int m_nextExternalIdNbr = 1;

    /// <summary>
    /// Returns the next unique ExternalId and increments the counter
    /// </summary>
    /// <returns></returns>
    public string NextForecastExternalId()
    {
        int nextId = m_nextExternalIdNbr;
        while (GetByExternalId(ExternalBaseIdObject.MakeExternalId(nextId)) != null)
        {
            nextId++;
        }

        m_nextExternalIdNbr = nextId + 1;
        return ExternalBaseIdObject.MakeExternalId(nextId);
    }
}