using System;

namespace PT.ServerManagerSharedLib.Exceptions
{
    public class ThumbprintMismatchException : Exception
    {
        public ThumbprintMismatchException(string a_message = "", object[] a_stringParameters = null, bool a_appendHelpUrl = false) { }
    }
}