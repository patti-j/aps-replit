using System.ComponentModel;

using PT.APSCommon;
using PT.Common.Exceptions;
using PT.SchedulerDefinitions;

namespace PT.Scheduler;

/// <summary>
/// Base class from which several GanttViewerLayout objects are derived.
/// </summary>
public abstract class GanttViewerObjectBase2 : GanttViewerObjectBase, IPTSerializable
{
    public new const int UNIQUE_ID = 388;

    #region IPTSerializable Members
    public GanttViewerObjectBase2(IReader reader)
        : base(reader)
    {
        if (reader.VersionNumber >= 1)
        {
            reader.Read(out name);
            reader.Read(out description);
            bool haveSchedulerObject;
            reader.Read(out haveSchedulerObject);
            if (haveSchedulerObject)
            {
                referenceInfo = new ReferenceInfo();
                reader.Read(out referenceInfo.objectType);

                if (referenceInfo.objectType == Resource.UNIQUE_ID)
                {
                    referenceInfo.plantId = new BaseId(reader);
                    ;
                    referenceInfo.deptId = new BaseId(reader);
                    referenceInfo.resourceId = new BaseId(reader);
                }
                else if (referenceInfo.objectType == Department.UNIQUE_ID)
                {
                    referenceInfo.plantId = new BaseId(reader);
                    referenceInfo.deptId = new BaseId(reader);
                }
                else if (referenceInfo.objectType == Plant.UNIQUE_ID)
                {
                    referenceInfo.plantId = new BaseId(reader);
                }
            }
        }
    }

    public override void Serialize(IWriter writer)
    {
        base.Serialize(writer);

        writer.Write(name);
        writer.Write(description);

        writer.Write(schedulerObject != null); //Job gantts have no schedule object
        if (schedulerObject != null)
        {
            if (schedulerObject is Resource)
            {
                writer.Write(Resource.UNIQUE_ID);
                Resource r = (Resource)schedulerObject;
                r.PlantId.Serialize(writer);
                r.DepartmentId.Serialize(writer);
                r.Id.Serialize(writer);
            }
            else if (schedulerObject is Department)
            {
                writer.Write(Department.UNIQUE_ID);
                Department d = (Department)schedulerObject;
                d.PlantId.Serialize(writer);
                d.Id.Serialize(writer);
            }
            else if (schedulerObject is Plant)
            {
                writer.Write(Plant.UNIQUE_ID);
                ((Plant)schedulerObject).Id.Serialize(writer);
            }
            else
            {
                throw new PTException("Invalid object type linked to GanttViewRow.");
            }
        }
    }

    [Browsable(false)]
    public override int UniqueId => UNIQUE_ID;

    [NonSerialized] private ReferenceInfo referenceInfo;

    private class ReferenceInfo
    {
        internal int objectType;
        internal BaseId plantId;
        internal BaseId deptId;
        internal BaseId resourceId;
    }

    internal virtual void RestoreReferences(PlantManager plants)
    {
        if (referenceInfo == null)
        {
            return; //It's null if the object has no scheduler object reference.
        }

        if (referenceInfo.objectType == Resource.UNIQUE_ID)
        {
            schedulerObject = plants.GetResource(new ResourceKey(referenceInfo.plantId, referenceInfo.deptId, referenceInfo.resourceId));
        }
        else if (referenceInfo.objectType == Department.UNIQUE_ID)
        {
            Plant p = plants.GetById(referenceInfo.plantId);
            schedulerObject = p.Departments.GetById(referenceInfo.deptId);
        }
        else if (referenceInfo.objectType == Plant.UNIQUE_ID)
        {
            schedulerObject = plants.GetById(referenceInfo.plantId);
        }
        else
        {
            throw new PTException("Tried to restore a reference to an invalid object in GanttViwerObjectBase2.");
        }

        referenceInfo = null;
    }
    #endregion

    public GanttViewerObjectBase2(BaseId id, BaseId creatorId, bool isPublic, int sortIndex, string name, string description, BaseObject schedulerObject)
        : base(id, creatorId, isPublic, sortIndex)
    {
        this.name = name;
        this.description = description;
        this.schedulerObject = schedulerObject;
    }

    private string name;

    /// <summary>
    /// Text for display only.
    /// </summary>
    public string Name
    {
        get => name;
        set => name = value;
    }

    private string description;

    /// <summary>
    /// Text for display only.
    /// </summary>
    public string Description
    {
        get => description;
        set => description = value;
    }

    private BaseObject schedulerObject;

    /// <summary>
    /// The Id of the Scheduler object to which this object corresponds.  For example, if type=Plant then this Id is the PlantId that this object corresponds to.
    /// </summary>
    public BaseObject SchedulerObject
    {
        get => schedulerObject;
        set => schedulerObject = value;
    }
}