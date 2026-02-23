using PT.APSCommon;
using PT.APSCommon.Extensions;
using PT.Common.Exceptions;

namespace PT.PackageDefinitions;

public class PackageException : PTException
{
    public PackageException(IPackageModule a_module, string a_message, object[] a_stringParameters = null, bool a_appendHelpUrl = true)
        : base(a_message + Environment.NewLine + $"Module: '{a_module.GetType()}'".Localize(), a_stringParameters, a_appendHelpUrl) { }

    public PackageException(string a_message, object[] a_stringParameters = null, bool a_appendHelpUrl = true)
        : base(a_message, a_stringParameters, a_appendHelpUrl) { }

    public PackageException(string a_message, Exception a_innerException, object[] a_stringParameters = null, bool a_appendHelpUrl = true)
        : base(a_message, a_innerException, a_stringParameters, a_appendHelpUrl) { }
}