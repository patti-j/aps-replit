using PT.APSCommon;
using PT.Common.Exceptions;

namespace PT.Scheduler;

/// <summary>
/// A Plant that is allowed to have the work assigned to it.
/// </summary>
public class EligiblePlant : ExternalBaseIdObject, ICloneable, IPTSerializable
{
    #region IPTSerializable Members
    private ReferenceInfo referenceInfo;

    internal EligiblePlant(IReader reader)
        : base(reader)
    {
        reader.Read(out m_preference);
        referenceInfo = new ReferenceInfo();
        referenceInfo.plantId = new BaseId(reader);
    }

    public override void Serialize(IWriter writer)
    {
        base.Serialize(writer);
        writer.Write(m_preference);
        Plant.Id.Serialize(writer);
    }

    public override int UniqueId =>
        // TODO:  Add EligiblePlant.UniqueId getter implementation
        0;

    internal class ReferenceInfo
    {
        internal BaseId plantId;
    }

    internal void RestoreReferences(PlantManager a_plants)
    {
        m_plant = a_plants.GetById(referenceInfo.plantId);
        referenceInfo = null;
    }
    #endregion

    #region Construction
    public EligiblePlant(Plant a_plant, int a_preference)
        : base(a_plant.Id)
    {
        Plant = a_plant;
        m_preference = a_preference;
    }

    public EligiblePlant(Plant a_plant)
        : base(a_plant.Id)
    {
        Plant = a_plant;
    }

    internal EligiblePlant(EligiblePlant a_source)
        : this(a_source.Plant, a_source.m_preference) { }
    #endregion

    #region Declarations
    //Property names for DataTables.
    public const string PREFERENCE = "Preference";

    public class EligiblePlantException : PTException
    {
        public EligiblePlantException(string a_message)
            : base(a_message) { }
    }
    #endregion

    #region Shared Properties
    private int m_preference;

    /// <summary>
    /// Specifies, relative to other Plants, which Plants should be selected first.
    /// </summary>
    public int Preference
    {
        get => m_preference;
        set => m_preference = value;
    }
    #endregion

    private Plant m_plant;

    /// <summary>
    /// A Plant that can perform the work.
    /// </summary>
    public Plant Plant
    {
        get => m_plant;
        set => m_plant = value;
    }

    #region Cloning
    public EligiblePlant Clone()
    {
        return new EligiblePlant(this);
    }

    object ICloneable.Clone()
    {
        return Clone();
    }
    #endregion

    public override string ToString()
    {
        return string.Format("{0}; PlantPreference={1}", Plant, Preference);
    }
}