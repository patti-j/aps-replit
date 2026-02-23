using System.Collections;

namespace PT.Scheduler;

public class PlantArrayList
{
    private readonly ArrayList m_plantArrayList = new ();

    public void Add(Plant a_plant)
    {
        m_plantArrayList.Add(a_plant);
    }

    public void Clear()
    {
        m_plantArrayList.Clear();
    }

    public int Count => m_plantArrayList.Count;

    public Plant this[int a_index] => (Plant)m_plantArrayList[a_index];

    public bool Contains(Plant a_plant)
    {
        return m_plantArrayList.Contains(a_plant);
    }

    public void Remove(Plant a_plant)
    {
        m_plantArrayList.Remove(a_plant);
    }

    public void Copy(PlantArrayList a_copy)
    {
        Clear();
        m_plantArrayList.AddRange(a_copy.m_plantArrayList);
    }

    public void Copy(PlantManager a_plantManager)
    {
        Clear();
        for (int plantManagerI = 0; plantManagerI < a_plantManager.Count; plantManagerI++)
        {
            Plant plant = a_plantManager.GetByIndex(plantManagerI);
            m_plantArrayList.Add(plant);
        }
    }

    internal void Union(PlantArrayList a_list)
    {
        for (int listIdx = 0; listIdx < a_list.Count; listIdx++)
        {
            Plant plant = a_list[listIdx];
            if (!m_plantArrayList.Contains(plant))
            {
                m_plantArrayList.Add(plant);
            }
        }
    }

    internal void Intersection(PlantArrayList a_list)
    {
        for (int plantArrayListIdx = 0; plantArrayListIdx < m_plantArrayList.Count; plantArrayListIdx++)
        {
            Plant plant = (Plant)m_plantArrayList[plantArrayListIdx];

            if (!a_list.m_plantArrayList.Contains(plant))
            {
                m_plantArrayList.Remove(plant);
            }
        }
    }
}