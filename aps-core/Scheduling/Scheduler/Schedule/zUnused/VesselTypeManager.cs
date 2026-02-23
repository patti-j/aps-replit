using System.ComponentModel;

using PT.APSCommon;
using PT.Common.Exceptions;
using PT.ERPTransmissions;
using PT.SchedulerDefinitions;
using PT.Transmissions;

namespace PT.Scheduler;

/// <summary>
/// Manages a sorted list of VesselType objects.
/// </summary>
public class VesselTypeManager : ScenarioBaseObjectManager<VesselType>, IPTSerializable
{
    #region IPTSerializable Members
    public VesselTypeManager(IReader reader, BaseIdGenerator idGen)
        : base(idGen)
    {
        if (reader.VersionNumber >= 1)
        {
            int count;
            reader.Read(out count);
            for (int i = 0; i < count; i++)
            {
                VesselType v = new (reader);
                Add(v);
            }
        }
    }
    
    public new const int UNIQUE_ID = 354;

    [Browsable(false)]
    public override int UniqueId => UNIQUE_ID;
    #endregion

    #region Declarations
    public class VesselTypeManagerException : PTException
    {
        public VesselTypeManagerException(string message, object[] a_stringParameters = null, bool a_appendHelpUrl = false)
            : base(message, a_stringParameters, a_appendHelpUrl) { }
    }
    #endregion

    #region Construction
    public VesselTypeManager(BaseIdGenerator idGen)
        : base(idGen) { }
    #endregion

    #region VesselType Edit Functions
    private VesselType AddDefault(Scenario scenario, Department w, VesselTypeDefaultT t, ShopViewResourceOptions resourceOptions)
    {
        VesselType v = new (NextID(), w, resourceOptions);
        ValidateAdd(v);
        return base.Add(v);
    }

    private VesselType AddCopy(VesselTypeCopyT t)
    {
        ValidateCopy(t);
        VesselType v = GetById(t.originalId);
        return AddCopy(v, v.Clone(), NextID());
    }

    private VesselType Delete(VesselTypeDeleteT t)
    {
        ValidateDelete(t);
        VesselType v = GetById(t.vesselTypeId);
        Remove(v.Id); //Now remove it from the Manager.
        return v;
    }
    #endregion

    #region Transmissions
    private void ValidateAdd(VesselType v)
    {
        if (Contains(v.Id))
        {
            throw new VesselTypeManagerException("2764", new object[] { v.Id.ToString() });
        }
    }

    private void ValidateCopy(VesselTypeCopyT t)
    {
        ValidateExistence(t.originalId);
    }

    private void ValidateDelete(VesselTypeDeleteT t)
    {
        ValidateExistence(t.vesselTypeId);
    }

    public void Receive(Scenario scenario, GanttViewerLayout gvl, Department w, VesselTypeBaseT t, ScenarioDetail scenarioDetail, ShopViewResourceOptions resourceOptions)
    {
        VesselType v = null;
        ScenarioEvents se;
        if (t is VesselTypeDefaultT)
        {
            DispatcherDefinition dispatcherDefinition = scenarioDetail.DispatcherDefinitionManager.DefaultDispatcherDefinition;
            ReadyActivitiesDispatcher dispatcher = dispatcherDefinition.CreateDispatcher();
            v = AddDefault(scenario, w, (VesselTypeDefaultT)t, resourceOptions);
            using (scenario.ScenarioEventsLock.EnterRead(out se))
            {
                se.FireVesselTypeDefaultEvent(v, (VesselTypeDefaultT)t);
            }

            gvl.Add(v);
        }
        else if (t is VesselTypeCopyT)
        {
            v = AddCopy((VesselTypeCopyT)t);
            using (scenario.ScenarioEventsLock.EnterRead(out se))
            {
                se.FireVesselTypeCopyEvent(v, (VesselTypeCopyT)t);
            }

            gvl.Add(v);
        }
        else if (t is VesselTypeDeleteT)
        {
            v = Delete((VesselTypeDeleteT)t);
            using (scenario.ScenarioEventsLock.EnterRead(out se))
            {
                se.FireVesselTypeDeleteEvent(v, (VesselTypeDeleteT)t);
            }

            gvl.Delete(v);
        }
        else if (t is VesselTypeDeleteAllT)
        {
            for (int i = Count - 1; i >= 0; i--)
            {
                v = this[i];
                VesselTypeDeleteT deleteT = new (t.scenarioId, v.Department.Plant.Id, v.Department.Id, v.Id);
                deleteT.Instigator = t.Instigator;
                Delete(deleteT);
                using (scenario.ScenarioEventsLock.EnterRead(out se))
                {
                    se.FireVesselTypeDeleteEvent(v, deleteT);
                }

                gvl.Delete(v);
            }
        }
    }
    #endregion Internal Transmissions

    #region ERP Transmissions
    internal ERPTHandlerResult VesselTypeTHandler(GanttViewerLayout gvl, VesselTypeT.VesselType vn, Department department, out VesselType v, ShopViewResourceOptions resourceOptions, PlantManager plants, bool updateConnectors, bool autoDeleteConnectors, PTTransmission t)
    {
        v = GetByExternalId(vn.ExternalId);

        if (v == null)
        {
            v = new VesselType(m_scenarioDetail.AttributeManager, NextID(), vn, department, resourceOptions, plants, updateConnectors, autoDeleteConnectors, t);
            v.Name = vn.Name;
            Add(v);
            gvl.Add(v);
            return ERPTHandlerResult.added;
        }

        //v.Update(vn, t);
        return ERPTHandlerResult.updated;
    }
    #endregion ERP Transmissions

    #region ICopyTable
    public override Type ElementType => typeof(VesselType);
    #endregion
}