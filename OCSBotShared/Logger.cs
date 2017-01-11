using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OCSBot.Shared
{
    public class Logger
    {
        public static void Info(string msg)
        {
            var log = $"[{DateTime.UtcNow}]{msg}";
            System.Diagnostics.Trace.TraceInformation(log);
            Console.WriteLine(log);
        }
    }
}
