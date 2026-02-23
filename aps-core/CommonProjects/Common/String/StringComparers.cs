namespace PT.Common;

public class StringComparers
{
    private static StringComparer s_externalIdComparer = StringComparer.CurrentCulture;

    /// <summary>
    /// For non-visual external id comparisons.
    /// </summary>
    public static StringComparer CaseSensitiveComparer
    {
        get => s_externalIdComparer;

        set => s_externalIdComparer = value;
    }

    private static StringComparer s_nameComparer = StringComparer.CurrentCultureIgnoreCase;

    /// <summary>
    /// For non-visual name comparisons.
    /// </summary>
    public static StringComparer CaseInsensitiveComparer
    {
        get => s_nameComparer;

        set => s_nameComparer = value;
    }
}