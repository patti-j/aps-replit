using System.ComponentModel;

using PT.APSCommon;
using PT.Common.Exceptions;
using PT.ERPTransmissions;
using PT.SchedulerDefinitions;
using PT.Transmissions;

namespace PT.Scheduler;

/// <summary>
/// Vessels are the boxes, trays, pallets, racks or other containers in which parts are placed in for moving it around the plant.
/// This can also be used to represent storage space.
/// Vessel Requirements specify the quantity of parts per Vessel and therefore the number of Vessels needed for an operatoin can
/// be calculated from the Operations' RequiredQty  to determine the number of Vessels needed.
/// Vessel availability is displayed in a Plot instead of the Gantt Chart since they are not tracked individually as explained
/// in the NumberAvailable Property. Vessels are different from Resources in that they don't use Capabilities for assignment
/// since no individual resource selection is done.
/// </summary>
public class VesselType : BaseResource, ICloneable, IPTSerializable
{
    #region IPTSerializable Members
    public VesselType(IReader reader)
        : base(reader)
    {
        if (reader.VersionNumber >= 1)
        {
            reader.Read(out numberAvailable);
        }
    }

    public override void Serialize(IWriter writer)
    {
        base.Serialize(writer);

        writer.Write(numberAvailable);
    }

    public new const int UNIQUE_ID = 353;

    [Browsable(false)]
    public override int UniqueId => UNIQUE_ID;
    #endregion

    #region Construction
    public VesselType(BaseId id, Department w, ShopViewResourceOptions resourceOptions)
        : base(id, w, resourceOptions) { }

    /// <summary>
    /// Sets the field values for an ERP transmission.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="wcn"></param>
    /// <param name="plant"></param>
    public VesselType(AttributeManager a_am, BaseId id, VesselTypeT.VesselType v, Department w, ShopViewResourceOptions resourceOptions, PlantManager plants, bool updateConnectors, bool autoDeleteConnectors, PTTransmission t)
        : base(id, w, resourceOptions)
    {
        //Update(a_am, v, w, plants, updateConnectors, autoDeleteConnectors, t);
    }

    public class VesselTypeException : PTException
    {
        public VesselTypeException(string e)
            : base(e) { }
    }
    #endregion

    #region Shared Properties
    private int numberAvailable;

    /// <summary>
    /// Unlike all other Resources, Vessels are not tracked individually and therefore are not defined individually.  Instead, a total number of Vessels must be specified indicating their current inventory.
    /// </summary>
    public int NumberAvailable
    {
        get => numberAvailable;
        set => numberAvailable = value;
    }
    #endregion Shared Properties

    #region Overrides
    /// <summary>
    /// Used as a prefix for generating default names
    /// </summary>
    [Browsable(false)]
    public override string DefaultNamePrefix => "VesselType";

    /// <summary>
    /// If more than the Plant's Bottleneck Threshold of Activites on the Resource's schedule are Capacity Bottlenecked then the Resource is flagged as a Bottleneck.
    /// </summary>
    public override bool Bottleneck => false; //JMC TODO

    /// <summary>
    /// The percent of the Resource's scheduled Activities that are Capacity Bottlenecked.
    /// </summary>
    public override decimal BottleneckPercent => 0; //JMC TODO
    #endregion

    #region Transmission functionality
    //internal void Update(AttributeManager a_am, VesselTypeT.VesselType v, Department w, PlantManager plants, bool updateConnectors, bool autoDeleteConnectors, PTTransmission t)
    //{
    //    Update(a_am, (PT.ERPTransmissions.BaseResource)v, w, plants, updateConnectors, autoDeleteConnectors, t);
    //}
    #endregion

    #region Cloning
    public VesselType Clone()
    {
        return (VesselType)MemberwiseClone();
    }

    object ICloneable.Clone()
    {
        return Clone();
    }
    #endregion

    protected override bool IsActiveSettableToFalse(out string o_msg)
    {
        throw new NotImplementedException();
    }
}