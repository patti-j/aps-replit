using System.Data;
using System.Text;

using PT.APSCommon;

namespace PT.Scheduler;

/// <summary>
/// A DataSet that contains schedule data for the specified object.
/// This is used in the internal schedule reports and the xml schedule export.
/// </summary>
public class ScheduleDataSet : DataSet
{
    #region Column Names
    private const string COL_PLANT_ID = "PlantId";
    private const string COL_PLANT_NAME = "Plant";
    private const string COL_DEPARTMENT_ID = "DepartmentId";
    private const string COL_DEPARTMENT_NAME = "Department";
    private const string COL_MACHINE_ID = "ResourceId";
    private const string COL_BLOCK_ID = "BlockId";

    private const string COL_DAY = "Day";
    private const string COL_SHIFT_START = "Shift Start";
    private const string COL_SHIFT_END = "Shift End";
    private const string COL_RUN_NBR = "Run#";
    private const string COL_START_DATE_ONLY = "Start Day";
    private const string COL_WEEK_NBR = "Week#";
    private const string COL_SEQUENCE = "Sequence";
    private const string COL_MACHINE_NAME = "Resource";
    private const string COL_MACHINE_DESC = "Description";
    private const string COL_MACHINE_WORKCENTER = "Workcenter";
    private const string COL_MACHINE_RESOURCE_TYPE = "Resource Type";
    private const string COL_ACTIVITY_START = "Start DateTime";
    private const string COL_ACTIVITY_END = "End DateTime";
    private const string COL_PROD_STATUS = "Status";
    private const string COL_PAUSED = "Paused";
    private const string COL_DESCRIPTION = "Description";
    private const string COL_SETUP_HOURS = "Setup Hrs";
    private const string COL_RUN_HOURS = "Run Hrs";
    private const string COL_ACTIVITY_REQ_QTY = "Required Qty";
    private const string COL_NBR_OF_PEOPLE = "Nbr Of People";
    private const string COL_OPERATION_SETUP_NBR = "Setup Nbr";
    private const string COL_OPERATION_SETUP_COLOR = "Setup Color";
    private const string COL_JOB_NAME = "Job";
    private const string COL_JOB_DESC = "Job Desc";
    private const string COL_MO_NAME = "MO";
    private const string COL_OP_NAME = "Op";
    private const string COL_MO_PART = "Product";
    private const string COL_CUSTOMER = "Customer";
    private const string COL_PRIORITY = "Priority";
    private const string COL_SLACKDAYS = "Slack days";
    private const string COL_JOB_NEED_DATE = "Job NeedDate";
    private const string COL_OP_NEED_DATE = "Op NeedDate";
    private const string COL_HOLD = "OnHold";
    private const string COL_HOLD_REASON = "Hold Reason";
    private const string COL_HOLD_UNTIL = "Hold Until";
    private const string COL_ANCHORED = "Anchored";
    private const string COL_LOCKED = "Locked";
    private const string COL_CYCLES = "Cycles";
    private const string COL_QTY_PER_CYCLE = "Cycle Qty";
    private const string COL_COMMENTS = "Comments";
    private const string COL_COMMENTS2 = "Comments 2";

    private const string COL_MATERIAL_NAME = "Material";
    private const string COL_MATERIAL_DESCRIPTION = "Material Description";
    private const string COL_MATERIAL_TOTAL_QTY = "Total Req'd Qty";
    private const string COL_MATERIAL_UOM = "Uom";
    private const string c_materialLatestSourceDate = "LatestSourceDate";
    private const string COL_ONHAND_QTY = "OnHand Qty";
    private const string COL_MATERIAL_ISSUED_COMPLETE = "Issued Complete";
    private const string COL_MATERIAL_ISSUED_QTY = "Issued Qty";
    private const string COL_MATERIAL_SOURCE = "Source";
    private const string COL_MATERIAL_EXTERNAL_ID = "ExternalId";
    private const string COL_MATERIAL_CONSTRAINT_TYPE = "Constraint Type";
    private const string COL_MATERIAL_BUY_DIRECT = "Buy Direct";
    private const string COL_MATERIAL_WAREHOUSE = "Warehouse";
    private const string COL_MATERIAL_SUPPLY = "Supply";
    #endregion

    /// <summary>
    /// Schedule data for all plants.
    /// </summary>
    /// <param name="sd"></param>
    /// <param name="maxScheduledStart">The maximum start date of Activities to show (in server time).</param>
    public ScheduleDataSet(ScenarioDetail sd, DateTime a_minScheduledStart, DateTime maxScheduledStart, bool includeMaterials)
    {
        //Add the necessary DataTable structures
        DataTable plantsTable = GetPlantTable();
        DataTable departmentsTable = GetDepartmentTable();
        DataTable machinesTable = GetMachineTable();
        DataTable blocksTable = GetBlockTable();
        DataTable materialsTable = GetMaterialsTable();

        Tables.Add(plantsTable);
        Tables.Add(departmentsTable);
        Tables.Add(machinesTable);
        Tables.Add(blocksTable);
        if (includeMaterials)
        {
            Tables.Add(materialsTable);
        }

        //Add the Table Relations
        AddPlantDepartmentRelation(this, plantsTable, departmentsTable);
        AddDepartmentMachineRelation(this, departmentsTable, machinesTable);
        AddMachineBlockRelation(this, machinesTable, blocksTable);
        if (includeMaterials)
        {
            AddBlockMaterialRelation(this, blocksTable, materialsTable);
        }

        //Populate the tables
        AddPlants(plantsTable, departmentsTable, machinesTable, blocksTable, materialsTable, sd, a_minScheduledStart, maxScheduledStart, includeMaterials);
    }

