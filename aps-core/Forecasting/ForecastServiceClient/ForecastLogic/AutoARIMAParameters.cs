namespace ForecastServiceClient.ForecastLogic;

public class AutoARIMAParameters
{
    /// <summary>
    /// Time series for which forecast is required
    /// </summary>
    public double[] Observations { get; set; }

    /// <summary>
    /// The periodicity (cycle of values) (e.g. monthly values with a seasonal cycle = 12)
    /// </summary>
    public int Frequency { get; set; }

    /// <summary>
    /// Number of periods for forecasting
    /// </summary>
    public int Horizon { get; set; }

    /// <summary>
    /// </summary>
    /// <exception cref="InvalidAutoARIMAParametersException"></exception>
    public void Validate()
    {
        if (Observations == null || Observations.Length == 0)
        {
            throw new InvalidAutoARIMAParametersException("Observations cannot be null or empty array.");
        }

        if (Frequency <= 0)
        {
            throw new InvalidAutoARIMAParametersException("Frequency must be greater than zero.");
        }

        if (Horizon <= 0)
        {
            throw new InvalidAutoARIMAParametersException("Horizon must be greater than zero.");
        }
    }
}

public class InvalidAutoARIMAParametersException : Exception
{
    public InvalidAutoARIMAParametersException(string message) : base(message) { }
}