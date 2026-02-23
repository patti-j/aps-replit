using RDotNet;

namespace ForecastServiceClient.ForecastLogic;

public class RAutoArima
{
    public class RInitializationException : Exception
    {
        public RInitializationException(string message, Exception innerException) : base(message, innerException) { }
    }

    public class ForecastCalculationException : Exception
    {
        public ForecastCalculationException(string message, Exception innerException) : base(message, innerException) { }
    }

    /// <summary>
    /// Returns an array of double values, representing the predicted values.
    /// </summary>
    /// <param name="a_observations">array of historical values</param>
    /// <param name="a_frequency">The periodicity (cycle of values) (e.g. monthly values with a seasonal cycle = 12)</param>
    /// <param name="a_horizon">The number of prediction steps to return</param>
    public static double[] AutoArima(AutoARIMAParameters a_params)
    {
        if (a_params == null)
        {
            throw new ArgumentNullException("a_params");
        }

        a_params.Validate();

        REngine rEngine = GetREngine();

        try
        {
            // create observations vector from historic demand 
            NumericVector observations = rEngine.CreateNumericVector(a_params.Observations);
            rEngine.SetSymbol("observations", observations);

            // create time series with frequency
            rEngine.Evaluate($"observationsTS <- ts(observations, frequency={a_params.Frequency})");

            // create arima model
            rEngine.Evaluate("modelArima <- auto.arima(observationsTS)");

            // run forecast
            rEngine.Evaluate($"result <- forecast(modelArima, h={a_params.Horizon})");

            // get forecast results
            NumericVector numeric = rEngine.Evaluate("result$mean").AsNumeric();
            return numeric.ToArray();
        }
        catch (Exception err)
        {
            throw new ForecastCalculationException("Error calculating forecast.", err);
        }
    }

    private static REngine s_rEngine;

    private static REngine GetREngine()
    {
        try
        {
            if (s_rEngine != null)
            {
                return s_rEngine;
            }

            REngine.SetEnvironmentVariables();
            s_rEngine = REngine.GetInstance();
            if (!s_rEngine.IsRunning)
            {
                s_rEngine.Initialize();
            }

            // load forecast package
            s_rEngine.Evaluate("library(forecast)");
            return s_rEngine;
        }
        catch (Exception err)
        {
            throw new RInitializationException("Error Initializing R.", err);
        }
    }

    #region R path -> PATH Environment Variables
    //const string c_rHome = @"C:\Program Files\R\R-3.4.0\";
    //const string c_r32bitPath = @"bin\i386";
    //const string c_r64bitPath = @"bin\x64";

    ///// <summary>
    ///// set environment variables if 
    ///// </summary>
    //static void SetEnvironmentPath()
    //{
    //    string requiredPath = GetRPath();
    //    if (!System.IO.Directory.Exists(requiredPath))
    //    {
    //        return;
    //    }
    //    string path = Environment.GetEnvironmentVariable("PATH");
    //    if (!path.Contains(requiredPath))
    //    {
    //        if (!path.EndsWith(";"))
    //        {
    //            path += ";";
    //        }
    //        path += requiredPath + ";";
    //        Environment.SetEnvironmentVariable("PATH", path);
    //    }
    //}

    //static string GetRPath()
    //{
    //    if (Environment.Is64BitProcess)
    //    {
    //        return c_rHome + c_r64bitPath;
    //    }
    //    return c_rHome + c_r32bitPath;
    //}
    #endregion
}