    /// <summary>
    /// Schedule data for one plant.
    /// </summary>
    /// <param name="sd"></param>
    /// <param name="maxScheduledStart">The maximum start date of Activities to show (in local time).</param>
    public ScheduleDataSet(Plant plant, DateTime a_minScheduledStart, DateTime maxScheduledStart, bool includeMaterials, ScenarioDetail sd)
    {
        //Add the necessary DataTable structures
        DataTable departmentsTable = GetDepartmentTable();
        DataTable machinesTable = GetMachineTable();
        DataTable blocksTable = GetBlockTable();
        DataTable materialsTable = GetMaterialsTable();

        Tables.Add(departmentsTable);
        Tables.Add(machinesTable);
        Tables.Add(blocksTable);
        if (includeMaterials)
        {
            Tables.Add(materialsTable);
        }

        //Add the Table Relations
        AddDepartmentMachineRelation(this, departmentsTable, machinesTable);
        AddMachineBlockRelation(this, machinesTable, blocksTable);
        if (includeMaterials)
        {
            AddBlockMaterialRelation(this, blocksTable, materialsTable);
        }

        //Populate the tables
        AddDepartments(departmentsTable, machinesTable, blocksTable, materialsTable, plant, a_minScheduledStart, maxScheduledStart, includeMaterials, sd);
    }

    /// <summary>
    /// Schedule data for one department.
    /// </summary>
    /// <param name="sd"></param>
    /// <param name="maxScheduledStart">The maximum start date of Activities to show (in local time).</param>
    public ScheduleDataSet(Department department, DateTime a_minScheduledStart, DateTime maxScheduledStart, bool includeMaterials, ScenarioDetail sd)
    {
        //Add the necessary DataTable structures
        DataTable machinesTable = GetMachineTable();
        DataTable blocksTable = GetBlockTable();
        DataTable materialsTable = GetMaterialsTable();

        Tables.Add(machinesTable);
        Tables.Add(blocksTable);
        if (includeMaterials)
        {
            Tables.Add(materialsTable);
        }

        //Add the Table Relations
        AddMachineBlockRelation(this, machinesTable, blocksTable);
        if (includeMaterials)
        {
            AddBlockMaterialRelation(this, blocksTable, materialsTable);
        }

        //Populate the tables
        AddMachines(machinesTable, blocksTable, materialsTable, department, a_minScheduledStart, maxScheduledStart, includeMaterials, sd);
    }

    /// <summary>
    /// Schedule data for one resource.
    /// </summary>
    /// <param name="sd"></param>
    /// <param name="maxScheduledStart">The maximum start date of Activities to show (in local time).</param>
    public ScheduleDataSet(Resource machine, DateTime a_minScheduledStart, DateTime maxScheduledStart, bool includeMaterials, ScenarioDetail sd)
    {
        //Add the necessary DataTable structures
        DataTable blockTable = GetBlockTable();
        Tables.Add(blockTable);
        DataTable materialsTable = GetMaterialsTable();
        if (includeMaterials)
        {
            Tables.Add(materialsTable);
        }

        if (includeMaterials)
        {
            AddBlockMaterialRelation(this, blockTable, materialsTable);
        }

        //Populate the table
        AddBlocks(blockTable, materialsTable, machine, a_minScheduledStart, maxScheduledStart, includeMaterials, sd);
    }

    public ScheduleDataSet()
    {
        //Add the necessary DataTable structures
        DataTable blockTable = GetBlockTable();
        Tables.Add(blockTable);
        DataTable materialsTable = GetMaterialsTable();
        Tables.Add(materialsTable);
        AddBlockMaterialRelation(this, blockTable, materialsTable);
    }
    public void RefreshDataSet(Resource machine, DateTime a_minScheduledStart, DateTime maxScheduledStart, bool includeMaterials, ScenarioDetail sd)
    {
        AddBlocks(machine, a_minScheduledStart, maxScheduledStart, includeMaterials, sd);
    }

    private DataTable GetPlantTable()
    {
        //Create the Plants Table
        DataTable plantsTable = new ();
        plantsTable.TableName = "Plants";

        DataColumn col = new (COL_PLANT_ID);
        col.DataType = BaseId.GetIdType();
        plantsTable.Columns.Add(col);

        col = new DataColumn(COL_PLANT_NAME);
        col.DataType = typeof(string);
        plantsTable.Columns.Add(col);

        return plantsTable;
    }

    private DataTable GetDepartmentTable()
    {
        //Create the Departments Table
        DataTable departmentsTable = new ();
        departmentsTable.TableName = "Departments";

        DataColumn col = new (COL_PLANT_ID);
        col.DataType = BaseId.GetIdType();
        departmentsTable.Columns.Add(col);

        col = new DataColumn(COL_DEPARTMENT_ID);
        col.DataType = BaseId.GetIdType();
        departmentsTable.Columns.Add(col);

        col = new DataColumn(COL_DEPARTMENT_NAME);
        col.DataType = typeof(string);
        departmentsTable.Columns.Add(col);

        return departmentsTable;
    }

