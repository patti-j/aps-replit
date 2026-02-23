using PT.Common.Compression;
using PT.Common.Extensions;

namespace PT.Scheduler;

internal class DataModelActivation
{
    #region Members
    private const double c_scoreThreshold = .8;
    private readonly List<IDataModelActivationModule> m_modules = new ();
    private string m_key;

    internal string Key => m_key;
    #endregion

    internal DataModelActivation(ScenarioDetail a_sd)
    {
        AddModules();
        CreateKey(a_sd);
    }

    internal DataModelActivation(string a_key)
    {
        AddModules();

        byte[] bytes = Convert.FromBase64String(a_key);
        using (BinaryMemoryReader reader = new (bytes))
        {
            foreach (IDataModelActivationModule module in m_modules)
            {
                module.CreateListFromReader(reader);
            }
        }
    }

    internal void AddModules()
    {
        //Everything that is using a name use 50% or greater is pass
        //<95% is danger zone, <90% is failure
        m_modules.Add(new ResourceDataModule());
        m_modules.Add(new ItemDataModule());
        m_modules.Add(new OperationAttributeModule());
        m_modules.Add(new PurchaseOrderModule());
        m_modules.Add(new WarehouseModule());
        m_modules.Add(new CapabilityModule());
        m_modules.Add(new CustomerModule());
        m_modules.Add(new PlantModule());
        m_modules.Add(new DepartmentModule());
        m_modules.Add(new FeaturesAndBoolsModule()); //75% is fail.  Under 100% is a warning
        m_modules.Add(new CountsModules()); //Look at the member variable comments
    }

    /// <summary>
    /// This is used to build an activation key based on resource names, items, wh names, op attributes, customer names, etc...
    /// This key is used for facilities that are not able to connect to the wider internet
    /// </summary>
    internal void CreateKey(ScenarioDetail a_sd)
    {
        using (BinaryMemoryWriter writer = new (ECompressionType.Fast))
        {
            foreach (IDataModelActivationModule dataModule in m_modules)
            {
                dataModule.CreateKeyMaterialAndSerialize(a_sd, writer);
            }

            m_key = Convert.ToBase64String(writer.GetBuffer());
        }
    }

    /// <summary>
    /// This iterates through the data modules and runs their CompareTo operations.  If any module fails an exception is thrown.  This should be caught and the client sent into readonly.
    /// If a fail is returned the scenario is in the danger zone and a message should be displayed
    /// </summary>
    internal bool CompareTo(ScenarioDetail a_sd)
    {
        double score = 0;
        bool showDangerMessage = false;

        foreach (IDataModelActivationModule dataModule in m_modules)
        {
            if (!dataModule.CompareToSd(a_sd))
            {
                showDangerMessage = true;
            }
        }

        return !showDangerMessage;
    }
}

#region DataModules
internal interface IDataModelActivationModule : IPTSerializable
{
    //Generate key material for the module to be concatenated with other modules.  This material can be deserialized back into objects to valdiate client scenarios
    void CreateKeyMaterialAndSerialize(ScenarioDetail a_sd, IWriter a_writer);

    //Deserializes from key material to List<objects> for comparison
    void CreateListFromReader(IReader a_reader);

    //returns a ratio of similarity to a given Sd
    bool CompareToSd(ScenarioDetail a_sd);
}

internal class ResourceDataModule : IDataModelActivationModule
{
    #region Members
    private readonly List<short> m_resourceList = new ();
    private const double c_failScore = .5;
    private const double c_dangerScore = .5;
    internal const int UNIQUE_ID = 845;
    public int UniqueId { get; }
    #endregion

    public void CreateListFromReader(IReader a_reader)
    {
        #region 732
        if (a_reader.VersionNumber >= 732)
        {
            short count;
            a_reader.Read(out count);
            for (int i = 0; i < count; i++)
            {
                a_reader.Read(out short x);
                AddResource(x);
            }
        }
        #endregion
    }

    private void AddResource(short a_nameHash)
    {
        m_resourceList.Add(a_nameHash);
    }

