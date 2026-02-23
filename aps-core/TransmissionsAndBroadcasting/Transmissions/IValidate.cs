using System.Collections;

using PT.APSCommon;

namespace PT.Transmissions;

/// <summary>
/// Summary description for IValidate.
/// </summary>
public interface IValidate
{
    void Validate();
}

public class ValidationException : PTValidationException
{
    public ValidationException()
        : this("") { }

    public ValidationException(string msg, object[] a_stringParameters = null, bool a_appendHelp = true)
        : base(msg, a_stringParameters, a_appendHelp) { }

    public ValidationException(string msg, object[] a_stringParameters, bool a_appendHelp, bool a_logToSentry)
        : base(msg, a_stringParameters, a_appendHelp)
    {
        // This constructor allows control over whether to log to Sentry.
        LogToSentry = a_logToSentry;
    }

    public static void ValidateArrayList(ArrayList al)
    {
        HashSet<string> externalIds = new ();

        for (int i = 0; i < al.Count; ++i)
        {
            PTObjectBase o = (PTObjectBase)al[i];

            if (externalIds.Contains(o.ExternalId))
            {
                throw new ValidationException("2774", new object[] { o.ExternalId, o.GetType().FullName });
            }

            o.Validate();

            externalIds.Add(o.ExternalId);
        }
    }
}