    private DataTable GetMachineTable()
    {
        //Creaet Resources Table
        DataTable machinesTable = new ();
        machinesTable.TableName = "Resources";

        DataColumn col = new (COL_PLANT_ID);
        col.DataType = BaseId.GetIdType();
        machinesTable.Columns.Add(col);

        col = new DataColumn(COL_DEPARTMENT_ID);
        col.DataType = BaseId.GetIdType();
        machinesTable.Columns.Add(col);

        col = new DataColumn(COL_MACHINE_ID);
        col.DataType = BaseId.GetIdType();
        machinesTable.Columns.Add(col);

        col = new DataColumn(COL_MACHINE_NAME);
        col.DataType = typeof(string);
        machinesTable.Columns.Add(col);

        col = new DataColumn(COL_MACHINE_DESC);
        col.DataType = typeof(string);
        machinesTable.Columns.Add(col);

        col = new DataColumn(COL_MACHINE_WORKCENTER);
        col.DataType = typeof(string);
        machinesTable.Columns.Add(col);

        col = new DataColumn(COL_MACHINE_RESOURCE_TYPE);
        col.DataType = typeof(string);
        machinesTable.Columns.Add(col);

        return machinesTable;
    }

    private DataTable GetBlockTable()
    {
        //Create Blocks Table
        DataTable blocksTable = new ();
        blocksTable.TableName = "Activities";

        DataColumn col = new (COL_PLANT_ID);
        col.DataType = BaseId.GetIdType();
        blocksTable.Columns.Add(col);

        col = new DataColumn(COL_DEPARTMENT_ID);
        col.DataType = BaseId.GetIdType();
        blocksTable.Columns.Add(col);

        col = new DataColumn(COL_MACHINE_ID);
        col.DataType = BaseId.GetIdType();
        blocksTable.Columns.Add(col);

        col = new DataColumn(COL_BLOCK_ID);
        col.DataType = typeof(string);
        blocksTable.Columns.Add(col);


        col = new DataColumn(COL_SEQUENCE);
        col.DataType = typeof(int);
        blocksTable.Columns.Add(col);

        col = new DataColumn(COL_WEEK_NBR);
        col.DataType = typeof(int);
        blocksTable.Columns.Add(col);

        col = new DataColumn(COL_DAY);
        col.DataType = typeof(string);
        blocksTable.Columns.Add(col);

        col = new DataColumn(COL_SHIFT_START);
        col.DataType = typeof(string);
        blocksTable.Columns.Add(col);

        col = new DataColumn(COL_SHIFT_END);
        col.DataType = typeof(string);
        blocksTable.Columns.Add(col);

        col = new DataColumn(COL_START_DATE_ONLY);
        col.DataType = typeof(DateTime);
        blocksTable.Columns.Add(col);

        col = new DataColumn(COL_RUN_NBR);
        col.DataType = typeof(int);
        blocksTable.Columns.Add(col);

        col = new DataColumn(COL_JOB_NAME);
        col.DataType = typeof(string);
        blocksTable.Columns.Add(col);

        col = new DataColumn(COL_JOB_DESC);
        col.DataType = typeof(string);
        blocksTable.Columns.Add(col);

        col = new DataColumn(COL_MO_NAME);
        col.DataType = typeof(string);
        blocksTable.Columns.Add(col);

        col = new DataColumn(COL_MO_PART);
        col.DataType = typeof(string);
        blocksTable.Columns.Add(col);

        col = new DataColumn(COL_OP_NAME);
        col.DataType = typeof(string);
        blocksTable.Columns.Add(col);

        col = new DataColumn(COL_DESCRIPTION);
        col.DataType = typeof(string);
        blocksTable.Columns.Add(col);

        col = new DataColumn(COL_ACTIVITY_START);
        col.DataType = typeof(DateTime);
        blocksTable.Columns.Add(col);

        col = new DataColumn(COL_ACTIVITY_END);
        col.DataType = typeof(DateTime);
        blocksTable.Columns.Add(col);

        col = new DataColumn(COL_PROD_STATUS);
        col.DataType = typeof(string);
        blocksTable.Columns.Add(col);

        col = new DataColumn(COL_PAUSED);
        col.DataType = typeof(bool);
        blocksTable.Columns.Add(col);

        col = new DataColumn(COL_JOB_NEED_DATE);
        col.DataType = typeof(DateTime);
        blocksTable.Columns.Add(col);

        col = new DataColumn(COL_OP_NEED_DATE);
        col.DataType = typeof(DateTime);
        blocksTable.Columns.Add(col);

        col = new DataColumn(COL_SLACKDAYS);
        col.DataType = typeof(decimal);
        blocksTable.Columns.Add(col);

        col = new DataColumn(COL_CUSTOMER);
        col.DataType = typeof(string);
        blocksTable.Columns.Add(col);

        col = new DataColumn(COL_PRIORITY);
        col.DataType = typeof(int);
        blocksTable.Columns.Add(col);

        col = new DataColumn(COL_SETUP_HOURS);
        col.DataType = typeof(double);
        blocksTable.Columns.Add(col);

        col = new DataColumn(COL_RUN_HOURS);
        col.DataType = typeof(double);
        blocksTable.Columns.Add(col);

        col = new DataColumn(COL_ACTIVITY_REQ_QTY);
        col.DataType = typeof(double);
        blocksTable.Columns.Add(col);

        col = new DataColumn(COL_NBR_OF_PEOPLE);
        col.DataType = typeof(double);
        blocksTable.Columns.Add(col);

        col = new DataColumn(COL_OPERATION_SETUP_NBR);
        col.DataType = typeof(decimal);
        blocksTable.Columns.Add(col);

        col = new DataColumn(COL_OPERATION_SETUP_COLOR);
        col.DataType = typeof(System.Drawing.Color);
        blocksTable.Columns.Add(col);

        col = new DataColumn(COL_HOLD);
        col.DataType = typeof(string);
        blocksTable.Columns.Add(col);

        col = new DataColumn(COL_HOLD_REASON);
        col.DataType = typeof(string);
        blocksTable.Columns.Add(col);

        col = new DataColumn(COL_HOLD_UNTIL);
        col.DataType = typeof(DateTime);
        blocksTable.Columns.Add(col);

        col = new DataColumn(COL_ANCHORED);
        col.DataType = typeof(bool);
        blocksTable.Columns.Add(col);

        col = new DataColumn(COL_LOCKED);
        col.DataType = typeof(bool);
        blocksTable.Columns.Add(col);

        col = new DataColumn(COL_CYCLES);
        col.DataType = typeof(decimal);
        blocksTable.Columns.Add(col);

        col = new DataColumn(COL_QTY_PER_CYCLE);
        col.DataType = typeof(decimal);
        blocksTable.Columns.Add(col);

        col = new DataColumn(COL_COMMENTS);
        col.DataType = typeof(string);
        blocksTable.Columns.Add(col);

        return blocksTable;
    }

