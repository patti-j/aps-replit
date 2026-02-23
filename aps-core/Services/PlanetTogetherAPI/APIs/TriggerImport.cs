using System.Net;

using PT.APIDefinitions.RequestsAndResponses;
using PT.APSCommon;
using PT.APSCommon.Exceptions;
using PT.Common.Exceptions;
using PT.Common.Localization;
using PT.Common.Sql.Exceptions;
using PT.ERPTransmissions;
using PT.ImportDefintions;
using PT.ImportDefintions.RequestsAndResponses;
using PT.Scheduler;
using PT.Transmissions;

namespace PT.PlanetTogetherAPI.APIs;

internal class TriggerImport : ImportExportActionBase
{
    internal TriggerImport()
    {
        InitializeListeners();
    }

    internal ApsWebServiceResponseBase RunERPImport(ISystemServiceClient a_systemActionsClient, string a_userName, BaseId a_targetScenarioId, BaseId a_instigator)
    {
        PerformImportResult importResult;

        try
        {
            PerformImportRequest performImportRequestRequest = new ()
            {
                TestOnly = false,
                UserName = a_userName,
                Instigator = SystemController.CurrentUserId.Value,
                SpecificScenarioId = a_targetScenarioId.Value
            };

            importResult = a_systemActionsClient.RunImport(performImportRequestRequest);
        }
        catch (WebException e)
        {
            string msg = Localizer.GetErrorString("2444", new object[] { e.Message }, true);
            return new ApsWebServiceResponseBase(EApsWebServicesResponseCodes.Failure, msg);
        }
        catch (InterfaceLockedException lockedErr)
        {
            string msg = Localizer.GetErrorString("2446", new object[] { lockedErr.lockUser, lockedErr.lockTime }, true);
            return new ApsWebServiceResponseBase(EApsWebServicesResponseCodes.Failure, msg);
        }
        catch (ImporterConnectionException connectionErr)
        {
            string msg = Localizer.GetErrorString("2447", new object[] { connectionErr.Message }, true);
            return new ApsWebServiceResponseBase(EApsWebServicesResponseCodes.Failure, msg);
        }
        catch (ImporterCommandException commandeErr)
        {
            string msg = Localizer.GetErrorString("2448", new object[] { commandeErr.Message }, true);
            return new ApsWebServiceResponseBase(EApsWebServicesResponseCodes.Failure, msg);
        }
        catch (ImporterInvalidConnectionException invalidConnErr)
        {
            string msg = Localizer.GetErrorString("2449", new object[] { invalidConnErr.Message }, true);
            return new ApsWebServiceResponseBase(EApsWebServicesResponseCodes.Failure, msg);
        }
        catch (ImporterException ie)
        {
            string msg = Localizer.GetErrorString("2450", new object[] { ie.Message }, true);
            return new ApsWebServiceResponseBase(EApsWebServicesResponseCodes.Failure, msg);
        }
        catch (ValidationException vErr)
        {
            string msg = Localizer.GetErrorString("2451", new object[] { vErr.Message }, true);
            return new ApsWebServiceResponseBase(EApsWebServicesResponseCodes.Failure, msg);
        }
        catch (CommonException ptErr)
        {
            if (ptErr is PTValidationException ||
                ptErr is PTDatabaseException ||
                ptErr is PTHandleableException)
            {
                string msg = Localizer.GetErrorString("2444", new object[] { ptErr.Message });
                return new ApsWebServiceResponseBase(EApsWebServicesResponseCodes.Failure, msg);
            }

            throw;
        }
        catch (Exception exc)
        {
            string msg = Localizer.GetErrorString("2452", new object[] { exc.Message }, true);
            return new ApsWebServiceResponseBase(EApsWebServicesResponseCodes.Failure, msg);
        }

        if (importResult == PerformImportResult.Busy)
        {
            string msg = Localizer.GetErrorString("2428", null, true);
            return new ApsWebServiceResponseBase(EApsWebServicesResponseCodes.Failure, msg);
        }

        if (importResult == PerformImportResult.Failed)
        {
            string msg = Localizer.GetErrorString("2429", null, true);
            return new ApsWebServiceResponseBase(EApsWebServicesResponseCodes.Failure, msg);
        }

        return new ApsWebServiceResponseBase(EApsWebServicesResponseCodes.SuccessWithoutValidation, string.Empty);
    }

    private void InitializeListeners()
    {
        SystemController.Sys.PerformImportStartedEvent += new PTSystem.PerformImportStartedDelegate(Sys_PerformImportStartedEvent);
        SystemController.Sys.PerformImportCompletedEvent += new PTSystem.PerformImportCompletedDelegate(Sys_PerformImportCompletedEvent);
    }

    protected override void DeInitializeListeners()
    {
        SystemController.Sys.PerformImportStartedEvent -= new PTSystem.PerformImportStartedDelegate(Sys_PerformImportStartedEvent);
        SystemController.Sys.PerformImportCompletedEvent -= new PTSystem.PerformImportCompletedDelegate(Sys_PerformImportCompletedEvent);
    }

    private void Sys_PerformImportCompletedEvent(PerformImportCompletedT a_t)
    {
        lock (m_dataLock)
        {
            m_finished = true;
            m_started = false;
            m_exceptions = a_t.Exceptions;
        }
    }

    private void Sys_PerformImportStartedEvent(PerformImportStartedT a_t)
    {
        lock (m_dataLock)
        {
            m_finished = false;
            m_started = true;
            m_exceptions = null;
        }
    }
}