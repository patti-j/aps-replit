using Microsoft.AspNetCore.Mvc;
using PT.APIDefinitions;
using PT.Common.Http;

using PT.Scheduler;
using PT.APIDefinitions.RequestsAndResponses.Webapp;
using PT.APSCommon;
namespace PT.PlanetTogetherAPI.Controllers;
/// <summary>
/// Controller with an endpoints log exceptions and user activity to the server
/// </summary>
[ApiController]
[Route("api/[controller]")]
    public class LoggingController : SessionControllerBase
{
        //private readonly HashSet<string> m_ptExceptionCollection = new HashSet<string>();
        //public LoggingController()
        //{
        //    //TODO there may be a cleaner way to go about this but this works for now
        //    //The idea behind creating this collection is to be able to filter out known PTExceptions when logging to sentry
        //    //therefore if an exception is not in the collection then it is safe to log to sentry
            
        //    //Create a collection of PTExceptions

        //    //Attempt to get all exception types with the PT.Scheduler namespace
        //    foreach (Type type in typeof(JobManager.JobValidationException).Assembly.GetTypes())
        //    {
        //        if (typeof(PTException).IsAssignableFrom(type))
        //        {
        //            m_ptExceptionCollection.Add(type.FullName);
        //        }
        //    }

        //    //Attempt to get all exception types with the PT.APSCommon namespace
        //    foreach (Type type in typeof(PTException).Assembly.GetTypes())
        //    {
        //        if (typeof(PTException).IsAssignableFrom(type))
        //        {
        //            m_ptExceptionCollection.Add(type.FullName);
        //        }
        //    }
        //}
        /// <summary>
        /// Endpoint to receive exceptions from client and then logs the exceptions depending on its type to either Sentry or Web app
        /// </summary>
        /// <param name="a_request"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("exception")]
        public ActionResult<BoolResponse> LogValidationExceptions(LogErrorRequest a_request)
        {
            try
            {
                SystemController.Sys.SystemLoggerInstance.LogClientException(a_request);

                return Ok(new BoolResponse { Content = true });
            }
            catch (Exception)
            {
                return Ok(new BoolResponse { Content = false });
            }
        }
        /// <summary>
        /// Endpoint to receive and prepare a description of user activity to be sent to the Audit Logs
        /// </summary>
        /// <param name="a_userActivity"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("user-actions")]
        public ActionResult<BoolResponse> AuditLog(LogUserActionRequest a_request)
        {
            try
            {
                SystemController.WebAppActionsClient.LogUserAction(a_request);
                return Ok();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
    }