    private DataTable GetMaterialsTable()
    {
        //Create the Plants Table
        DataTable materialsTable = new ();
        materialsTable.TableName = "Materials";

        DataColumn col = new (COL_PLANT_ID);
        col.DataType = BaseId.GetIdType();
        materialsTable.Columns.Add(col);

        col = new DataColumn(COL_DEPARTMENT_ID);
        col.DataType = BaseId.GetIdType();
        materialsTable.Columns.Add(col);

        col = new DataColumn(COL_MACHINE_ID);
        col.DataType = BaseId.GetIdType();
        materialsTable.Columns.Add(col);

        col = new DataColumn(COL_BLOCK_ID);
        col.DataType = typeof(string);
        materialsTable.Columns.Add(col);

        col = new DataColumn(COL_MATERIAL_NAME);
        col.DataType = typeof(string);
        materialsTable.Columns.Add(col);

        col = new DataColumn(COL_MATERIAL_DESCRIPTION);
        col.DataType = typeof(string);
        materialsTable.Columns.Add(col);

        col = new DataColumn(COL_MATERIAL_TOTAL_QTY);
        col.DataType = typeof(decimal);
        materialsTable.Columns.Add(col);

        col = new DataColumn(COL_MATERIAL_UOM);
        col.DataType = typeof(string);
        materialsTable.Columns.Add(col);

        col = new DataColumn(c_materialLatestSourceDate);
        col.DataType = typeof(DateTime);
        materialsTable.Columns.Add(col);

        col = new DataColumn(COL_ONHAND_QTY);
        col.DataType = typeof(decimal);
        materialsTable.Columns.Add(col);

        col = new DataColumn(COL_MATERIAL_ISSUED_COMPLETE);
        col.DataType = typeof(bool);
        materialsTable.Columns.Add(col);

        col = new DataColumn(COL_MATERIAL_ISSUED_QTY);
        col.DataType = typeof(decimal);
        materialsTable.Columns.Add(col);

        col = new DataColumn(COL_MATERIAL_SOURCE);
        col.DataType = typeof(string);
        materialsTable.Columns.Add(col);

        col = new DataColumn(COL_MATERIAL_EXTERNAL_ID);
        col.DataType = typeof(string);
        materialsTable.Columns.Add(col);

        col = new DataColumn(COL_MATERIAL_CONSTRAINT_TYPE);
        col.DataType = typeof(string);
        materialsTable.Columns.Add(col);

        col = new DataColumn(COL_MATERIAL_BUY_DIRECT);
        col.DataType = typeof(bool);
        materialsTable.Columns.Add(col);

        col = new DataColumn(COL_MATERIAL_WAREHOUSE);
        col.DataType = typeof(string);
        materialsTable.Columns.Add(col);

        col = new DataColumn(COL_MATERIAL_SUPPLY);
        col.DataType = typeof(string);
        materialsTable.Columns.Add(col);


        return materialsTable;
    }

    private DataTable AddPlants(DataTable plantsTable, DataTable departmentsTable, DataTable machinesTable, DataTable blocksTable, DataTable materialsTable, ScenarioDetail sd, DateTime a_minScheduledStart, DateTime maxScheduledStart, bool includeMaterials)
    {
        for (int i = 0; i < sd.PlantManager.Count; i++)
        {
            Plant plant = sd.PlantManager[i];
            DataRow plantRow = plantsTable.NewRow();
            plantRow[COL_PLANT_ID] = plant.Id.ToBaseType();
            plantRow[COL_PLANT_NAME] = plant.Name;
            plantsTable.Rows.Add(plantRow);

            AddDepartments(departmentsTable, machinesTable, blocksTable, materialsTable, plant, a_minScheduledStart, maxScheduledStart, includeMaterials, sd);
        }

        return plantsTable;
    }

    private DataTable AddDepartments(DataTable departmentsTable, DataTable machinesTable, DataTable blocksTable, DataTable materialsTable, Plant plant, DateTime a_minScheduledStart, DateTime maxScheduledStart, bool includeMaterials, ScenarioDetail sd)
    {
        //Fill it
        for (int i = 0; i < plant.Departments.Count; i++)
        {
            Department dept = plant.Departments.GetByIndex(i);
            DataRow wcRow = departmentsTable.NewRow();
            wcRow[COL_PLANT_ID] = dept.Plant.Id.ToBaseType();
            wcRow[COL_DEPARTMENT_ID] = dept.Id.ToBaseType();
            wcRow[COL_DEPARTMENT_NAME] = dept.Name;
            departmentsTable.Rows.Add(wcRow);

            AddMachines(machinesTable, blocksTable, materialsTable, dept, a_minScheduledStart, maxScheduledStart, includeMaterials, sd);
        }

        return departmentsTable;
    }

