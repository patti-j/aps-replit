namespace PT.Common;

public class ObjectCreatorDelegateArray
{
    private readonly ObjectCreatorDelegate[] m_objectCreatorDelegateArray;

    public ObjectCreatorDelegateArray(int a_size)
    {
        m_objectCreatorDelegateArray = new ObjectCreatorDelegate[a_size];

        for (int i = 0; i < a_size; ++i)
        {
            m_objectCreatorDelegateArray[i] = null;
        }
    }

    public ObjectCreatorDelegate this[int a_i]
    {
        get => m_objectCreatorDelegateArray[a_i];

        set
        {
            if (m_objectCreatorDelegateArray[a_i] != null)
            {
                throw new Exception("Duplicate entry in ObjectCreatorDelegate array. The value can only be set once.");
            }

            m_objectCreatorDelegateArray[a_i] = value;
        }
    }
}