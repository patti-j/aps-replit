using PT.APSCommon.Extensions;
using PT.Scheduler;
using PT.SchedulerDefinitions;

namespace PT.SchedulerData;

public static class AlternatePathData
{
    public static void PtDbPopulate(this AlternatePath a_path, ref Database.PtDbDataSet a_dataSet, Database.PtDbDataSet.ManufacturingOrdersRow a_moRow)
    {
        Database.PtDbDataSet.JobPathsRow pathRow = a_dataSet.JobPaths.AddJobPathsRow(
            a_moRow.PublishDate,
            a_moRow.InstanceId,
            a_moRow.JobId,
            a_moRow.ManufacturingOrderId,
            a_path.Id.ToBaseType(),
            a_path.ExternalId,
            a_path.Name,
            a_path.Preference,
            a_path.AutoUse.ToString(),
            a_path.AutoUseReleaseOffsetTimeSpan.TotalDays,
            a_path.ValidityStartDate,
            a_path.ValidityEndDate);

        for (int nodeI = 0; nodeI < a_path.NodeCount; nodeI++)
        {
            a_path.GetNodeByIndex(nodeI).PtDbPopulate(ref a_dataSet, pathRow);
        }
    }

    public static void PtDbPopulate(this AlternatePath.Node a_node, ref Database.PtDbDataSet a_dataSet, Database.PtDbDataSet.JobPathsRow a_pathRow)
    {
        if (a_node.Successors.Count == 0) //just add one record for the node
        {
            a_dataSet.JobPathNodes.AddJobPathNodesRow(
                a_pathRow.PublishDate,
                a_pathRow.InstanceId,
                a_pathRow.JobId,
                a_pathRow.ManufacturingOrderId,
                a_pathRow.PathId,
                a_node.Operation.Id.ToBaseType(),
                -1,
                0,
                0,
                0,
                InternalOperationDefs.overlapTypes.NoOverlap.ToString().Localize(),
                0,
                0,
                false,
                false,
                OperationDefs.EOperationTransferPoint.EndOfOperation.Localize(),
                OperationDefs.EOperationTransferPoint.StartOfOperation.Localize());
        }
        else //Add one row for each successor
        {
            for (int sucI = 0; sucI < a_node.Successors.Count; sucI++)
            {
                AlternatePath.Association association = a_node.Successors[sucI];
                a_dataSet.JobPathNodes.AddJobPathNodesRow(
                    a_pathRow.PublishDate,
                    a_pathRow.InstanceId,
                    a_pathRow.JobId,
                    a_pathRow.ManufacturingOrderId,
                    a_pathRow.PathId,
                    a_node.Operation.Id.ToBaseType(),
                    association.Successor.Operation.Id.ToBaseType(),
                    association.UsageQtyPerCycle,
                    association.MaxDelay.TotalHours,
                    association.TransferSpan.TotalHours,
                    association.OverlapType.ToString().Localize(),
                    association.OverlapTransferSpan.TotalHours,
                    Convert.ToDouble(association.OverlapPercentComplete),
                    association.AllowManualConnectorViolation,
                    association.TransferDuringPredeccessorOnlineTime,
                    association.TransferStart.ToString().Localize(),
                    association.TransferEnd.ToString().Localize()
                );
            }
        }
    }
}