    private DataTable AddMachines(DataTable machinesTable, DataTable blocksTable, DataTable materialsTable, Department dept, DateTime a_minScheduledStart, DateTime maxScheduledStart, bool includeMaterials, ScenarioDetail sd)
    {
        for (int j = 0; j < dept.Resources.Count; j++)
        {
            Resource m = dept.Resources.GetByIndex(j);
            DataRow mRow = machinesTable.NewRow();
            mRow[COL_PLANT_ID] = dept.Plant.Id.ToBaseType();
            mRow[COL_DEPARTMENT_ID] = dept.Id.ToBaseType();
            mRow[COL_MACHINE_ID] = m.Id.ToBaseType();
            mRow[COL_MACHINE_NAME] = m.Name;
            mRow[COL_MACHINE_DESC] = m.Description;
            mRow[COL_MACHINE_WORKCENTER] = m.Workcenter;
            mRow[COL_MACHINE_RESOURCE_TYPE] = m.ResourceType.ToString();

            //AddPTAttributesToRowAndTable(mRow, m.Attributes);

            machinesTable.Rows.Add(mRow);

            AddBlocks(blocksTable, materialsTable, m, a_minScheduledStart, maxScheduledStart, includeMaterials, sd);
        }

        return machinesTable;
    }

    private DataTable AddBlocks(DataTable a_blocksTable, DataTable a_materialsTable, Resource a_resource, DateTime a_minScheduledStart, DateTime a_maxScheduledStart, bool a_includeMaterials, ScenarioDetail a_sd)
    {
        ResourceBlockList.Node node = a_resource.Blocks.First;
        DateTime curDate = DateTime.MinValue;
        int curRunNbr = 0;
        int curSequence = 1;
        while (node != null)
        {
            DataRow bRow = a_blocksTable.NewRow();
            ResourceBlock b = node.Data;
            if (b.StartDateTime >= a_minScheduledStart && b.StartDateTime <= a_maxScheduledStart)
            {
                bRow[COL_PLANT_ID] = a_resource.Department.Plant.Id.ToBaseType();
                bRow[COL_DEPARTMENT_ID] = a_resource.Department.Id.ToBaseType();
                bRow[COL_MACHINE_ID] = a_resource.Id.ToBaseType();
                bRow[COL_BLOCK_ID] = b.GetKey().ToString();
                bRow[COL_SEQUENCE] = curSequence;
                curSequence++;
                DateTimeOffset displayStart = b.StartDateTime.ToDisplayTime();
                DateTimeOffset displayEnd = b.EndDateTime.ToDisplayTime();
                bRow[COL_DAY] = displayStart.DayOfWeek.ToString();
                RecurringCapacityInterval rci = a_resource.RecurringCapacityIntervals.FindActiveIntervalAtPointInTime(b.StartDateTime);
                if (rci != null)
                {
                    bRow[COL_SHIFT_START] = rci.Name;
                }

                rci = a_resource.RecurringCapacityIntervals.FindActiveIntervalAtPointInTime(b.EndDateTime);
                if (rci != null)
                {
                    bRow[COL_SHIFT_END] = rci.Name;
                }

                bRow[COL_START_DATE_ONLY] = displayStart.Date;
                bRow[COL_WEEK_NBR] = WeekNumberCalculator.GetWeekNumberForCurrentCulture(displayStart);
                if (displayStart.Date > curDate.Date) //in a new day
                {
                    curRunNbr = 1;
                    curDate = displayStart.Date;
                }
                else //new activity in the same day
                {
                    curRunNbr++;
                }

                bRow[COL_RUN_NBR] = curRunNbr;
                bRow[COL_JOB_NAME] = b.Activity.Operation.ManufacturingOrder.Job.Name;
                bRow[COL_JOB_DESC] = b.Activity.Operation.ManufacturingOrder.Job.Description;
                bRow[COL_MO_NAME] = b.Activity.Operation.ManufacturingOrder.Name;
                bRow[COL_OP_NAME] = b.Activity.Operation.Name;
                bRow[COL_MO_PART] = b.Activity.Operation.ManufacturingOrder.ProductName;
                bRow[COL_DESCRIPTION] = b.Activity.Operation.Description;
                bRow[COL_ACTIVITY_START] = b.StartDateTime.ToDisplayTime().ToDateTime();
                bRow[COL_ACTIVITY_END] = b.EndDateTime.ToDisplayTime().ToDateTime();
                bRow[COL_PROD_STATUS] = b.Activity.ProductionStatus.ToString();
                bRow[COL_PAUSED] = b.Activity.Paused;
                bRow[COL_ACTIVITY_REQ_QTY] = b.Activity.RequiredFinishQty;
                bRow[COL_SETUP_HOURS] = b.Activity.ScheduledSetupSpan.TotalHours;
                bRow[COL_RUN_HOURS] = b.Activity.ScheduledProductionSpan.TotalHours;
                bRow[COL_NBR_OF_PEOPLE] = b.Activity.NbrOfPeople;
                bRow[COL_OPERATION_SETUP_NBR] = b.Activity.Operation.SetupNumber;
                bRow[COL_OPERATION_SETUP_COLOR] = b.Activity.Operation.SetupColor;
                bRow[COL_CUSTOMER] = b.Activity.Operation.ManufacturingOrder.Job.Customers.GetCustomerNamesList();

                //Hold
                GetConstrainingHold(b.Activity.Operation, out string holdType, out DateTime holdUntil, out string reason);
                bRow[COL_HOLD] = holdType;
                bRow[COL_HOLD_REASON] = reason;
                bRow[COL_HOLD_UNTIL] = holdUntil.ToDisplayTime().ToDateTime();

                bRow[COL_ANCHORED] = b.Activity.Anchored;
                bRow[COL_LOCKED] = b.Locked;
                decimal nbrCycles = -1;
                decimal qtyPerCycle = -1;
                if (b.Activity.Operation is ResourceOperation)
                {
                    qtyPerCycle = b.Activity.GetResourceProductionInfo(a_resource).QtyPerCycle;
                    if (qtyPerCycle > 0)
                    {
                        nbrCycles = b.Activity.RequiredStartQty / qtyPerCycle;
                    }
                }

                bRow[COL_CYCLES] = Math.Round(nbrCycles, 1);
                bRow[COL_QTY_PER_CYCLE] = Math.Round(qtyPerCycle, 2);
                bRow[COL_SLACKDAYS] = b.Activity.Slack.TotalDays;
                bRow[COL_JOB_NEED_DATE] = b.Activity.Operation.ManufacturingOrder.Job.NeedDateTime.ToDisplayTime().ToDateTime();
                bRow[COL_OP_NEED_DATE] = b.Activity.Operation.NeedDate.ToDisplayTime().ToDateTime();
                bRow[COL_PRIORITY] = b.Activity.Operation.ManufacturingOrder.Job.Priority;
                if (b.Activity.Comments != null)
                {
                    bRow[COL_COMMENTS] = b.Activity.Comments;
                }

                AddPTAttributesToRowAndTable(bRow, b.Activity.Operation.Attributes);

                a_blocksTable.Rows.Add(bRow);

                if (a_includeMaterials)
                {
                    AddMaterials(a_materialsTable, b, a_resource, a_sd);
                }
            }

            node = node.Next;
        }

        return a_blocksTable;
    }