    public void Serialize(IWriter a_writer)
    {
        a_writer.Write((short)m_resourceList.Count);
        for (int i = 0; i < m_resourceList.Count; i++)
        {
            a_writer.Write(m_resourceList[i]);
        }
    }

    public void CreateKeyMaterialAndSerialize(ScenarioDetail a_sd, IWriter a_writer)
    {
        List<Resource> resourceList = a_sd.PlantManager.GetResourceList();

        int resToAddCount = a_sd.PlantManager.Count * 5;
        int totalResCount = resourceList.Count;
        int stepSize = Math.Max(totalResCount / resToAddCount, 1);

        for (int i = 0; i < Math.Min(5, totalResCount); i++)
        {
            int curStep = Math.Min(stepSize * i, totalResCount);
            AddResource((short)resourceList[curStep].Name.GetHashCode());
        }

        Serialize(a_writer);
    }

    public bool CompareToSd(ScenarioDetail a_sd)
    {
        List<Resource> resourceList = a_sd.PlantManager.GetResourceList();
        if (resourceList.Count == 0)
        {
            //There are no resources.  Could be a wipe and refresh or another acceptable reason.  return 0 no penalty;
            return true;
        }

        int resToFindCount = a_sd.PlantManager.Count * 5;
        int foundResources = 0;
        foreach (short resHash in m_resourceList)
        {
            if (resourceList.FirstOrDefault(x => (short)x.Name.GetHashCode() == resHash) != null)
            {
                //There is a match
                foundResources++;
            }
        }


        double ratioFound = (double)foundResources / resToFindCount;
        if (ratioFound < c_failScore)
        {
            throw new Exception("Scenario fails to validate against key");
        }

        return ratioFound > c_dangerScore;
    }
}

internal class ItemDataModule : IDataModelActivationModule
{
    #region Members
    private readonly List<string> m_itemList = new ();
    private const double c_failScore = .5;
    private const double c_dangerScore = .5;
    internal const int UNIQUE_ID = 846;
    public int UniqueId { get; }
    #endregion

    public void CreateListFromReader(IReader a_reader)
    {
        #region 732
        if (a_reader.VersionNumber >= 732)
        {
            short count;
            a_reader.Read(out count);
            for (int i = 0; i < count; i++)
            {
                a_reader.Read(out string x);
                AddItem(x);
            }
        }
        #endregion
    }

    private void AddItem(string a_name)
    {
        m_itemList.Add(a_name);
    }

    public void Serialize(IWriter a_writer)
    {
        a_writer.Write((short)m_itemList.Count);
        for (int i = 0; i < m_itemList.Count; i++)
        {
            a_writer.Write(m_itemList[i]);
        }
    }

    public void CreateKeyMaterialAndSerialize(ScenarioDetail a_sd, IWriter a_writer)
    {
        int count = a_sd.ItemManager.Count;
        int stepSize = Math.Max(count / 5, 1);
        for (int i = 0; i < Math.Min(5, count); i++)
        {
            int step = Math.Min(i * stepSize, count);
            AddItem(a_sd.ItemManager[step].ExternalId);
        }

        Serialize(a_writer);
    }

    public bool CompareToSd(ScenarioDetail a_sd)
    {
        if (a_sd.ItemManager.Count == 0)
        {
            return true;
        }

        int foundItems = 0;
        a_sd.ItemManager.InitFastLookupByExternalId();
        foreach (string itemExternalId in m_itemList)
        {
            if (a_sd.ItemManager.ContainsExternalId(itemExternalId))
            {
                //There is a match
                foundItems++;
            }
        }

        a_sd.ItemManager.DeInitFastLookupByExternalId();
        double ratioFound = (double)foundItems / m_itemList.Count;
        if (ratioFound < c_failScore)
        {
            throw new Exception("Scenario fails to validate against key");
        }

        return ratioFound > c_dangerScore;
    }
}

