using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PT.Common.Debugging;

public static class PTConsole
{
    [Conditional("DEBUG")]
    public static void WriteLine(string a_message)
    {
        Console.WriteLine($"{DateTime.Now}: {a_message}");
    }
}