    private DataTable AddBlocks(Resource a_resource, DateTime a_minScheduledStart, DateTime a_maxScheduledStart, bool a_includeMaterials, ScenarioDetail a_sd)
    {
        DataTable blocksTable = Tables["Activities"];
        ResourceBlockList.Node node = a_resource.Blocks.First;
        DateTime curDate = DateTime.MinValue;
        int curRunNbr = 0;
        int curSequence = 1;
        blocksTable.Rows.Clear();

        while (node != null)
        {
            DataRow bRow = blocksTable.NewRow();
            ResourceBlock b = node.Data;
            if (b.StartDateTime >= a_minScheduledStart && b.StartDateTime <= a_maxScheduledStart)
            {
                bRow[COL_PLANT_ID] = a_resource.Department.Plant.Id.ToBaseType();
                bRow[COL_DEPARTMENT_ID] = a_resource.Department.Id.ToBaseType();
                bRow[COL_MACHINE_ID] = a_resource.Id.ToBaseType();
                bRow[COL_BLOCK_ID] = b.GetKey().ToString();
                bRow[COL_SEQUENCE] = curSequence;
                curSequence++;
                DateTimeOffset displayStart = b.StartDateTime.ToDisplayTime();
                DateTimeOffset displayEnd = b.EndDateTime.ToDisplayTime();
                bRow[COL_DAY] = displayStart.DayOfWeek.ToString();
                RecurringCapacityInterval rci = a_resource.RecurringCapacityIntervals.FindActiveIntervalAtPointInTime(b.StartDateTime);
                if (rci != null)
                {
                    bRow[COL_SHIFT_START] = rci.Name;
                }

                rci = a_resource.RecurringCapacityIntervals.FindActiveIntervalAtPointInTime(b.EndDateTime);
                if (rci != null)
                {
                    bRow[COL_SHIFT_END] = rci.Name;
                }

                bRow[COL_START_DATE_ONLY] = displayStart.Date;
                bRow[COL_WEEK_NBR] = WeekNumberCalculator.GetWeekNumberForCurrentCulture(displayStart);
                if (displayStart.Date > curDate.Date) //in a new day
                {
                    curRunNbr = 1;
                    curDate = displayStart.Date;
                }
                else //new activity in the same day
                {
                    curRunNbr++;
                }

                bRow[COL_RUN_NBR] = curRunNbr;
                bRow[COL_JOB_NAME] = b.Activity.Operation.ManufacturingOrder.Job.Name;
                bRow[COL_JOB_DESC] = b.Activity.Operation.ManufacturingOrder.Job.Description;
                bRow[COL_MO_NAME] = b.Activity.Operation.ManufacturingOrder.Name;
                bRow[COL_OP_NAME] = b.Activity.Operation.Name;
                bRow[COL_MO_PART] = b.Activity.Operation.ManufacturingOrder.ProductName;
                bRow[COL_DESCRIPTION] = b.Activity.Operation.Description;
                bRow[COL_ACTIVITY_START] = b.StartDateTime.ToDisplayTime().ToDateTime();
                bRow[COL_ACTIVITY_END] = b.EndDateTime.ToDisplayTime().ToDateTime();
                bRow[COL_PROD_STATUS] = b.Activity.ProductionStatus.ToString();
                bRow[COL_PAUSED] = b.Activity.Paused;
                bRow[COL_ACTIVITY_REQ_QTY] = b.Activity.RequiredFinishQty;
                bRow[COL_SETUP_HOURS] = b.Activity.ScheduledSetupSpan.TotalHours;
                bRow[COL_RUN_HOURS] = b.Activity.ScheduledProductionSpan.TotalHours;
                bRow[COL_NBR_OF_PEOPLE] = b.Activity.NbrOfPeople;
                bRow[COL_OPERATION_SETUP_NBR] = b.Activity.Operation.SetupNumber;
                bRow[COL_OPERATION_SETUP_COLOR] = b.Activity.Operation.SetupColor;
                bRow[COL_CUSTOMER] = b.Activity.Operation.ManufacturingOrder.Job.Customers.GetCustomerNamesList();

                //Hold
                GetConstrainingHold(b.Activity.Operation, out string holdType, out DateTime holdUntil, out string reason);
                bRow[COL_HOLD] = holdType;
                bRow[COL_HOLD_REASON] = reason;
                bRow[COL_HOLD_UNTIL] = holdUntil.ToDisplayTime().ToDateTime();

                bRow[COL_ANCHORED] = b.Activity.Anchored;
                bRow[COL_LOCKED] = b.Locked;
                decimal nbrCycles = -1;
                decimal qtyPerCycle = -1;
                if (b.Activity.Operation is ResourceOperation)
                {
                    qtyPerCycle = b.Activity.GetResourceProductionInfo(a_resource).QtyPerCycle;
                    if (qtyPerCycle > 0)
                    {
                        nbrCycles = b.Activity.RequiredStartQty / qtyPerCycle;
                    }
                }

                bRow[COL_CYCLES] = Math.Round(nbrCycles, 1);
                bRow[COL_QTY_PER_CYCLE] = Math.Round(qtyPerCycle, 2);
                bRow[COL_SLACKDAYS] = b.Activity.Slack.TotalDays;
                bRow[COL_JOB_NEED_DATE] = b.Activity.Operation.ManufacturingOrder.Job.NeedDateTime.ToDisplayTime().ToDateTime();
                bRow[COL_OP_NEED_DATE] = b.Activity.Operation.NeedDate.ToDisplayTime().ToDateTime();
                bRow[COL_PRIORITY] = b.Activity.Operation.ManufacturingOrder.Job.Priority;
                if (b.Activity.Comments != null)
                {
                    bRow[COL_COMMENTS] = b.Activity.Comments;
                }

                AddPTAttributesToRowAndTable(bRow, b.Activity.Operation.Attributes);

                blocksTable.Rows.Add(bRow);

                if (a_includeMaterials)
                {
                    DataTable materialsTable = Tables["Materials"];
                    materialsTable.Rows.Clear();
                    AddMaterials(materialsTable, b, a_resource, a_sd);
                }
            }

            node = node.Next;
        }

        return blocksTable;
    }

