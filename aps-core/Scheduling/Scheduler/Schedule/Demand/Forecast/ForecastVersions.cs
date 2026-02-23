using PT.APSCommon;
using PT.Transmissions;
using System;
using System.Collections;
using System.ComponentModel;

namespace PT.Scheduler.Demand;

/// <summary>
/// The master list of forecasts for a particular Inventory object.
/// </summary>
public class ForecastVersions : IPTSerializable, AfterRestoreReferences.IAfterRestoreReferences
{
    #region IPTSerializable Members
    public const int UNIQUE_ID = 632;

    internal ForecastVersions(IReader reader, IIdGenerator aIdGen)
    {
        m_idGen = aIdGen;

        if (reader.VersionNumber >= 257)
        {
            int versionCount;
            reader.Read(out versionCount);

            for (int i = 0; i < versionCount; i++)
            {
                ForecastVersion forecastVersion = new (reader, aIdGen);
                versions.Add(forecastVersion);
            }
        }
        else
        {
            int versionCount;
            reader.Read(out versionCount);

            for (int i = 0; i < versionCount; i++)
            {
                ForecastVersion forecastVersion = new (reader, aIdGen, null);
                versions.Add(forecastVersion);
            }
        }
    }

    internal void RestoreReferences(CustomerManager a_cm, Inventory a_inv)
    {
        m_inventory = a_inv;
        for (int i = 0; i < versions.Count; ++i)
        {
            ForecastVersion fv = versions[i];
            fv.RestoreReferences(a_cm, this);
        }
    }

    internal void RestoreReferences(UserFieldDefinitionManager a_udfManager)
    {
        for (int i = 0; i < versions.Count; ++i)
        {
            ForecastVersion fv = versions[i];
            fv.RestoreReferences(a_udfManager);
        }
    }

    public void Serialize(IWriter writer)
    {
        writer.Write(versions.Count);
        for (int i = 0; i < versions.Count; i++)
        {
            versions[i].Serialize(writer);
        }
    }

    [Browsable(false)]
    public int UniqueId => UNIQUE_ID;
    #endregion IPTSerializable

    internal ForecastVersions(Inventory a_inv, BaseIdGenerator aIdGen)
    {
        m_idGen = aIdGen;
        m_inventory = a_inv;
    }

    internal ForecastVersions(CustomerManager a_customerManager, Inventory a_inv, ERPTransmissions.ForecastT.ForecastVersions tForecastVersions, BaseIdGenerator aIdGen, PTTransmission t, UserFieldDefinitionManager a_udfManager)
    {
        m_idGen = aIdGen;
        m_inventory = a_inv;

        for (int i = 0; i < tForecastVersions.Versions.Count; i++)
        {
            versions.Add(new ForecastVersion(this, a_customerManager, tForecastVersions.Versions[i], m_idGen, m_idGen.NextID(), t, a_udfManager));
        }
    }

    private Inventory m_inventory;

    /// <summary>
    /// The Inventory to which the Forecast Version belongs.
    /// </summary>
    public Inventory Inventory => m_inventory;

    internal void Update(UserFieldDefinitionManager a_udfManager, ERPTransmissions.ForecastT t, ERPTransmissions.ForecastT.ForecastVersions tForecastVersions, out bool mrpNetChangeCriticalUpdates, ScenarioDetail a_sd)
    {
        mrpNetChangeCriticalUpdates = false;

        Hashtable updatedVersionsHash = new ();
        for (int i = 0; i < tForecastVersions.Versions.Count; i++)
        {
            ERPTransmissions.ForecastT.ForecastVersion tForecastVersion = tForecastVersions.Versions[i];
            ForecastVersion forecastVersion = Find(tForecastVersion.Version);
            if (forecastVersion == null)
            {
                versions.Add(new ForecastVersion(this, a_sd.CustomerManager, tForecastVersion, m_idGen, m_idGen.NextID(), t, a_udfManager));
                mrpNetChangeCriticalUpdates = true;
            }
            else
            {
                forecastVersion.Update(a_udfManager, t, tForecastVersion, out bool specificMrpNetChangeCriticalUpdates, a_sd);
            }

            if (!updatedVersionsHash.Contains(tForecastVersion.Version))
            {
                updatedVersionsHash.Add(tForecastVersion.Version, null);
            }
        }

        if (t.AutoDeleteMode)
        {
            for (int i = versions.Count - 1; i >= 0; i--)
            {
                ForecastVersion fv = versions[i];
                if (!updatedVersionsHash.ContainsKey(fv.Version))
                {
                    Delete(a_sd, fv, t);
                    mrpNetChangeCriticalUpdates = true;
                }
            }
        }
    }

    internal void Clearing(ScenarioDetail a_sd, PTTransmissionBase a_t)
    {
        for (int i = 0; i < versions.Count; i++)
        {
            versions[i].Deleting(a_sd, a_t);
        }
    }

    private void Delete(ScenarioDetail a_sd, ForecastVersion a_fv, PTTransmissionBase a_t)
    {
        a_fv.Deleting(a_sd, a_t);
        versions.Remove(a_fv);
    }

    /// <summary>
    /// Returns the matching version (using iterative search) or null if not found.
    /// </summary>
    internal ForecastVersion Find(string version)
    {
        for (int i = 0; i < versions.Count; i++)
        {
            if (versions[i].Version == version)
            {
                return versions[i];
            }
        }

        return null;
    }

    private readonly List<ForecastVersion> versions = new ();

    public List<ForecastVersion> Versions => versions;

    internal void AddVersion(ForecastVersion a_version)
    {
        ForecastVersion ver = Find(a_version.Version);
        if (ver == null)
        {
            versions.Add(a_version);
        }
        else
        {
            throw new PTValidationException("ForecastVersion with Version '{0}' already exists.", new object[] { a_version.Version });
        }
    }

    internal ForecastVersion Find(BaseId a_versionId)
    {
        for (int i = 0; i < versions.Count; i++)
        {
            if (versions[i].Id == a_versionId)
            {
                return versions[i];
            }
        }

        return null;
    }

    #region IAfterRestoreReferences Members
    private readonly IIdGenerator m_idGen;

    void AfterRestoreReferences.IAfterRestoreReferences.AfterRestoreReferences_1(int serializationVersionNbr, HashSet<object> processedAfterRestoreReferences_1, HashSet<object> processedAfterRestoreReferences_2)
    {
        AfterRestoreReferences.Helpers.IEnumerableHelperFor_AfterRestoreReferences_1(serializationVersionNbr, m_idGen, versions, this, processedAfterRestoreReferences_1, processedAfterRestoreReferences_2);
    }

    void AfterRestoreReferences.IAfterRestoreReferences.AfterRestoreReferences_2(int serializationVersionNbr, HashSet<object> processedAfterRestoreReferences_1, HashSet<object> processedAfterRestoreReferences_2)
    {
        AfterRestoreReferences.Helpers.IEnumerableHelperFor_AfterRestoreReferences_2(serializationVersionNbr, versions, this, processedAfterRestoreReferences_1, processedAfterRestoreReferences_2);
    }
    #endregion
}