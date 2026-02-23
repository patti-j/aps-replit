using PT.Common.Localization;

namespace PT.APSCommon.Range;

public class PTRangeException : PT.Common.Range.RangeException
{
    public PTRangeException(string a_message, object[] a_stringParameters = null, bool a_appendHelpUrl = false)
        : base(Localizer.GetErrorString(a_message, a_stringParameters, a_appendHelpUrl)) { }
}