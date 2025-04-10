using Crestron.SimplSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeneralHelpers.Debugging
{
    public class DebugErrorTracker
    {

        public bool Debug = false;
        public void SendDebug(string data)
        {
            if (Debug)
                CrestronConsole.PrintLine("\nDebugging Message is: " + data);

        }

        public void SendErrorLog(string data)
        {
            if (Debug)
                ErrorLog.Error("\nError Message is: " + data);
        }


    }
}
