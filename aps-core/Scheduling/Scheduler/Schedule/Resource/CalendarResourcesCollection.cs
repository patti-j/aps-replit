using System.Collections;
using System.Text;

using PT.Common.Exceptions;

namespace PT.Scheduler;

/// <summary>
/// Stores a list of CalendarResources.
/// </summary>
[Serializable]
public class CalendarResourcesCollection : ICopyTable
{
    #region Declarations
    private ArrayList calendarResources = new ();

    public class CalendarResourcesCollectionException : PTException
    {
        public CalendarResourcesCollectionException(string message)
            : base(message) { }
    }
    #endregion

    #region Construction
    #endregion

    #region Properties and Methods
    public Type ElementType => typeof(InternalResource);

    internal InternalResource Add(InternalResource r)
    {
        calendarResources.Add(r);
        return r;
    }

    internal void Remove(int index)
    {
        calendarResources.RemoveAt(index);
    }

    internal bool Remove(InternalResource r)
    {
        for (int i = 0; i < calendarResources.Count; i++)
        {
            if ((InternalResource)calendarResources[i] == r)
            {
                calendarResources.RemoveAt(i);
                return true;
            }
        }

        return false;
    }

    internal void Clear()
    {
        calendarResources.Clear();
    }

    public InternalResource this[int index] => (InternalResource)calendarResources[index];

    public object GetRow(int index)
    {
        return (InternalResource)calendarResources[index];
    }

    public int Count => calendarResources.Count;

    public bool Contains(InternalResource c)
    {
        for (int i = 0; i < calendarResources.Count; i++)
        {
            if ((InternalResource)calendarResources[i] == c)
            {
                return true;
            }
        }

        return false;
    }

    public new string ToString()
    {
        StringBuilder builder = new ();
        for (int i = 0; i < calendarResources.Count; i++)
        {
            InternalResource calendarResource = (InternalResource)calendarResources[i];

            if (i > 0)
            {
                builder.Append(", ");
            }

            builder.Append(calendarResource.Name);
        }

        return builder.ToString();
    }
    #endregion
}