internal class OperationAttributeModule : IDataModelActivationModule
{
    #region Members
    private readonly List<short> m_oAList = new ();
    private const double c_failScore = .5;
    private const double c_dangerScore = .5;
    internal const int UNIQUE_ID = 847;
    public int UniqueId { get; }
    #endregion

    public void CreateListFromReader(IReader a_reader)
    {
        #region 732
        if (a_reader.VersionNumber >= 732)
        {
            short count;
            a_reader.Read(out count);
            for (int i = 0; i < count; i++)
            {
                a_reader.Read(out short x);
                AddOA(x);
            }
        }
        #endregion
    }

    private void AddOA(short a_name)
    {
        m_oAList.Add(a_name);
    }

    public void Serialize(IWriter a_writer)
    {
        a_writer.Write((short)m_oAList.Count);
        for (int i = 0; i < m_oAList.Count; i++)
        {
            a_writer.Write(m_oAList[i]);
        }
    }

    public List<string> CreateOAList(ScenarioDetail a_sd)
    {
        List<string> oaList = new ();
        foreach (Job job in a_sd.JobManager)
        {
            foreach (InternalActivity act in job.GetActivities())
            {
                foreach (OperationAttribute attribute in act.Operation.Attributes)
                {
                    if (!string.IsNullOrEmpty(attribute.PTAttribute.Name) && !oaList.Contains(attribute.PTAttribute.Name))
                    {
                        oaList.Add(attribute.PTAttribute.Name);
                    }
                }
            }
        }

        return oaList;
    }

    public void CreateKeyMaterialAndSerialize(ScenarioDetail a_sd, IWriter a_writer)
    {
        List<string> oaList = CreateOAList(a_sd);
        int stepSize = Math.Max(oaList.Count / 5, 1);
        for (int i = 0; i < Math.Min(5, oaList.Count); i++)
        {
            int curStep = Math.Min(oaList.Count, i * stepSize);
            m_oAList.Add((short)oaList[curStep].GetHashCode());
        }

        Serialize(a_writer);
    }

    public bool CompareToSd(ScenarioDetail a_sd)
    {
        List<string> oaList = CreateOAList(a_sd);
        if (oaList.Count == 0)
        {
            //There are no MaterialAttributes.  Could be a wipe and refresh or another acceptable reason.  return 0. No penalty;
            return true;
        }

        int foundOAs = 0;
        foreach (short resHash in m_oAList)
        {
            if (oaList.FirstOrDefault(x => (short)x.GetHashCode() == resHash) != null)
            {
                //There is a match
                foundOAs++;
            }
        }

        double ratioFound = (double)foundOAs / m_oAList.Count;
        if (ratioFound < c_failScore)
        {
            throw new Exception("Scenario fails to validate against key");
        }

        return ratioFound > c_dangerScore;
    }
}

internal class SalesOrderModule : IDataModelActivationModule
{
    #region Members
    private readonly List<string> m_sOList = new ();
    private const double c_failScore = .8;
    private const double c_dangerScore = .9;
    internal const int UNIQUE_ID = 848;
    public int UniqueId { get; }
    #endregion

    public void CreateListFromReader(IReader a_reader)
    {
        #region 732
        if (a_reader.VersionNumber >= 732)
        {
            short count;
            a_reader.Read(out count);
            for (int i = 0; i < count; i++)
            {
                a_reader.Read(out string x);
                AddSO(x);
            }
        }
        #endregion
    }

    private void AddSO(string a_externalId)
    {
        m_sOList.Add(a_externalId);
    }

    public void Serialize(IWriter a_writer)
    {
        a_writer.Write((short)m_sOList.Count);
        for (int i = 0; i < m_sOList.Count; i++)
        {
            a_writer.Write(m_sOList[i]);
        }
    }

    public void CreateKeyMaterialAndSerialize(ScenarioDetail a_sd, IWriter a_writer)
    {
        int count = a_sd.SalesOrderManager.Count;
        int stepSize = Math.Max(count / 5, 1);

        for (int i = 0; i < Math.Min(5, count); i++)
        {
            int curStep = Math.Min(i * stepSize, count);
            m_sOList.Add(a_sd.SalesOrderManager[curStep].ExternalId);
        }

        Serialize(a_writer);
    }

