using System.Xml.Linq;

using PT.Common.File;

namespace PT.Scheduler.Simulation.Scheduler;

public static class UnitTestWriter
{
    /// <summary>
    /// Writes all of the Scenario data such as Resource, Job, and Warehouse inventory data, to an xml file in the form:
    /// <?xml version="1.0" encoding="utf-8"?>
    /// <ScenarioDetail SimulationType="TimeAdjustment" UnitTest="000" ID="014">
    /// <Plant UID="330">
    /// <Department UID="329">
    /// <Resource ID="-2145589069" />
    /// <AC ID="-2145425897" EID="Act1">
    /// <OP>
    /// <Name>CC</Name>
    /// <ID>-2145425898</ID>
    /// <EID>MO-5414-30</EID>
    /// </OP>
    /// <MO>
    /// <Name>MO-5414</Name>
    /// <ID>-2145425903</ID>
    /// <EID>MO-5414</EID>
    /// <Job>
    /// <Name>5414</Name>
    /// <ID>-2145425904</ID>
    /// <EID>5414</EID>
    /// </Job>
    /// </MO>
    /// </AC>
    /// <TimeStamp>
    /// <StartTicks>634250814760000000</StartTicks>
    /// <StartTime>2010-11-11T14:11:16</StartTime>
    /// <EndTicks>634255134760000000</EndTicks>
    /// <EndTime>2010-11-16T14:11:16</EndTime>
    /// </TimeStamp>
    /// </Resource>
    /// </Department>
    /// </Plant>
    /// <Warehouse UID="530">
    /// <Item ID="-2145749607" EID="00-000000" Name="00-000000">
    /// <Adjustment>
    /// <ID>1</ID>
    /// <Time>633881556000000000</Time>
    /// <Date>2009-09-10T05:00:00</Date>
    /// <QTY>52</QTY>
    /// </Adjustment>
    /// <Item ID="-2145438963" EID="test" Name="test" />
    /// </Warehouse>
    /// </ScenarioDetail>
    /// </summary>
    /// <param name="a_Directory">Folder to save the xml file</param>
    /// <param name="a_ScenarioDetail">Scenario class with all of the data</param>
    /// <param name="a_SimulationType">Type of Simulation</param>
    public static void WriteScenarioDetails(string a_Directory, ScenarioDetail a_ScenarioDetail, ScenarioDetail.SimulationType a_SimulationType)
    {
        XDocument XmlData = new ();

        //Create XML Root
        XElement xRoot = new ("ScenarioDetail",
            new XAttribute("SimulationType", a_SimulationType),
            new XAttribute("ID", a_ScenarioDetail.Scenario.Id.Value.ToString("D3"))
        );

        //Build XML Elements
        for (int plantI = 0; plantI < a_ScenarioDetail.PlantManager.Count; ++plantI)
        {
            //A new plant element.
            Plant plant = a_ScenarioDetail.PlantManager[plantI];
            XElement xPlant = new ("Plant");
            xPlant.Add(new XAttribute("UID", plant.UniqueId));

            for (int departmentI = 0; departmentI < plant.Departments.Count; ++departmentI)
            {
                //A new department element
                Department department = plant.Departments[departmentI];
                XElement xDepartment = new ("Department");
                xDepartment.Add(new XAttribute("UID", department.UniqueId));

                for (int resourceI = 0; resourceI < department.Resources.Count; ++resourceI)
                {
                    //A new resource element
                    Resource resource = department.Resources[resourceI];
                    XElement xResource = new ("Resource");
                    xResource.Add(new XAttribute("ID", resource.Id));

                    ResourceBlockList.Node node = resource.Blocks.First;

                    //Add all the jobs, manufacturing orders, operations, actvities, and time stamps.
                    while (node != null)
                    {
                        #region GetResourceData
                        ResourceBlock rb = node.Data;

                        // [REVIEW] Iterate through the activities in the blocks batch instead of using rb.Activity.
                        Batch ba = rb.Batch;
                        IEnumerator<InternalActivity> actI = ba.GetEnumerator();
                        InternalOperation io;
                        ManufacturingOrder mo;

                        XElement xBA = new ("Batch");

                        while (actI.MoveNext())
                        {
                            XElement xAC = new ("Activity",
                                new XAttribute("ID", actI.Current.Id),
                                new XAttribute("EID", actI.Current.ExternalId)
                            );

                            io = actI.Current.Operation;

                            XElement xOP = new ("OP",
                                new XAttribute("Name", io.Name),
                                new XAttribute("ID", io.Id),
                                new XAttribute("EID", io.ExternalId)
                            );

                            mo = actI.Current.ManufacturingOrder;

                            XElement xMO = new ("MO",
                                new XAttribute("Name", mo.Name),
                                new XAttribute("ID", mo.Id),
                                new XAttribute("EID", mo.ExternalId)
                            );

                            XElement xJob = new ("Job",
                                new XAttribute("Name", mo.Job.Name),
                                new XAttribute("ID", mo.Job.Id),
                                new XAttribute("EID", mo.Job.ExternalId)
                            );

                            xBA.Add(xAC);
                            xBA.Add(xOP);
                            xBA.Add(xMO);
                            xBA.Add(xJob);
                        }

                        XElement xTS = new ("TimeStamp",
                            new XAttribute("StartTicks", rb.StartTicks),
                            new XAttribute("StartTime", rb.StartDateTime),
                            new XAttribute("EndTicks", rb.EndTicks),
                            new XAttribute("EndTime", rb.EndDateTime)
                        );

                        xBA.Add(xTS);
                        xResource.Add(xBA);

                        node = node.Next;
                        #endregion GetResourceData
                    }

                    xDepartment.Add(xResource);
                }

                xPlant.Add(xDepartment);
            }

            xRoot.Add(xPlant);
        }

        // Write the inventory records to a file for the purpose of unit testing.
        for (int whI = 0; whI < a_ScenarioDetail.WarehouseManager.Count; ++whI)
        {
            Warehouse wh = a_ScenarioDetail.WarehouseManager.GetByIndex(whI);
            IEnumerator<Inventory> invEnum = wh.Inventories.GetEnumerator();

            XElement xWarehouse = new ("Warehouse");
            xWarehouse.Add(new XAttribute("UID", wh.UniqueId));

            while (invEnum.MoveNext())
            {
                Inventory inv = invEnum.Current;
                XElement xItem = new ("Item",
                    new XAttribute("ID", inv.Item.Id),
                    new XAttribute("EID", inv.Item.ExternalId),
                    new XAttribute("Name", inv.Item.Name));

                if (inv.Adjustments != null)
                {
                    for (int adjI = 0; adjI < inv.Adjustments.Count; ++adjI)
                    {
                        Adjustment adj = inv.Adjustments[adjI];

                        XElement xAdjustment = new ("Adjustment",
                            new XAttribute("ID", adjI + 1),
                            new XAttribute("Time", adj.Time),
                            new XAttribute("Date", adj.AdjDate),
                            new XAttribute("QTY", adj.ChangeQty)
                        );

                        xItem.Add(xAdjustment);
                    }
                }

                xWarehouse.Add(xItem);
            }

            xRoot.Add(xWarehouse);
        }

        XmlData.Add(xRoot);

        //Save Resource Block data
        //Common.File.TextFile file = WriteUnitTestFile(a_ScenarioDetail, a_SimulationType);

        //Set file path
        //Overwrites all files so that only the last simulation's scenario is written
        string fileName = "FinalScenario.UtT";
        string filePath = Path.Combine(a_Directory, fileName);

        //The simulation value is often 0 but it is not usually the first simulation being saved.
        //Keep incrementing the simulation number so that all files are saved.
        //while (File.Exists(filePath))
        //{
        //    simulationNumber++;
        //    fileName = string.Format("scn.{0}.UtT", simulationNumber.ToString("D3"));
        //    filePath = System.IO.Path.Combine(a_Directory, fileName);
        //}

        try
        {
            XmlData.Save(filePath);
        }
        catch
        {
            //If this is the last scenario then the files will be out of sync.
            //Since there is no way to tell if this is the last simulation, don't throw any errors.
        }
    }