    private DataTable AddMaterials(DataTable a_materialsTable, ResourceBlock a_rb, Resource a_r, ScenarioDetail a_sd)
    {
        if (a_rb.SatisfiedRequirement != null)
        {
            InternalOperation op = a_rb.SatisfiedRequirement.Operation;
            InternalActivity a = a_rb.Activity;
            decimal matlPct = 1;
            if (op.Activities.Count > 1 && op.RequiredFinishQty != 0) //Split so divide material requirements
            {
                matlPct = a.RequiredFinishQty / op.RequiredFinishQty;
            }

            for (int matlI = 0; matlI < op.MaterialRequirements.Count; matlI++)
            {
                MaterialRequirement mr = op.MaterialRequirements[matlI];
                DataRow mRow = a_materialsTable.NewRow();
                mRow[COL_PLANT_ID] = a_r.Department.Plant.Id.ToBaseType();
                mRow[COL_DEPARTMENT_ID] = a_r.Department.Id.ToBaseType();
                mRow[COL_MACHINE_ID] = a_r.Id.ToBaseType();
                mRow[COL_BLOCK_ID] = a_rb.GetKey().ToString();

                mRow[COL_MATERIAL_NAME] = mr.MaterialName;
                mRow[COL_MATERIAL_DESCRIPTION] = mr.MaterialDescription;
                mRow[COL_MATERIAL_TOTAL_QTY] = mr.TotalRequiredQty;
                mRow[COL_MATERIAL_UOM] = mr.UOM;
                mRow[c_materialLatestSourceDate] = mr.LatestSourceDateTime.ToDisplayTime().ToDateTime();
                if (!mr.BuyDirect)
                {
                    if (mr.Warehouse == null)
                    {
                        decimal totalOnHandQty = 0;
                        foreach (Warehouse warehouse in a_sd.WarehouseManager)
                        {
                            decimal onHandQty = warehouse.Inventories.GetOnHandQty(mr.Item);
                            if (onHandQty != -1)
                            {
                                totalOnHandQty += onHandQty;
                            }
                        }

                        mRow[COL_ONHAND_QTY] = totalOnHandQty;
                    }
                    else
                    {
                        mRow[COL_ONHAND_QTY] = mr.Warehouse.Inventories.GetOnHandQty(mr.Item);
                    }
                }

                mRow[COL_MATERIAL_ISSUED_COMPLETE] = mr.IssuedComplete;
                mRow[COL_MATERIAL_ISSUED_QTY] = mr.IssuedQty;
                mRow[COL_MATERIAL_SOURCE] = mr.Source;
                mRow[COL_MATERIAL_EXTERNAL_ID] = mr.ExternalId;
                mRow[COL_MATERIAL_CONSTRAINT_TYPE] = mr.ConstraintType.ToString();
                mRow[COL_MATERIAL_BUY_DIRECT] = mr.BuyDirect;
                if (mr.Warehouse != null)
                {
                    mRow[COL_MATERIAL_WAREHOUSE] = mr.Warehouse.Name;
                }

                StringBuilder fullDescription = new ();
                mRow[COL_MATERIAL_SUPPLY] = mr.MRSupply.GetDescription();

                a_materialsTable.Rows.Add(mRow);
            }
        }

        return a_materialsTable;
    }

