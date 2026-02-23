using PT.APSCommon;
using PT.APSCommon.Extensions;
using PT.ImportDefintions;
using PT.ImportDefintions.RequestsAndResponses;
using PT.Transmissions;
using System.Net;
using System.Text;

using PT.Common.Extensions;
using PT.Common.Localization;

namespace PT.Scheduler;

public static class RefreshScenarioDataHelper
{
    public static void RunERPImport(IImportingService a_importingService, BaseId a_targetScenarioId, BaseId a_instigator, int a_configId = -1)
    {
        PerformImportResult performImportResult;
        try
        {
            PerformImportRequest performImportRequestRequest = new ()
            {
                TestOnly = false,
                UserName = "",
                Instigator = a_instigator.Value,
                SpecificScenarioId = a_targetScenarioId.Value,
                SpecificConfigId = a_configId
            };

            performImportResult = a_importingService.RunImport(performImportRequestRequest);
        }
        catch (WebException e)
        {
            string msg = Localizer.GetErrorString("2444", new object[] { e.Message }, true);
            throw new ImportException(msg);
        }
        catch (InterfaceLockedException lockedErr)
        {
            string msg = Localizer.GetErrorString("2446", new object[] { lockedErr.lockUser, lockedErr.lockTime }, true);
            throw new ImportException(msg);
        }
        catch (ImporterConnectionException connectionErr)
        {
            string msg = Localizer.GetErrorString("2447", new object[] { connectionErr.Message }, true);
            throw new ImportException(msg);
        }
        catch (ImporterCommandException commandeErr)
        {
            string msg = Localizer.GetErrorString("2448", new object[] { commandeErr.Message }, true);
            throw new ImportException(msg);
        }
        catch (ImporterInvalidConnectionException invalidConnErr)
        {
            string msg = Localizer.GetErrorString("2449", new object[] { invalidConnErr.Message }, true);
            throw new ImportException(msg);
        }
        catch (ImporterException ie)
        {
            string msg = Localizer.GetErrorString("2450", new object[] { ie.Message }, true);
            throw new ImportException(msg);
        }
        catch (ValidationException vErr)
        {
            string msg = Localizer.GetErrorString("2451", new object[] { vErr.Message }, true);
            throw new ImportException(msg);
        }
        catch (CommonException ptErr)
        {
            //Parsed from PTMessage code that is no longer referenced
            string message = ptErr.Message;
            string details = ptErr.GetExceptionFullMessage() + ptErr.GetExceptionFullStackTrace();
            StringBuilder sb = new ();
            if (!string.IsNullOrEmpty(message))
            {
                sb.Append(message);
                sb.Append(Environment.NewLine);
            }

            if (!string.IsNullOrEmpty(details))
            {
                sb.Append(Environment.NewLine);
                sb.Append(details);
            }
            throw new ImportException(sb.ToString());
        }
        catch (Exception exc)
        {
            string msg = Localizer.GetErrorString("2452", new object[] { exc.Message }, true);
            throw new ImportException(msg);
        }

        if (performImportResult == PerformImportResult.Busy)
        {
            throw new ImportException(Localizer.GetErrorString("2428", null, true));
        }

        if (performImportResult == PerformImportResult.Failed)
        {
            throw new ImportException(Localizer.GetErrorString("2429", null, true));
        }

        if (performImportResult == PerformImportResult.Started)
        {
            //No need to show a message here.  A tranmission will be received to indicate the import started.
        }
    }
}