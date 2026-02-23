using System.Text;

using PT.Common.Exceptions;

namespace PT.Scheduler;

/// <summary>
/// Stores a list of Plants.
/// </summary>
public class EligiblePlantsCollection : ICopyTable, IPTSerializable
{
    #region IPTSerializable Members
    internal EligiblePlantsCollection(IReader reader)
    {
        int count;
        reader.Read(out count);
        for (int plantI = 0; plantI < count; ++plantI)
        {
            EligiblePlant eligiblePlant = new (reader);
            Add(eligiblePlant);
        }
    }

    public void Serialize(IWriter writer)
    {
        writer.Write(Count);
        for (int plantI = 0; plantI < Count; ++plantI)
        {
            this[plantI].Serialize(writer);
        }
    }

    public int UniqueId =>
        // TODO:  Add EligiblePlantsCollection.UniqueId getter implementation
        0;

    internal void RestoreReferences(PlantManager a_plants)
    {
        for (int plantI = 0; plantI < Count; ++plantI)
        {
            this[plantI].RestoreReferences(a_plants);
        }
    }
    #endregion

    public EligiblePlantsCollection() { }

    #region Declarations
    private readonly List<EligiblePlant> m_plants = new ();

    public class EligiblePlantsCollectionException : PTException
    {
        public EligiblePlantsCollectionException(string a_message)
            : base(a_message) { }
    }
    #endregion

    #region Properties and Methods
    public Type ElementType => typeof(EligiblePlant);

    public EligiblePlant Add(EligiblePlant a_plant)
    {
        m_plants.Add(a_plant);
        return a_plant;
    }

    public void Remove(int a_index)
    {
        m_plants.RemoveAt(a_index);
    }

    public object GetRow(int a_index)
    {
        return m_plants[a_index];
    }

    public int Count => m_plants.Count;

    /// <summary>
    /// Set the plant eligibility equal to all the plants in the given plant manager.
    /// </summary>
    /// <param name="plantManager"></param>
    public void MakeEligible(PlantManager a_plantManager)
    {
        Clear();

        for (int plantI = 0; plantI < a_plantManager.Count; plantI++)
        {
            Plant plant = a_plantManager.GetByIndex(plantI);
            Add(new EligiblePlant(plant));
        }
    }

    public EligiblePlant this[int a_index] => m_plants[a_index];

    internal void Clear()
    {
        m_plants.Clear();
    }
    #endregion

    public override string ToString()
    {
        StringBuilder sb = new ();
        for (int i = 0; i < Count; ++i)
        {
            EligiblePlant ep = this[i];
            if (i != 0)
            {
                sb.Append("; ");
            }

            sb.Append(ep);
        }

        return sb.ToString();
    }
}