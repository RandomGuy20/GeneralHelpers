using Crestron.SimplSharp;
using Crestron.SimplSharp.CrestronSockets;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GeneralHelpers.IPCommunications.UDP
{
    public class WakeOnLan
    {
        #region Fields

        private EthernetAdapterType ethernetAdapterType;
        private SocketErrorCodes errorCodes;


        private byte[] wolpacket = new byte[1024];
        private byte[] macAddress = new byte[] { };

        private string pattern = @"^([0-9A-Fa-f]{2}[:-]){5}([0-9A-Fa-f]{2})$";
        private string ipAddress;

        private int port;


        private UDPServer udpServer;






        #endregion

        #region Properties

        public string MacAddress 
        {
            get
            {
                StringBuilder sBuild = new StringBuilder();
                if (macAddress?.Length <= 0 )
                {
                    return "";
                }

                foreach (byte b in macAddress)
                {
                    sBuild.Append(b.ToString("X2") + ":");
                }

                return sBuild.ToString();
            }
            set
            {
                macAddress = GetMacAddress(value);
            }
        }

        public  EthernetAdapterType EthernetAdapter
        {
            get => ethernetAdapterType;
            set => ethernetAdapterType = value;
        }



        #endregion

        #region Delegates

        public delegate void PacketSentEventHandler(bool sent);

        #endregion

        #region Events

        public event PacketSentEventHandler onPacketSentEvent;

        #endregion

        #region Constructors

        public WakeOnLan(string IpAddress, string MacAddress, int PortNumber, EthernetAdapterType adapter)
        {
            udpServer = new UDPServer(ipAddress, PortNumber, 65535);
            port = PortNumber;
            ipAddress = IpAddress;
            ethernetAdapterType = adapter;
            macAddress = GetMacAddress(MacAddress);
        }



        #endregion

        #region Internal Methods

        internal byte[] GetMacAddress(string macAddy)
        {
            if (string.IsNullOrEmpty(macAddy))
                return macAddress;

            if (!Regex.IsMatch(macAddy, pattern))
                return macAddress;

            string[] mac = macAddy.Split(new char[] { ':', '-' });        

            return mac.Select(bit => Convert.ToByte(bit, 16)).ToArray();
        }

        #endregion

        #region Public Methods

        public void SendPacket()
        {
            try
            {
                if (macAddress?.Length <= 1)
                    return;


                for (int i = 0; i < 6; i++)
                {
                    wolpacket[i] = 255;
                }

                for (int i = 7; i < 23; i++)
                     for (int j = 0; j < macAddress.Length; j++)
                        wolpacket[i] = macAddress[j];

                errorCodes = udpServer.EnableUDPServer("0.0.0.0", 0, port);
                udpServer.EthernetAdapterToBindTo = ethernetAdapterType;
                errorCodes = udpServer.SendData(wolpacket, wolpacket.Length, ipAddress, port, false);
                if (errorCodes == SocketErrorCodes.SOCKET_OK)
                    onPacketSentEvent?.Invoke(true);
                else
                    onPacketSentEvent?.Invoke(false);

                errorCodes = udpServer.DisableUDPServer();
            }
            catch (Exception e)
            {
                CrestronConsole.PrintLine($"WakeOnLan Dispose Exception: {e.Message}");
            }
        }

        public void Dispose()
        {
            try
            {
                udpServer?.Dispose();
                udpServer = null;
            }
            catch (Exception e)
            {
                CrestronConsole.PrintLine($"WakeOnLan Dispose Exception: {e.Message}");
            }
        }

        #endregion
    }
}
