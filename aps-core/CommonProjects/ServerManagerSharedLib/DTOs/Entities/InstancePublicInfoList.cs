using System.Collections;
using System.Collections.Generic;

namespace PT.ServerManagerSharedLib.DTOs.Entities
{
    public class InstancePublicInfoList : IEnumerable<InstancePublicInfo>
    {
        public InstancePublicInfoList() { }

        private readonly List<InstancePublicInfo> m_instancePublicInfos = new List<InstancePublicInfo>();

        public int Count
        {
            get { return m_instancePublicInfos.Count;  }
            set { ; }
        }

        public List<InstancePublicInfo> PublicInfos
        {
            get { return m_instancePublicInfos; }
            set { ; }
        }

        public InstancePublicInfo this[int index]
        {
            get { return m_instancePublicInfos[index]; }
            set { m_instancePublicInfos[index] = value; }
        }

        public void Add(InstancePublicInfo a_instancePublicInfo)
        {
            m_instancePublicInfos.Add(a_instancePublicInfo);
        }

        public IEnumerator<InstancePublicInfo> GetEnumerator()
        {
            return m_instancePublicInfos.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return m_instancePublicInfos.GetEnumerator();
        }
    }
}