    public bool CompareToSd(ScenarioDetail a_sd)
    {
        if (a_sd.SalesOrderManager.Count == 0)
        {
            return true;
        }

        int foundSos = 0;
        a_sd.SalesOrderManager.InitFastLookupByExternalId();
        foreach (string so in m_sOList)
        {
            if (a_sd.SalesOrderManager.ContainsExternalId(so))
            {
                //There is a match
                foundSos++;
            }
        }

        a_sd.SalesOrderManager.DeInitFastLookupByExternalId();

        double ratioFound = (double)foundSos / m_sOList.Count;
        if (ratioFound < c_failScore)
        {
            throw new Exception("Scenario fails to validate against key");
        }

        return ratioFound > c_dangerScore;
    }
}

internal class PurchaseOrderModule : IDataModelActivationModule
{
    #region Members
    private readonly List<string> m_pOList = new ();
    private const double c_failScore = .5;
    private const double c_dangerScore = .5;
    internal const int UNIQUE_ID = 849;
    public int UniqueId { get; }
    #endregion

    public void CreateListFromReader(IReader a_reader)
    {
        #region 732
        if (a_reader.VersionNumber >= 732)
        {
            short count;
            a_reader.Read(out count);
            for (int i = 0; i < count; i++)
            {
                a_reader.Read(out string x);
                AddPO(x);
            }
        }
        #endregion
    }

    private void AddPO(string a_externalId)
    {
        m_pOList.Add(a_externalId);
    }

    public void Serialize(IWriter a_writer)
    {
        a_writer.Write((short)m_pOList.Count);
        for (int i = 0; i < m_pOList.Count; i++)
        {
            a_writer.Write(m_pOList[i]);
        }
    }

    public void CreateKeyMaterialAndSerialize(ScenarioDetail a_sd, IWriter a_writer)
    {
        int count = a_sd.PurchaseToStockManager.Count;
        int stepSize = Math.Max(count / 5, 1);

        for (int i = 0; i < Math.Min(5, count); i++)
        {
            int curStep = Math.Min(i * stepSize, count);
            m_pOList.Add(a_sd.PurchaseToStockManager[curStep].ExternalId);
        }

        Serialize(a_writer);
    }

    public bool CompareToSd(ScenarioDetail a_sd)
    {
        if (a_sd.PurchaseToStockManager.Count == 0)
        {
            return true;
        }

        int foundPos = 0;
        a_sd.PurchaseToStockManager.InitFastLookupByExternalId();
        foreach (string so in m_pOList)
        {
            if (a_sd.PurchaseToStockManager.ContainsExternalId(so))
            {
                //There is a match
                foundPos++;
            }
        }

        a_sd.PurchaseToStockManager.DeInitFastLookupByExternalId();

        double ratioFound = (double)foundPos / m_pOList.Count;
        if (ratioFound < c_failScore)
        {
            throw new Exception("Scenario fails to validate against key");
        }

        return ratioFound > c_dangerScore;
    }
}

internal class WarehouseModule : IDataModelActivationModule
{
    #region Members
    private readonly List<short> m_warehouseList = new ();
    private const double c_failScore = .99;
    private const double c_dangerScore = .9;
    internal const int UNIQUE_ID = 849;
    public int UniqueId { get; }
    #endregion

    public void CreateListFromReader(IReader a_reader)
    {
        #region 732
        if (a_reader.VersionNumber >= 732)
        {
            short count;
            a_reader.Read(out count);
            for (int i = 0; i < count; i++)
            {
                a_reader.Read(out short x);
                AddWarehouse(x);
            }
        }
        #endregion
    }

    private void AddWarehouse(short a_nameHash)
    {
        m_warehouseList.Add(a_nameHash);
    }

    public void Serialize(IWriter a_writer)
    {
        a_writer.Write((short)m_warehouseList.Count);
        for (int i = 0; i < m_warehouseList.Count; i++)
        {
            a_writer.Write(m_warehouseList[i]);
        }
    }

