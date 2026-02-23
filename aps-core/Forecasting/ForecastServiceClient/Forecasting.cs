using System.Net.Http.Headers;
using System.Net.NetworkInformation;

using ForecastServiceClient.ForecastLogic;

using Newtonsoft.Json;

using PT.Common.Exceptions;

namespace ForecastServiceClient;

public class Forecasting
{
    public class ForecastServiceException : PTHandleableException
    {
        public ForecastServiceException(string a_message, object[] a_stringParameters = null, bool a_appendHelpUrl = false)
            : base(a_message, a_stringParameters, a_appendHelpUrl) { }

        public ForecastServiceException(string a_message, Exception a_innerException, object[] a_stringParameters = null, bool a_appendHelpUrl = false)
            : base(a_message, a_innerException, a_stringParameters, a_appendHelpUrl) { }
    }

    private const string c_forecastingServiceUrl = "https://forecastservice.planettogether.com/";
    private static bool s_noLocalRFound;

    /// <summary>
    /// Calculates forecasts using the AutoARIMA algorithm implemented in the forecasts package of R. It tries to
    /// perform the calculation locally, but if R isn't installed, it will call PTForecastingService.
    /// </summary>
    /// <param name="a_observations">array of historical values</param>
    /// <param name="a_frequency">The periodicity (cycle of values) (e.g. monthly values with a seasonal cycle = 12)</param>
    /// <param name="a_horizon">The number of prediction steps to return</param>
    /// <returns>Returns an array of double values, representing the predicted values.</returns>
    public static async Task<double[]> Forecast(double[] a_observations, int a_frequency, int a_horizon)
    {
        AutoARIMAParameters autoArimaParams = new () { Observations = a_observations, Frequency = a_frequency, Horizon = a_horizon };
        autoArimaParams.Validate();

        double[] forecast = null;
        if (!s_noLocalRFound)
        {
            try
            {
                forecast = await Task.Run(() => { return RAutoArima.AutoArima(autoArimaParams); });
            }
            catch (RAutoArima.RInitializationException)
            {
                s_noLocalRFound = true;
            }
            catch (Exception err)
            {
                throw new ForecastServiceException("2895", err);
            }
        }

        if (forecast == null)
        {
            forecast = await ForecastUsingForecastingService(autoArimaParams);
        }

        return forecast;
    }

    private static async Task<double[]> ForecastUsingForecastingService(AutoARIMAParameters a_params)
    {
        try
        {
            using (HttpClientHandler handler = new ())
            {
                using (HttpClient client = new (handler))
                {
                    client.BaseAddress = new Uri(c_forecastingServiceUrl);
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("CustomAuth", "JMm7VT7aX9PzYgwK");
                    HttpContent content = new StringContent(JsonConvert.SerializeObject(a_params), System.Text.Encoding.UTF8, "application/json");
                    HttpResponseMessage msg = await client.PostAsync("api/autoarima", content);
                    msg.EnsureSuccessStatusCode();
                    string result = await msg.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<double[]>(result);
                }
            }
        }
        catch (Exception err)
        {
            throw new ForecastServiceException("2895", err);
        }
    }

    /// <summary>
    /// Pings the service connection
    /// </summary>
    /// <returns>True if ping is successful</returns>
    public static bool TestServiceConnection()
    {
        try
        {
            Ping ping = new ();
            PingReply reply = ping.Send(c_forecastingServiceUrl);
            return reply.Status == IPStatus.Success;
        }
        catch (Exception e)
        {
            return false;
        }
    }
}