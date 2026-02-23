using Microsoft.Data.SqlClient;

namespace PT.Common.Sql.SqlServer
{
    /// <summary>
    /// Used to collect the components of a sql command (script and parameters) so they can be used together when connecting to SQL.
    /// Params are optional, but useful to ensure that values are properly encoded and to avoid risk of injection.
    /// This class is useful when the schema of a command is static, but the values are dynamic.
    /// If used, the Param key should be hardcoded into the script, without wrapping quotes.
    /// </summary>
    public class SqlCommandHelper
    {
        public string CommandText { get; set; }

        private Dictionary<string, object> SqlParams = new Dictionary<string, object>();

        /// <summary>
        /// Adds a param to the sql script.
        /// </summary>
        /// <param name="a_key">The name of the param. This is what is added to the script. Arbitrarily, but must be unique, start with an @, and better if descriptive.</param>
        /// <param name="a_value">The string value that will replace the param key.</param>
        /// <returns>The key arg (for convenience, so that it needn't be declared as a variable twice).</returns>
        public string AddParam(string a_key, string a_value)
        {
            SqlParams.Add(a_key, a_value ?? string.Empty);
            return a_key;
        }

        public void SetCommand(SqlCommand a_command)
        {
            if (a_command == null)
            {
                throw new ArgumentException("Command must not be null");
            }

            a_command.Parameters.Clear();

            a_command.CommandText = CommandText;

            foreach (KeyValuePair<string, object> param in SqlParams)
            {
                a_command.Parameters.AddWithValue(param.Key, param.Value ?? DBNull.Value);
            }
        }
    }
}
