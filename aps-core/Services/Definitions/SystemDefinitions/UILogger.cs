using PT.APIDefinitions.RequestsAndResponses.Webapp;
using PT.APSCommon;
using PT.Common.File;
using PT.Common.Http;
using PT.SystemDefinitions.Interfaces;
using PT.Transmissions.Interfaces;

namespace PT.SystemDefinitions
{
    /// <summary>
    /// UI Logger this is a dedicated logger to the UI to always make sure all UI exceptions are sent the server or logged to the client if it fails
    /// </summary>
    public class UILogger : ICommonLogger
    {
        private readonly IClientSession m_clientSession; //Used to make API requests to the Instance's LoggingController
        private readonly BaseId m_currentUserId; //This value is passed into the API request so that the Instance can track the source of the logging request
        /// <summary>
        /// UI Logger this is a dedicated logger to the UI to always make sure all UI exceptions are sent the server or logged to the client if it fails
        /// </summary>
        /// <param name="a_clientSession"></param>
        /// <param name="a_currentUserId"></param>
        public UILogger(IClientSession a_clientSession, BaseId a_currentUserId)
        {
            m_clientSession = a_clientSession;
            m_currentUserId = a_currentUserId;
        }
        public void LogException(Exception a_e, ScenarioExceptionInfo a_sei, bool a_logToSentry, ELogClassification a_logClassification)
        {
            try
            {
                if (m_clientSession == null)
                {
                    //TODO: Maybe log in debug
                    return;
                }
                string headerMessage = string.Empty;
                LogErrorRequest exceptionLogRequest = new LogErrorRequest(m_currentUserId.Value, a_e, ELogClassification.Fatal.ToString(), headerMessage, PTDateTime.UserDateTimeNow.ToServerTime(), a_logToSentry);
                //Send the exception to the server since this happened on the client
                BoolResponse boolResponse = m_clientSession.MakePostRequest<BoolResponse>("exception", exceptionLogRequest, "api/Logging");

                if (!boolResponse.Content)
                {
                    //TODO: Handle Logging Failure
                    //Handle failure to server exception from server to web app or sentry
                    //Either Log to event viewer, disk or retry
                }
            }
            catch (Exception e)
            {
                //TODO: Log to disk? Error happened Logging to to server
            }
            
        }
    }
}