    #region Hold
    /// <summary>
    /// Returns values for the most constraining of the holds, if any.
    /// </summary>
    /// <returns>DateTime is in server time.</returns>
    private void GetConstrainingHold(BaseOperation op, out string holdType, out DateTime holdUntil, out string reason)
    {
        holdUntil = PTDateTime.MinDateTime;
        reason = "";
        holdType = "";
        if (op.OnHold)
        {
            holdType = "Operation Hold";
            reason = op.HoldReason;
            holdUntil = op.HoldUntil;
        }

        if (op.ManufacturingOrder.Hold && op.ManufacturingOrder.HoldUntil > holdUntil)
        {
            holdType = "M.O. Hold";
            reason = op.ManufacturingOrder.HoldReason;
            holdUntil = op.ManufacturingOrder.HoldUntil;
        }

        if (op.ManufacturingOrder.Job.Hold && op.ManufacturingOrder.Job.HoldUntil > holdUntil)
        {
            holdType = "Job Hold";
            reason = op.ManufacturingOrder.Job.HoldReason;
            holdUntil = op.ManufacturingOrder.Job.HoldUntil;
        }
    }
    #endregion

    /// <summary>
    /// If the object has PTAttributes then add them to the row.  This may mean adding columns to the table.
    /// </summary>
    private static void AddPTAttributesToRowAndTable(DataRow a_row, AttributesCollection a_attributesCollection)
    {
        for (int attribI = 0; attribI < a_attributesCollection.Count; attribI++)
        {
            OperationAttribute opAttribute = a_attributesCollection[attribI];
            DataColumn column = a_row.Table.Columns[GetColNameForAttribute(opAttribute)];
            if (column == null)
            {
                column = new DataColumn();
                column.ColumnName = GetColNameForAttribute(opAttribute);
                //read-only because the grids can't handle edits of these
                column.ReadOnly = true;

                a_row.Table.Columns.Add(column);
            }

            a_row[column] = opAttribute.Code;
        }
    }

    private static string GetColNameForAttribute(OperationAttribute attrib)
    {
        return attrib.PTAttribute.Name + " Attribute";
    }

    private void AddPlantDepartmentRelation(DataSet dataSet, DataTable plantsTable, DataTable departmentsTable)
    {
        DataRelation relPlantDept = new ("PlantDepartment", plantsTable.Columns[COL_PLANT_ID], departmentsTable.Columns[COL_PLANT_ID], false);
        dataSet.Relations.Add(relPlantDept);
    }

    private void AddDepartmentMachineRelation(DataSet dataSet, DataTable departmentsTable, DataTable machinesTable)
    {
        DataColumn[] parentColumns = new DataColumn[2];
        parentColumns[0] = departmentsTable.Columns[COL_PLANT_ID];
        parentColumns[1] = departmentsTable.Columns[COL_DEPARTMENT_ID];
        DataColumn[] childColumns = new DataColumn[2];
        childColumns[0] = machinesTable.Columns[COL_PLANT_ID];
        childColumns[1] = machinesTable.Columns[COL_DEPARTMENT_ID];

        DataRelation relation = new ("DeptMachine", parentColumns, childColumns, false);

        dataSet.Relations.Add(relation);
    }

    private void AddMachineBlockRelation(DataSet dataSet, DataTable machinesTable, DataTable blocksTable)
    {
        DataColumn[] parentColumns = new DataColumn[3];
        parentColumns[0] = machinesTable.Columns[COL_PLANT_ID];
        parentColumns[1] = machinesTable.Columns[COL_DEPARTMENT_ID];
        parentColumns[2] = machinesTable.Columns[COL_MACHINE_ID];
        DataColumn[] childColumns = new DataColumn[3];
        childColumns[0] = blocksTable.Columns[COL_PLANT_ID];
        childColumns[1] = blocksTable.Columns[COL_DEPARTMENT_ID];
        childColumns[2] = blocksTable.Columns[COL_MACHINE_ID];

        DataRelation relation = new ("ResourceBlock", parentColumns, childColumns, false);

        dataSet.Relations.Add(relation);
    }

    private void AddBlockMaterialRelation(DataSet dataSet, DataTable blocksTable, DataTable materialsTable)
    {
        DataColumn[] parentColumns = new DataColumn[4];
        parentColumns[0] = blocksTable.Columns[COL_PLANT_ID];
        parentColumns[1] = blocksTable.Columns[COL_DEPARTMENT_ID];
        parentColumns[2] = blocksTable.Columns[COL_MACHINE_ID];
        parentColumns[3] = blocksTable.Columns[COL_BLOCK_ID];
        DataColumn[] childColumns = new DataColumn[4];
        childColumns[0] = materialsTable.Columns[COL_PLANT_ID];
        childColumns[1] = materialsTable.Columns[COL_DEPARTMENT_ID];
        childColumns[2] = materialsTable.Columns[COL_MACHINE_ID];
        childColumns[3] = materialsTable.Columns[COL_BLOCK_ID];

        DataRelation relation = new ("BlockMaterials", parentColumns, childColumns, false);

        dataSet.Relations.Add(relation);
    }
}