    /// <summary>
    /// Write the resource schedule and inventory allocations to a file for the purpose of unit testing.
    /// First all the block on every resource are written then all the inventory items are written.
    /// [1] RESOURCE [Id] [ExternalId] [Name]
    /// JOB [Id] [ExternalId] [Name] *** MO [id] [ExternalId] [Name] *** OP [Id] [ExternalId] [Name] *** AC [Id] [ExternalId]
    /// START [632204060661718750] [5/17/2004 3:54 PM] *** END [632205809661718750] [5/19/2004 4:29 PM]
    /// [2] RESOURCE [2] [M30] [MIX1      ]
    /// JOB [74] [403] [Planned WO 403] *** MO [1] [PO1] [PO1] *** OP [1] [2231] [040   ] *** AC [1] [Activity1]
    /// START [632278332000000000] [8/11/2004 3:00 PM] *** END [632283776880000000] [8/17/2004 10:14 PM]
    /// ----------------------------------------------------------------------------------------------------
    /// [1] RESOURCE [3] [M31] [MOLD1     ]
    /// JOB [55] [41] [00001019       ] *** MO [1] [WO1] [WO1] *** OP [2] [193] [060   ] *** AC [1] [Activity1]
    /// START [632204061071718750] [5/17/2004 3:55 PM] *** END [632204073071718750] [5/17/2004 4:15 PM]
    /// ----------------------------------------------------------------------------------------------------------
    private static TextFile WriteUnitTestFile(ScenarioDetail a_sd, ScenarioDetail.SimulationType a_simulationType)
    {
        TextFile file = new ();

        file.AppendText(a_simulationType.ToString());
        file.AppendText("");
        file.AppendText(GetSeparator());

        for (int plantI = 0; plantI < a_sd.PlantManager.Count; ++plantI)
        {
            Plant plant = a_sd.PlantManager[plantI];

            for (int departmentI = 0; departmentI < plant.Departments.Count; ++departmentI)
            {
                Department department = plant.Departments[departmentI];

                for (int resourceI = 0; resourceI < department.Resources.Count; ++resourceI)
                {
                    string resourceLine;
                    Resource resource = department.Resources[resourceI];
                    ResourceBlockList.Node node = resource.Blocks.First;
                    resourceLine = string.Format("RESOURCE [{0}] [{1}] [{2}]", resource.Id, resource.ExternalId, resource.Name);

                    int count = 0;
                    while (node != null)
                    {
                        ++count;
                        ResourceBlock rb = node.Data;
                        string line = string.Format("[{1}] {0}", resourceLine, count);
                        file.AppendText(line);

                        InternalActivity ia = rb.Activity;
                        InternalOperation io = ia.Operation;
                        ManufacturingOrder mo = io.ManufacturingOrder;

                        PT.Scheduler.Job job = mo.Job;

                        line = string.Format("     JOB [{0}] [{1}] [{2}]", job.Id, job.ExternalId, job.Name);
                        line = string.Format("{0} *** MO [{1}] [{2}] [{3}]", line, mo.Id, mo.ExternalId, mo.Name);
                        line = string.Format("{0} *** OP [{1}] [{2}] [{3}]", line, io.Id, io.ExternalId, io.Name);
                        line = string.Format("{0} *** AC [{1}] [{2}]", line, ia.Id, ia.ExternalId);

                        file.AppendText(line);

                        line = string.Format("     START [{0}] [{1}] *** END [{2}] [{3}]", rb.StartTicks, PrintDate(rb.StartDateTime), rb.EndTicks, PrintDate(rb.EndDateTime));
                        file.AppendText(line);
                        file.AppendText("");

                        node = node.Next;
                    }

                    if (count > 0)
                    {
                        file.AppendText(GetSeparator());
                    }
                }
            }
        }


        //WriteUnitTestItems(file);

        return file;
    }

    private static string GetSeparator()
    {
        return "----------------------------------------------------------------------------------------------------------------------------------------------------------------";
    }

    private static string PrintDate(DateTime a_dt)
    {
        return string.Format("{0} {1}", a_dt.ToShortDateString(), a_dt.ToShortTimeString());
    }
}