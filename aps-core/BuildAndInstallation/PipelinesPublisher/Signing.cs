using System.Diagnostics;
using System.Reflection;
using System.Text;

namespace PipelinesPublisher;

internal class Signing
{
    internal static void SignTool(Process a_tool, string a_targetPath, string a_signAppSecret)
    {
        StringBuilder sb = InitSignToolString(a_signAppSecret);

        sb.Append(" ");
        sb.Append(a_targetPath);
        a_tool.StartInfo.Arguments = sb.ToString(); //File to sign
        a_tool.Start();
        a_tool.WaitForExit();
        switch (a_tool.ExitCode)
        {
            case 0:
                Console.WriteLine("Successfully Signed " + a_targetPath);
                break;
            default:
                throw new Exception("Error signing " + a_targetPath);
        }
    }

    internal static void SignTool(Process a_tool, List<string> a_filePaths, string a_signAppSecret, string a_group)
    {
        if (a_filePaths.Count == 0)
        {
            throw new Exception($"There were no files to sing in group: {a_group}");
        }

        StringBuilder sb = InitSignToolString(a_signAppSecret);
        
        foreach (string filePath in a_filePaths)
        {
            sb.Append(" ");
            sb.Append(filePath);
        }

        a_tool.StartInfo.Arguments = sb.ToString(); //File to sign
        a_tool.Start();
        a_tool.WaitForExit();
        switch (a_tool.ExitCode)
        {
            case 0:
                Console.WriteLine($"Successfully Signed all {a_group} files!");
                break;
            default:
                throw new Exception($"Error signing at least one file in {a_group} :(");
        }
    }

    private static StringBuilder InitSignToolString(string a_signAppSecret)
    {
        StringBuilder sb = new();
        sb.Append("sign");
        sb.Append(" -kvu https://pt-codesign.vault.azure.net/");
        sb.Append(" -kvi e912f046-7dae-4a19-8b20-87f1f5d8a33d");
        sb.Append(" -kvt c41e2af3-6d94-41b8-9fa5-94c0d8dfcd99");
        sb.Append($" -kvs {a_signAppSecret}");
        sb.Append(" -kvc CodeSign2023");
        sb.Append(" -tr http://timestamp.digicert.com");
        sb.Append(" -td sha384");
        return sb;
    }

    internal static Process GetSignToolProcess()
    {
        Process signtool = new ();

        //Find signtool from the Nuget package output
        string rootPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        string[] files = Directory.GetFiles(rootPath, "AzureSignTool-x64.exe", SearchOption.TopDirectoryOnly);
        if (files.Length > 0)
        {
            signtool.StartInfo.FileName = files[0];
        }
        else
        {
            throw new Exception("AzureSignTool executable not found!");
        }

        signtool.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
        //signtool.StartInfo.Verb = "runas";

        return signtool;
    }
}