    public void CreateKeyMaterialAndSerialize(ScenarioDetail a_sd, IWriter a_writer)
    {
        int count = a_sd.WarehouseManager.Count;
        int stepSize = Math.Max(count / 5, 1);

        for (int i = 0; i < Math.Min(5, count); i++)
        {
            int curStep = Math.Min(i * stepSize, count);
            m_warehouseList.Add((short)a_sd.WarehouseManager[curStep].ExternalId.GetHashCode());
        }

        Serialize(a_writer);
    }

    public bool CompareToSd(ScenarioDetail a_sd)
    {
        if (a_sd.WarehouseManager.Count == 0)
        {
            return true;
        }

        int foundWhs = 0;
        foreach (short whHash in m_warehouseList)
        {
            if (a_sd.WarehouseManager.FirstOrDefault(x => (short)x.ExternalId.GetHashCode() == whHash) != null)
            {
                //There is a match
                foundWhs++;
            }
        }


        double ratioFound = (double)foundWhs / m_warehouseList.Count;
        if (ratioFound < c_failScore)
        {
            throw new Exception("Scenario fails to validate against key");
        }

        return ratioFound > c_dangerScore;
    }
}

internal class CapabilityModule : IDataModelActivationModule
{
    #region Members
    private readonly List<short> m_capabilityList = new ();
    private const double c_failScore = .5;
    private const double c_dangerScore = .5;
    internal const int UNIQUE_ID = 850;
    public int UniqueId => UNIQUE_ID;
    #endregion

    public void CreateListFromReader(IReader a_reader)
    {
        #region 732
        if (a_reader.VersionNumber >= 732)
        {
            short count;
            a_reader.Read(out count);
            for (int i = 0; i < count; i++)
            {
                a_reader.Read(out short x);
                AddCapability(x);
            }
        }
        #endregion
    }

    private void AddCapability(short a_nameHash)
    {
        m_capabilityList.Add(a_nameHash);
    }

    public void Serialize(IWriter a_writer)
    {
        a_writer.Write((short)m_capabilityList.Count);
        for (int i = 0; i < m_capabilityList.Count; i++)
        {
            a_writer.Write(m_capabilityList[i]);
        }
    }

    public void CreateKeyMaterialAndSerialize(ScenarioDetail a_sd, IWriter a_writer)
    {
        int count = a_sd.CapabilityManager.Count;
        int stepSize = Math.Max(count / 5, 1);

        for (int i = 0; i < Math.Min(5, count); i++)
        {
            int curStep = Math.Min(i * stepSize, count);
            m_capabilityList.Add((short)a_sd.CapabilityManager[curStep].Name.GetHashCode());
        }

        Serialize(a_writer);
    }

    public bool CompareToSd(ScenarioDetail a_sd)
    {
        if (a_sd.CapabilityManager.Count == 0)
        {
            return true;
        }

        int foundCaps = 0;
        foreach (short capHash in m_capabilityList)
        {
            if (a_sd.CapabilityManager.FirstOrDefault(x => (short)x.Name.GetHashCode() == capHash) != null)
            {
                //There is a match
                foundCaps++;
            }
        }


        double ratioFound = (double)foundCaps / m_capabilityList.Count;
        if (ratioFound < c_failScore)
        {
            throw new Exception("Scenario fails to validate against key");
        }

        return ratioFound > c_dangerScore;
    }
}

internal class CustomerModule : IDataModelActivationModule
{
    #region Members
    private readonly List<short> customerList = new ();
    private const double c_failScore = .5;
    private const double c_dangerScore = .5;
    internal const int UNIQUE_ID = 851;
    public int UniqueId { get; }
    #endregion

    public void CreateListFromReader(IReader a_reader)
    {
        #region 732
        if (a_reader.VersionNumber >= 732)
        {
            short count;
            a_reader.Read(out count);
            for (int i = 0; i < count; i++)
            {
                a_reader.Read(out short x);
                AddCustomer(x);
            }
        }
        #endregion
    }

