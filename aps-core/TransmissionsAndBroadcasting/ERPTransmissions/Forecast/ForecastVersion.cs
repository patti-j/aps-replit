using System.ComponentModel;

namespace PT.ERPTransmissions;

public partial class ForecastT
{
    /// <summary>
    /// The master list of forecasts for a particular Inventory object.
    /// </summary>
    public class ForecastVersions : IPTSerializable
    {
        #region IPTSerializable Members
        public const int UNIQUE_ID = 636;

        public ForecastVersions(IReader reader)
        {
            reader.Read(out itemExternalId);
            reader.Read(out warehouseExternalId);

            int versionCount;
            reader.Read(out versionCount);
            for (int i = 0; i < versionCount; i++)
            {
                ForecastVersion forecastVersion = new (reader);
                versions.Add(forecastVersion);
            }
        }

        public void Serialize(IWriter writer)
        {
            writer.Write(itemExternalId);
            writer.Write(warehouseExternalId);

            writer.Write(versions.Count);
            for (int i = 0; i < versions.Count; i++)
            {
                versions[i].Serialize(writer);
            }
        }

        [Browsable(false)]
        public int UniqueId => UNIQUE_ID;
        #endregion IPTSerializable

        public ForecastVersions() { } // reqd. for xml serialization

        public ForecastVersions(ForecastTDataSet.ForecastsRow row)
        {
            itemExternalId = row.ItemExternalId;
            warehouseExternalId = row.WarehouseExternalId;
        }

        private string itemExternalId;

        /// <summary>
        /// The item the Forecasts are for.
        /// </summary>
        public string ItemExternalId
        {
            get => itemExternalId;
            set => itemExternalId = value;
        }

        private string warehouseExternalId;

        /// <summary>
        /// The Warehouse the Forecasts are for.
        /// </summary>
        public string WarehouseExternalId
        {
            get => warehouseExternalId;
            set => warehouseExternalId = value;
        }

        private readonly List<ForecastVersion> versions = new ();

        public List<ForecastVersion> Versions => versions;

        /// <summary>
        /// Returns the Forecast Version with the matching (case-sensitive) version or null if one doesn't exist.
        /// </summary>
        /// <param name="version"></param>
        /// <returns></returns>
        public ForecastVersion Find(string version)
        {
            //Probably just a few of these so a search should be ok
            for (int i = 0; i < versions.Count; i++)
            {
                if (versions[i].Version == version)
                {
                    return versions[i];
                }
            }

            return null;
        }
    }

    /// <summary>
    /// Contains a list of forecasts that are based on a particular set of planning assumptions.
    /// Various versions can be created to compare possibilities to evaluate "what-ifs" such as running a special sale to increase demand for a product.
    /// </summary>
    public class ForecastVersion : IPTSerializable
    {
        #region IPTSerializable Members
        public const int UNIQUE_ID = 635;

        public ForecastVersion(IReader reader)
        {
            reader.Read(out version);
            int forecastCount;
            reader.Read(out forecastCount);
            for (int i = 0; i < forecastCount; i++)
            {
                try
                {
                    Add(new Forecast(reader));
                }
                catch (Transmissions.ValidationException) { } // this is so forecastTs that were serialized before this validation can open. Without it, some scenarios may not open.
            }
        }

        public void Serialize(IWriter writer)
        {
            writer.Write(version);
            writer.Write(m_forecasts.Count);
            foreach (Forecast f in m_forecasts.Values)
            {
                f.Serialize(writer);
            }
        }

        [Browsable(false)]
        public int UniqueId => UNIQUE_ID;
        #endregion IPTSerializable

        public ForecastVersion() { } // reqd. for xml serialization

        public ForecastVersion(ForecastTDataSet.ForecastsRow row)
        {
            version = row.ForecastVersion;
        }

        private string version;

        /// <summary>
        /// Uniquue identifier.
        /// </summary>
        public string Version
        {
            get => version;
            set => version = value;
        }

        private readonly Dictionary<string, Forecast> m_forecasts = new ();

        public Dictionary<string, Forecast> Forecasts => m_forecasts;

        public void Add(Forecast a_forecast)
        {
            if (m_forecasts.ContainsKey(a_forecast.ExternalId))
            {
                throw new Transmissions.ValidationException("2045", new object[] { a_forecast.GetType().FullName, a_forecast.ExternalId });
            }

            m_forecasts.Add(a_forecast.ExternalId, a_forecast);
        }
    }
}