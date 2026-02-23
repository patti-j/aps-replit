using PT.Common.Exceptions;

namespace PT.Common.Localization;

internal class LocalizationException : PTException
{
    public LocalizationException(string a_msg)
        : base(a_msg) { }
}