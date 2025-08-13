using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeneralHelpers.IPCommunications
{
    internal interface ISecurityBase
    {
        string UserName { get; set; }
        string Password { get; set; }
    }
}
