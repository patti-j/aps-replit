namespace PT.Scheduler;

/// <summary>
/// Summary description for BalancedCompositeDispatcher.
/// </summary>
public partial class BalancedCompositeDispatcherDefinition
{
    internal class Key : IDispatchKey
    {
        internal Key(decimal a_composite, long a_id, long a_multiTaskingOrder)
        {
            Composite = a_composite;
            ObjectId = a_id;
            SimultaneousSequenceIndex = a_multiTaskingOrder;
        }

        internal decimal Composite;
        internal long ObjectId;
        internal long SimultaneousSequenceIndex;

        public override string ToString()
        {
            string s;

            s = GetType().Name + "::" + Composite + "; " + SimultaneousSequenceIndex + "; " + ObjectId;

            return s;
        }

        public decimal Score => Composite;
    }
}