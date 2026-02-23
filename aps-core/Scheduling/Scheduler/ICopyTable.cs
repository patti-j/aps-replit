namespace PT.Scheduler;
/// <summary>
/// Interface for all Managers to enable DataTableManager to copy them to DataTables. (such as AttributesManager)
/// </summary>
//	public interface ICopyTable
//	{
//		System.Type ElementType
//		{
//			get;
//		}
//		BaseIdObject GetByIndex(int index);
//
//		int Count
//		{
//			get;
//		}	
//	}

/// <summary>
/// Temporary until ICopyTable can be modified.
/// </summary>
public interface ICopyTable
{
    Type ElementType { get; }

    object GetRow(int index);

    int Count { get; }
}