    private void AddCustomer(short a_nameHash)
    {
        customerList.Add(a_nameHash);
    }

    public void Serialize(IWriter a_writer)
    {
        a_writer.Write((short)customerList.Count);
        for (int i = 0; i < customerList.Count; i++)
        {
            a_writer.Write(customerList[i]);
        }
    }

    public List<string> CreateCustomerList(ScenarioDetail a_sd)
    {
        List<string> customerNames = new ();
        foreach (Job job in a_sd.JobManager)
        {
            foreach (Customer cust in job.Customers)
            {
                customerNames.AddIfNew(cust.ExternalId);
            }
        }

        return customerNames;
    }

    public void CreateKeyMaterialAndSerialize(ScenarioDetail a_sd, IWriter a_writer)
    {
        List<string> customerNames = CreateCustomerList(a_sd);
        int count = customerNames.Count;
        int stepSize = Math.Max(count / 5, 1);

        for (int i = 0; i < Math.Min(5, count); i++)
        {
            int curStep = Math.Min(i * stepSize, count);
            customerList.Add((short)customerNames[curStep].GetHashCode());
        }

        Serialize(a_writer);
    }

    public bool CompareToSd(ScenarioDetail a_sd)
    {
        List<string> customerNames = CreateCustomerList(a_sd);
        if (customerNames.Count == 0)
        {
            return true;
        }

        int foundCustomers = 0;
        foreach (short custHash in customerList)
        {
            if (customerNames.FirstOrDefault(x => (short)x.GetHashCode() == custHash) != null)
            {
                //There is a match
                foundCustomers++;
            }
        }


        double ratioFound = (double)foundCustomers / customerList.Count;
        if (ratioFound < c_failScore)
        {
            throw new Exception("Scenario fails to validate against key");
        }

        return ratioFound > c_dangerScore;
    }
}

internal class PlantModule : IDataModelActivationModule
{
    #region Members
    private readonly List<short> plantList = new ();
    private const double c_failScore = .99;
    private const double c_dangerScore = .9;
    internal const int UNIQUE_ID = 852;
    public int UniqueId { get; }
    #endregion

    public void CreateListFromReader(IReader a_reader)
    {
        #region 732
        if (a_reader.VersionNumber >= 732)
        {
            short count;
            a_reader.Read(out count);
            for (int i = 0; i < count; i++)
            {
                a_reader.Read(out short x);
                AddPlant(x);
            }
        }
        #endregion
    }

    private void AddPlant(short a_nameHash)
    {
        plantList.Add(a_nameHash);
    }

    public void Serialize(IWriter a_writer)
    {
        a_writer.Write((short)plantList.Count);
        for (int i = 0; i < plantList.Count; i++)
        {
            a_writer.Write(plantList[i]);
        }
    }

    public void CreateKeyMaterialAndSerialize(ScenarioDetail a_sd, IWriter a_writer)
    {
        int count = a_sd.PlantManager.Count;
        int stepSize = Math.Max(count / 5, 1);

        for (int i = 0; i < Math.Min(5, count); i++)
        {
            int curStep = Math.Min(i * stepSize, count);
            plantList.Add((short)a_sd.PlantManager[curStep].Name.GetHashCode());
        }

        Serialize(a_writer);
    }

    public bool CompareToSd(ScenarioDetail a_sd)
    {
        if (a_sd.PlantManager.Count == 0)
        {
            return true;
        }

        int foundPlants = 0;
        foreach (short plantHash in plantList)
        {
            if (a_sd.PlantManager.FirstOrDefault(x => (short)x.Name.GetHashCode() == plantHash) != null)
            {
                //There is a match
                foundPlants++;
            }
        }


        double ratioFound = (double)foundPlants / plantList.Count;
        if (ratioFound < c_failScore)
        {
            throw new Exception("Scenario fails to validate against key");
        }

        return ratioFound > c_dangerScore;
    }
}

