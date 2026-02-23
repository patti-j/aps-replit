using PT.APSCommon;
using PT.Common.Exceptions;

namespace PT.SchedulerDefinitions;

public class JobDataSetValidation
{
    /// <summary>
    /// Validates an array of AlternatePath rows (used in JobT and UI)
    /// </summary>
    /// <param name="a_pathRows"></param>
    public static void ValidateAlternatePaths(JobDataSet.AlternatePathRow[] a_pathRows)
    {
        foreach (JobDataSet.AlternatePathRow pathRow in a_pathRows)
        {
            try
            {
                ValidatePathMultipleFinalOps(pathRow);

                if (a_pathRows.Count() > 1)
                {
                    ValidatePathSameOp(a_pathRows, pathRow);
                }
            }
            catch (CommonException pte)
            {
                throw new PTException("4005", pte.InnerException, new object[] { pathRow.ExternalId, pte.Message });
            }
            catch (Exception e)
            {
                throw new PTException("4005", e, new object[] { pathRow.ExternalId, e.StackTrace });
            }
        }
    }

    /// <summary>
    /// checks wheteher any of the paths have multiple final Operations (Operations with no successors)
    /// </summary>
    /// <returns>true if multiple final paths exist</returns>
    private static void ValidatePathMultipleFinalOps(JobDataSet.AlternatePathRow a_pathRow)
    {
        int finalOpCount = 0;

        JobDataSet.AlternatePathNodeRow[] nodeRows = a_pathRow.GetAlternatePathNodeRows();
        foreach (JobDataSet.AlternatePathNodeRow node in nodeRows)
        {
            if (node.IsSuccessorOperationExternalIdNull() || node.SuccessorOperationExternalId == "")
            {
                finalOpCount++;

                if (finalOpCount > 1)
                {
                    throw new PTValidationException("4108", new object[] { a_pathRow.ExternalId });
                }
            }
        }
    }

    /// <summary>
    /// Given a collection of paths and a path being Validated, check to see if Op ExternalIds in path being validated doesn't exist in other paths.
    /// </summary>
    /// <param name="a_pathRows"></param>
    /// <param name="a_pathBeingValidated"></param>
    private static void ValidatePathSameOp(JobDataSet.AlternatePathRow[] a_pathRows, JobDataSet.AlternatePathRow a_pathBeingValidated)
    {
        JobDataSet.AlternatePathNodeRow[] nodeRows = a_pathBeingValidated.GetAlternatePathNodeRows();

        HashSet<string> checkedExternalIds = new ();

        foreach (JobDataSet.AlternatePathNodeRow node in nodeRows) // for each node
        {
            foreach (JobDataSet.AlternatePathRow pathRow in a_pathRows) // check other paths
            {
                string externalId = "";
                string moExternalId = node.MoExternalId;

                if (pathRow.ExternalId == a_pathBeingValidated.ExternalId) // skip if the same path
                {
                    continue;
                }

                if (node.IsSuccessorOperationExternalIdNull() || node.SuccessorOperationExternalId == "")
                {
                    continue;
                }

                externalId = node.SuccessorOperationExternalId;
                if (!checkedExternalIds.Contains(externalId)) // check successor
                {
                    if (OperationExistsInPath(pathRow, externalId, moExternalId))
                    {
                        throw new PTValidationException("4109", new object[] { externalId, pathRow.ExternalId, a_pathBeingValidated.ExternalId });
                    }

                    checkedExternalIds.Add(externalId);
                }

                if (node.IsSuccessorOperationExternalIdNull() || node.PredecessorOperationExternalId == "")
                {
                    continue;
                }

                externalId = node.PredecessorOperationExternalId;
                if (!checkedExternalIds.Contains(externalId)) // check predecessor
                {
                    if (OperationExistsInPath(pathRow, externalId, moExternalId))
                    {
                        throw new PTValidationException("4109", new object[] { externalId, pathRow.ExternalId, a_pathBeingValidated.ExternalId });
                    }

                    checkedExternalIds.Add(externalId);
                }
            }
        }
    }

    /// <summary>
    /// returns bool -> does OpExternalId exist in any of the path's node
    /// </summary>
    /// <param name="a_pathRow"></param>
    /// <param name="a_opExternalId"></param>
    /// <returns></returns>
    private static bool OperationExistsInPath(JobDataSet.AlternatePathRow a_pathRow, string a_opExternalId, string a_moExternalId)
    {
        JobDataSet.AlternatePathNodeRow[] nodeRows = a_pathRow.GetAlternatePathNodeRows();
        foreach (JobDataSet.AlternatePathNodeRow node in nodeRows)
        {
            if (!node.IsSuccessorOperationExternalIdNull() && node.MoExternalId == a_moExternalId)
            {
                if (node.PredecessorOperationExternalId == a_opExternalId || node.SuccessorOperationExternalId == a_opExternalId)
                {
                    return true;
                }
            }
        }

        return false;
    }
}