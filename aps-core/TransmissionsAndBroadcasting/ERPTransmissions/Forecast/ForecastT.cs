using System.Collections;
using System.ComponentModel;

namespace PT.ERPTransmissions;

public partial class ForecastT : ERPMaintenanceTransmission<ForecastT.Forecast>, IPTSerializable
{
    #region IPTSerializable Members
    public new const int UNIQUE_ID = 637;

    public ForecastT(IReader reader) : base(reader)
    {
        int count;
        reader.Read(out count);
        for (int i = 0; i < count; i++)
        {
            ForecastVersions invForecasts = new (reader);
            inventoryForecasts.Add(invForecasts);
        }
    }

    public override void Serialize(IWriter writer)
    {
        base.Serialize(writer);
        writer.Write(inventoryForecasts.Count);
        for (int i = 0; i < inventoryForecasts.Count; i++)
        {
            inventoryForecasts[i].Serialize(writer);
        }
    }

    [Browsable(false)]
    public override int UniqueId => UNIQUE_ID;
    #endregion IPTSerializable

    public ForecastT() { }

    private readonly List<ForecastVersions> inventoryForecasts = new ();

    /// <summary>
    /// Lists a set of Forecasts for Inventory objects.
    /// </summary>
    public List<ForecastVersions> InventoryForecasts => inventoryForecasts;

    public override int Count => InventoryForecasts.Count;

    public override string Description => "Forecast updated";

    #region Database Loading
    public void Fill(System.Data.IDbCommand forecastTableCmd, System.Data.IDbCommand forecastShipmentsCmd, Transmissions.ApplicationExceptionList a_error)
    {
        ForecastTDataSet ds = new ();
        FillTable(ds.Forecasts, forecastTableCmd);
        FillTable(ds.ForecastShipments, forecastShipmentsCmd);

        Fill(ds, a_error);
    }

    private readonly Hashtable addedForecastVersionsHash = new ();

    private string GetForecastVersionsKey(string itemExternalId, string warehouseExternalId)
    {
        return itemExternalId + "$#$!@" + warehouseExternalId;
    }

    /// <summary>
    /// Fill the transmission with data from the DataSet.
    /// </summary>
    /// <param name="ds"></param>
    public void Fill(ForecastTDataSet ds, Transmissions.ApplicationExceptionList a_errors)
    {
        try
        {
            for (int i = 0; i < ds.Forecasts.Count; i++)
            {
                //The ForecastTDataSet does not have the four layers of the hierarch here so that it's simpler in the integration setup (fewer queries).  So we'll have to do some grouping here.
                ForecastTDataSet.ForecastsRow row = ds.Forecasts[i];
                ForecastVersions forecastVersions;

                Forecast forecast = new Forecast(row);
                forecast.Validate();

                if (!addedForecastVersionsHash.Contains(GetForecastVersionsKey(row.ItemExternalId, row.WarehouseExternalId)))
                {
                    forecastVersions = new ForecastVersions(row);
                    inventoryForecasts.Add(forecastVersions);
                    addedForecastVersionsHash.Add(GetForecastVersionsKey(row.ItemExternalId, row.WarehouseExternalId), forecastVersions);
                }
                else //already have a ForecastVersions object for this Item/Warehouse combination                
                {
                    forecastVersions = (ForecastVersions)addedForecastVersionsHash[GetForecastVersionsKey(row.ItemExternalId, row.WarehouseExternalId)];
                }

                //See if this version is there already

                ForecastVersion forecastVersion = forecastVersions.Find(row.ForecastVersion);
                if (forecastVersion == null)
                {
                    forecastVersion = new ForecastVersion(row);
                    forecastVersions.Versions.Add(forecastVersion);
                }

                forecastVersion.Add(forecast);
            }
        }
        catch (Transmissions.ValidationException err)
        {
            a_errors.Add(err);
        }
    }
    #endregion Database Loading
}