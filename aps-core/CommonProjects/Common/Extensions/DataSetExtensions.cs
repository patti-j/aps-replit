using System.Data;

namespace PT.Common.Extensions;

public static class DataSetExtensions
{
    /// <summary>
    /// Begins a load operation on the specified <see cref="DataSet"/> and all its associated <see cref="DataTable"/> objects.
    /// </summary>
    /// <param name="a_dataSet">The <see cref="DataSet"/> on which to begin the load operation.</param>
    /// <remarks>
    /// This method improves performance when loading a large number of rows into a <see cref="DataSet"/> 
    /// by temporarily suspending constraints and index maintenance on all associated <see cref="DataTable"/> objects.
    /// Ensure that <see cref="DataSet.EndLoadData"/> is called after the load operation to resume normal behavior.
    /// </remarks>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="a_dataSet"/> is <c>null</c>.</exception>
    public static void BeginLoadData(this DataSet a_dataSet)
    {
        foreach (DataTable t in a_dataSet.Tables)
        {
            t.BeginLoadData();
        }
    }

    
    /// <summary>
    /// Ends the batch load operation for the specified <see cref="DataSet"/> and all its associated <see cref="DataTable"/> objects.
    /// </summary>
    /// <param name="a_dataSet">The <see cref="DataSet"/> for which to end the batch load operation.</param>
    public static void EndLoadData(this DataSet a_dataSet)
    {
        foreach (DataTable t in a_dataSet.Tables)
        {
            t.EndLoadData();
        }
    }
}