internal class DepartmentModule : IDataModelActivationModule
{
    #region Members
    private readonly List<short> departmentList = new ();
    private const double c_failScore = .5;
    private const double c_dangerScore = .5;
    internal const int UNIQUE_ID = 853;
    public int UniqueId { get; }
    #endregion

    public void CreateListFromReader(IReader a_reader)
    {
        #region 732
        if (a_reader.VersionNumber >= 732)
        {
            short count;
            a_reader.Read(out count);
            for (int i = 0; i < count; i++)
            {
                a_reader.Read(out short x);
                AddDepartment(x);
            }
        }
        #endregion
    }

    private void AddDepartment(short a_nameHash)
    {
        departmentList.Add(a_nameHash);
    }

    public void Serialize(IWriter a_writer)
    {
        a_writer.Write((short)departmentList.Count);
        for (int i = 0; i < departmentList.Count; i++)
        {
            a_writer.Write(departmentList[i]);
        }
    }

    public void CreateKeyMaterialAndSerialize(ScenarioDetail a_sd, IWriter a_writer)
    {
        List<Department> departments = a_sd.PlantManager.GetDepartments();

        int stepSize = departments.Count / 5;

        for (int i = 0; i < Math.Min(5, departments.Count); i++)
        {
            int curStep = Math.Min(i * stepSize, departments.Count);
            departmentList.Add((short)departments[curStep].Name.GetHashCode());
        }


        Serialize(a_writer);
    }

    public bool CompareToSd(ScenarioDetail a_sd)
    {
        List<Department> departments = a_sd.PlantManager.GetDepartments();
        if (departments.Count == 0)
        {
            return true;
        }

        int foundPlants = 0;
        foreach (short deptHash in departmentList)
        {
            if (departments.FirstOrDefault(x => (short)x.Name.GetHashCode() == deptHash) != null)
            {
                //There is a match
                foundPlants++;
            }
        }

        double ratioFound = (double)foundPlants / departmentList.Count;
        if (ratioFound < c_failScore)
        {
            throw new Exception("Scenario fails to validate against key");
        }

        return ratioFound > c_dangerScore;
    }
}

internal class FeaturesAndBoolsModule : IDataModelActivationModule
{
    #region Members
    private BoolVector32 bools;
    private const short c_alternatePathsEnabled = 0;
    private const short c_compatibilityCodeEnabled = 1;
    private const short c_lotControlEnabled = 2;
    private const short c_tanksEnabled = 3;
    private const double c_failScore = .5;
    private const double c_dangerScore = .5;
    internal const int UNIQUE_ID = 854;
    public int UniqueId { get; }
    #endregion

    public void CreateListFromReader(IReader a_reader)
    {
        #region 732
        if (a_reader.VersionNumber >= 732)
        {
            bools = new BoolVector32(a_reader);
        }
        #endregion
    }

    public void Serialize(IWriter a_writer)
    {
        bools.Serialize(a_writer);
    }

    public BoolVector32 CreateFeatureList(ScenarioDetail a_sd)
    {
        BoolVector32 features = new ();
        features[c_alternatePathsEnabled] = false;
        features[c_compatibilityCodeEnabled] = false;
        features[c_lotControlEnabled] = false;
        features[c_tanksEnabled] = false;

        foreach (Job job in a_sd.JobManager)
        {
            foreach (ManufacturingOrder jMo in job.ManufacturingOrders)
            {
                if (jMo.AlternatePaths.Count > 1)
                {
                    features[c_alternatePathsEnabled] = true;
                }
            }

            foreach (InternalActivity act in job.GetActivities())
            {
                if (!string.IsNullOrEmpty(act.Operation.CompatibilityCode))
                {
                    features[c_compatibilityCodeEnabled] = true;
                }
            }
        }

        features[c_lotControlEnabled] = true;

        if (a_sd.PlantManager.GetResourceList().FirstOrDefault(x => x.IsTank) != null)
        {
            features[c_tanksEnabled] = true;
        }

        return features;
    }

