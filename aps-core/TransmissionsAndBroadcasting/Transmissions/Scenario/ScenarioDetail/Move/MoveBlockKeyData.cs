using PT.Scheduler;
using PT.SchedulerDefinitions;

namespace PT.Transmissions;

/// <summary>
/// Identifies a block and some of its activities. This object is constant. Values passed to its constructors are all deep copied, so
/// Once set, there's no way to change the values in it.
/// </summary>
public class MoveBlockKeyData : IPTSerializable, IEnumerable<ActivityKey>
{
    #region Serialization
    public MoveBlockKeyData(IReader reader)
    {
        if (reader.VersionNumber >= 487)
        {
            bool haveResourceKey;
            reader.Read(out haveResourceKey);
            if (haveResourceKey)
            {
                m_resourceKey = new ResourceKey(reader);
            }

            m_blockKey = new BlockKey(reader);
            m_activityKeyList = new ActivityKeyList(reader);
        }
        else if (reader.VersionNumber >= 484)
        {
            m_resourceKey = new ResourceKey(reader);
            m_blockKey = new BlockKey(reader);
            m_activityKeyList = new ActivityKeyList(reader);
        }
        else
        {
            throw new Exception("This scenario file is no longer valid and can no longer be used. This class has changed significantly. Old versions of it are no longer supported.");
        }
    }

    public virtual void Serialize(IWriter writer)
    {
        writer.Write(m_resourceKey != null);
        if (m_resourceKey != null)
        {
            m_resourceKey.Serialize(writer);
        }

        m_blockKey.Serialize(writer);
        m_activityKeyList.Serialize(writer);
    }

    public const int UNIQUE_ID = 789;

    public virtual int UniqueId => UNIQUE_ID;
    #endregion

    private ResourceKey m_resourceKey;

    /// <summary>
    /// The resource the block is being moved from.
    /// </summary>
    public ResourceKey ResourceKey
    {
        get => m_resourceKey;
        private set => m_resourceKey = value;
    }

    /// <summary>
    /// Set the resource key if it hasn't already been specified.
    /// </summary>
    /// <param name="a_k"></param>
    public void SetResourceKey(ResourceKey a_k)
    {
        if (a_k == null)
        {
            throw new Exception("Resource key can't be null.");
        }

        if (ResourceKey != null)
        {
            throw new Exception("The resource key has already been set and can't be changed.");
        }

        ResourceKey = new ResourceKey(a_k);
    }

    private BlockKey m_blockKey;

    /// <summary>
    /// The block being moved.
    /// </summary>
    public BlockKey BlockKey => m_blockKey;

    /// <summary>
    /// Used to identify some activities within the block referred to by an object of this type.
    /// </summary>
    private ActivityKeyList m_activityKeyList;

    /// <summary>
    /// Deep copies of the objects passed to this constructor are made, changing them later will have no affect on this object.
    /// This object is basically constant.
    /// </summary>
    /// <param name="a_resourceKey"></param>
    /// <param name="a_block"></param>
    /// <param name="a_keys"></param>
    public MoveBlockKeyData(ResourceKey a_resourceKey, BlockKey a_block, ActivityKeyList a_keys)
    {
        DeepCopyInitialize(a_resourceKey, a_block, a_keys);
    }

    /// <summary>
    /// Create copies of the keys referred to by this block, so even if the keys passed in are changed by some other code,
    /// this object maintains its own unchangable copy.
    /// </summary>
    /// <param name="a_resourceKey"></param>
    /// <param name="a_block"></param>
    /// <param name="a_keys"></param>
    private void DeepCopyInitialize(ResourceKey a_resourceKey, BlockKey a_block, ActivityKeyList a_keys)
    {
        if (a_resourceKey != null)
        {
            m_resourceKey = new ResourceKey(a_resourceKey);
        }

        m_blockKey = new BlockKey(a_block);
        m_activityKeyList = new ActivityKeyList(a_keys);
    }

    /// <summary>
    /// Deep copy constructor.
    /// </summary>
    /// <param name="a_mbkd"></param>
    public MoveBlockKeyData(MoveBlockKeyData a_mbkd)
    {
        DeepCopyInitialize(a_mbkd.ResourceKey, a_mbkd.BlockKey, a_mbkd.m_activityKeyList);
    }

    #region Enumerable
    /// <summary>
    /// Activities of the block.
    /// See description of ActivityKeyList.ActivityKeyEnumerator.
    /// </summary>
    /// <returns></returns>
    public IEnumerator<ActivityKey> GetEnumerator()
    {
        return m_activityKeyList.GetEnumerator();
    }

    /// <summary>
    /// See description of ActivityKeyList.ActivityKeyEnumerator.
    /// </summary>
    /// <returns></returns>
    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
    #endregion

    public override string ToString()
    {
        string s = string.Format("ResourceKey={0}; BlockKey={1}; ActivityCount={2}", m_resourceKey, m_blockKey, m_activityKeyList.Count);
        return s;
    }
}