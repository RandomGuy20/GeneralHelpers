using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeneralHelpers.IPCommunications
{
    internal interface IIpCommunications
        : IDisposable
    {
        void Connect();
        void Disconnect();
        void SendData(byte[] data);
        void SendData(string data);


    }
}
