namespace PT.APSCommon.ProgramArguments;

/// <summary>
/// Represents a single program argument.
/// </summary>
public class Argument
{
    /// <summary>
    /// Define a program argument
    /// </summary>
    /// <param name="a_name">The name of the program argument. It will be searched for using the following format with the first format parameter being replaced by the value of name: \{0}:</param>
    /// <param name="a_valueAfterNameRequirement">Whether a value after the colon in the format is required, optional, or can't be specified.</param>
    public Argument(string a_name, EValueAfterNameRequirement a_valueAfterNameRequirement)
        : this(a_name, a_valueAfterNameRequirement, null) { }

    public Argument(string a_name, EValueAfterNameRequirement a_valueAfterNameRequirement, params string[] a_aliasNames)
    {
        Name = a_name;
        ValueRequirement = a_valueAfterNameRequirement;
        ConstructHelper(a_name, a_aliasNames);
    }

    public Argument(string a_name, string a_defaultValue, EValueAfterNameRequirement a_valueAfterNameRequirement)
        : this(a_name, a_defaultValue, a_valueAfterNameRequirement, null) { }

    public Argument(string a_name, string a_defaultValue, EValueAfterNameRequirement a_valueAfterNameRequirement, params string[] a_aliasNames)
        : this(a_name, a_valueAfterNameRequirement, null)
    {
        SetDefaultValue(a_defaultValue);
    }

    private void ConstructHelper(string a_primaryName, params string[] a_aliases)
    {
        int argsAliasCount = 0;
        if (a_aliases != null)
        {
            argsAliasCount = a_aliases.Length;
        }

        int totalAliases = 1 + argsAliasCount;
        Aliases = new string[totalAliases];
        Aliases[0] = a_primaryName;
        if (a_aliases != null)
        {
            for (int aliasI = 0; aliasI < a_aliases.Length; ++aliasI)
            {
                Aliases[aliasI + 1] = a_aliases[aliasI];
            }
        }
    }

    private string m_name;

    /// <summary>
    /// The string representation of the argument's value (if the argument has a value).
    /// </summary>
    public string Name
    {
        get => m_name;
        set
        {
            if (string.IsNullOrEmpty(value))
            {
                throw new Exception("The program argument name can't be null or the empty string.");
            }

            m_name = value;
        }
    }

    private string[] Aliases { get; set; }

    public EValueAfterNameRequirement ValueRequirement { get; private set; }

    /// <summary>
    /// Whether the program argument was specified in the list of program arguments.
    /// </summary>
    public bool ArgumentFound { get; set; }

    public bool DefaultValue { get; set; }

    private bool m_valueSet;
    private string m_value;

    /// <summary>
    /// The value of the program arguments.
    /// </summary>
    public string Value
    {
        get
        {
            if (!ArgumentFound && !DefaultValue)
            {
                throw new PTArgumentException($"You can't access the value because Argument '{Name}' wasn't found and there was no default value.");
            }

            if (ValueRequirement == EValueAfterNameRequirement.NoValue)
            {
                throw new PTArgumentException($"You can't access the value because Argument '{Name}' doesn't accept a value.");
            }

            return m_value;
        }

        set
        {
            m_value = value;
            ValueSet(m_value);
            m_valueSet = true;
        }
    }

    private void SetDefaultValue(string a_defaultValue)
    {
        m_value = a_defaultValue;
        DefaultValue = true;
        ValueSet(a_defaultValue);
    }

    protected virtual void ValueSet(string a_value) { }

    public override string ToString()
    {
        string msg;
        string format = "Name: {0}; ValueRequirement: {1}; ArgumentFound: {2}";

        if (m_valueSet && Value != null)
        {
            format += "; Value: {3}";
            msg = string.Format(format, Name, ValueRequirement.ToString(), ArgumentFound, Value);
        }
        else if (DefaultValue && Value != null)
        {
            format += "; Value Not Specified; Default Specified: {3}";
            msg = string.Format(format, Name, ValueRequirement.ToString(), ArgumentFound, Value);
        }
        else
        {
            msg = string.Format(format, Name, ValueRequirement.ToString(), ArgumentFound);
        }

        return msg;
    }

    internal bool ArgumentMatch(string a_argName)
    {
        if (string.Compare(a_argName, Name, true) == 0)
        {
            return true;
        }

        foreach (string alias in Aliases)
        {
            if (string.Compare(a_argName, alias, true) == 0)
            {
                return true;
            }
        }

        return false;
    }
}