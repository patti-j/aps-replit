namespace PT.ServerManagerSharedLib.Data;

/// <summary>
/// Stores 
/// </summary>
internal class SqlScriptHelper
{
    public string Script { get; set; }

    public Dictionary<string, object> SqlParams = new Dictionary<string, object>();

    /// <summary>
    /// Adds a param to the sql script.
    /// Use this to ensure values are properly parameterized for SQL, avoiding injection and improper reading of sql script properties.
    /// </summary>
    /// <param name="a_key">The name of the param. This is what is added to the script. Arbitrarily, but must be unique, start with an @, and better if descriptive.</param>
    /// <param name="a_value">The string value that will replace the param key.</param>
    /// <returns>The key arg (for convenience, so that it needn't be declared as a variable twice).</returns>
    public string SetSqlParam(string a_key, string a_value)
    {
        SqlParams.Add(a_key, a_value ?? string.Empty);
        return a_key;
    }

}