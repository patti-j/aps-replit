using System.Collections;

namespace PT.ERPTransmissions;

public partial class JobT
{
    public class SuccessorMOArrayList : IPTSerializable
    {
        #region IPTSerializable Members
        public SuccessorMOArrayList(IReader reader)
        {
            int successorMOCnt;
            reader.Read(out successorMOCnt);
            for (int i = 0; i < successorMOCnt; ++i)
            {
                SuccessorMO smo = new (reader);
                Add(smo);
            }
        }

        public SuccessorMOArrayList() { }

        public void Serialize(IWriter writer)
        {
            writer.Write(Count);
            for (int i = 0; i < Count; ++i)
            {
                ((IPTSerializable)this[i]).Serialize(writer);
            }
        }

        public int UniqueId => 0;
        #endregion

        #region List functionality
        private readonly ArrayList successorMOs = new ();

        public void Add(SuccessorMO smo)
        {
            successorMOs.Add(smo);
        }

        public int Count => successorMOs.Count;

        public SuccessorMO this[int index] => (SuccessorMO)successorMOs[index];
        #endregion
    }
}