    public void CreateKeyMaterialAndSerialize(ScenarioDetail a_sd, IWriter a_writer)
    {
        bools = CreateFeatureList(a_sd);

        Serialize(a_writer);
    }

    public bool CompareToSd(ScenarioDetail a_sd)
    {
        int featureMatch = 0;
        BoolVector32 features = CreateFeatureList(a_sd);
        if (bools[c_alternatePathsEnabled] == features[c_alternatePathsEnabled])
        {
            featureMatch++;
        }

        if (bools[c_compatibilityCodeEnabled] == features[c_compatibilityCodeEnabled])
        {
            featureMatch++;
        }

        if (bools[c_lotControlEnabled] == features[c_lotControlEnabled])
        {
            featureMatch++;
        }

        if (bools[c_tanksEnabled] == features[c_tanksEnabled])
        {
            featureMatch++;
        }

        double ratioFound = (double)featureMatch / 4;
        if (ratioFound < c_failScore)
        {
            throw new Exception("Scenario fails to validate against key");
        }

        return ratioFound > c_dangerScore;
    }
}

internal class CountsModules : IDataModelActivationModule
{
    #region Members
    private int[] m_countsList;
    private const short c_cellsEnabled = 0;
    private const short c_productRules = 1;
    private const short c_items = 2;
    private const short c_resources = 3;
    private const short c_templates = 4;
    private const short c_plants = 5; //100% or fail
    private const short c_departments = 6;
    private const short c_capabilities = 7;
    private const short c_warehouses = 8;

    private const double c_failScore = .75;
    private const double c_dangerScore = .9;
    internal const int UNIQUE_ID = 855;
    public int UniqueId { get; }
    #endregion

    public void CreateListFromReader(IReader a_reader)
    {
        #region 732
        if (a_reader.VersionNumber >= 732)
        {
            short count;
            a_reader.Read(out count);
            m_countsList = new int[count];
            for (int i = 0; i < count; i++)
            {
                a_reader.Read(out m_countsList[i]);
            }
        }
        #endregion
    }

    public void Serialize(IWriter a_writer)
    {
        a_writer.Write((short)m_countsList.Length);
        for (int i = 0; i < m_countsList.Length; i++)
        {
            a_writer.Write(m_countsList[i]);
        }
    }

    public int[] CreateCountList(ScenarioDetail a_sd)
    {
        int[] counts = new int[9];

        counts[c_cellsEnabled] = (short)a_sd.CellManager.Count;
        counts[c_productRules] = 0; //We don't need to support this, plus this should be removed in V12

        counts[c_items] = a_sd.ItemManager.Count;
        counts[c_resources] = a_sd.PlantManager.GetResourceList().Count;
        counts[c_templates] = a_sd.JobManager.TemplatesCount;
        counts[c_plants] = a_sd.PlantManager.Count;
        counts[c_departments] = a_sd.PlantManager.GetDepartments().Count;
        counts[c_capabilities] = a_sd.CapabilityManager.Count;
        counts[c_warehouses] = a_sd.WarehouseManager.Count;
        return counts;
    }

    public void CreateKeyMaterialAndSerialize(ScenarioDetail a_sd, IWriter a_writer)
    {
        m_countsList = CreateCountList(a_sd);

        Serialize(a_writer);
    }

    public bool CompareToSd(ScenarioDetail a_sd)
    {
        bool inDanger = false;
        int[] counts = CreateCountList(a_sd);

        //Plants must match exactly
        if (counts[c_plants] != 0 && counts[c_plants] != m_countsList[c_plants])
        {
            throw new Exception("Scenario fails to validate against key");
        }

        for (int i = 0; i < m_countsList.Length; i++)
        {
            if (counts[i] == 0 || m_countsList[i] == 0)
            {
                continue;
            }

            double similarity = Math.Abs((double)counts[i] / m_countsList[i]);
            if (similarity < c_failScore)
            {
                throw new Exception("Scenario fails to validate against key");
            }

            if (similarity < c_dangerScore)
            {
                inDanger = true;
            }
        }

        return !inDanger;
    }
}
#endregion