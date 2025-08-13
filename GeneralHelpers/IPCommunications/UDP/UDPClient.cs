using Crestron.SimplSharp.CrestronSockets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeneralHelpers.IPCommunications
{
    internal class UDPClient: AIpCommunicationsBase
    {
        #region Fields

        private bool isInitialized;
        private bool isEnabled;




        #endregion

        #region Properties
        public bool IsInitialized => isInitialized;

        public bool IsEnabled => isEnabled;

        #endregion

        #region Delegates

        public delegate void UDPSendDataEventHandler(bool success);
        public delegate void UDPClientEnabledEventHandler(bool enabled);

        #endregion

        #region Events

        public event UDPSendDataEventHandler onUDPSentDataEvent;
        public event UDPClientEnabledEventHandler onUDPClientEnabledEvent;

        #endregion

        #region Constructors

        public UDPClient(string IpAddress, int Port, int BufferSize = 65535)
        {
            bufferSize = BufferSize;
            port = Port;
            ipAddress = IpAddress;
            isEnabled = false;
            udpServer = new Crestron.SimplSharp.CrestronSockets.UDPServer(ipAddress, port, bufferSize);

        }



        #endregion

        #region Internal Methods

        private void UDPClientReceivedDataCallback(UDPServer server, int numberOfBytesReceived)
        {
            try
            {
                onDataReceivedEvent?.Invoke(this,
                    Encoding.UTF8.GetString(server.IncomingDataBuffer,0,numberOfBytesReceived), server.IncomingDataBuffer);
            }
            catch (Exception e)
            {
                SendDebug($"UDP Client Received Data Exception: {e.Message}");
            }

        }

        private void UDPClientSentDataCallback(UDPServer myUDPServer, int numberOfBytesSent)
        {
            onUDPSentDataEvent?.Invoke(numberOfBytesSent > 0);
        }

        #endregion

        #region Public Methods

        public override void Connect()
        {
            try
            {
                errorCodes = udpServer.EnableUDPServer(ipAddress, port);
                if(errorCodes != SocketErrorCodes.SOCKET_OK)
                {
                    SendDebug($"UDP Client Enable Error: {errorCodes}");
                    return;
                }

                isEnabled = true;
                onUDPClientEnabledEvent?.Invoke(isEnabled);
                errorCodes = udpServer.ReceiveDataAsync(UDPClientReceivedDataCallback);
            }
            catch (Exception e)
            {
                SendDebug($"UDP Client Connect Exception: {e.Message}");
            }
        }

        public override void Disconnect()
        {
            SocketErrorCodes err = udpServer.DisableUDPServer();

            isEnabled = false;
            onUDPClientEnabledEvent?.Invoke(isEnabled);
        }

        public override void SendData(byte[] data)
        {
            if (!isEnabled)
                return;

            errorCodes = udpServer.SendDataAsync(data, data.Length,ipAddress,
                port, UDPClientSentDataCallback);
        }

        public override void SendData(string data)
        {
            errorCodes = udpServer.SendDataAsync(Encoding.UTF8.GetBytes(data), data.Length, ipAddress,
                port, UDPClientSentDataCallback);
        }

        public override void Dispose()
        {
            Disconnect();
            udpServer.Dispose();
        }

        #endregion
    }
}
