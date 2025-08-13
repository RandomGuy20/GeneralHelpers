using Crestron.SimplSharp;
using Crestron.SimplSharp.CrestronSockets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Channels;
using System.Text;
using System.Threading.Tasks;

namespace GeneralHelpers.IPCommunications.TCP
{
    internal class TCPIPClient: AIpCommunicationsBase
    {

        #region Fields

        #endregion

        #region Properties


        #endregion

        #region Delegates


        #endregion

        #region Events

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="TCPIPClient"/> class with the specified IP address, port,
        /// buffer size, and auto-reconnect option.
        /// </summary>
        /// <remarks>Use this constructor to create a TCP/IP client configured to connect to a specific
        /// server and port. The buffer size determines the maximum amount of data that can be read or written in a
        /// single operation.</remarks>
        /// <param name="IpAddress">The IP address of the server to connect to. This cannot be null or empty.</param>
        /// <param name="Port">The port number to connect to on the server. Must be a valid port number (0-65535).</param>
        /// <param name="BufferSize">The size of the buffer, in bytes, used for data transmission. The default value is 65535.</param>
        /// <param name="autoReconnect">A value indicating whether the client should automatically attempt to reconnect if the connection is lost.
        /// The default value is <see langword="false"/>.</param>
        public TCPIPClient(string IpAddress, int Port, int BufferSize = 65535, bool autoReconnect = false)
        {
            sender = this;
            ipAddress = IpAddress;
            port = Port;
            bufferSize = BufferSize;
            isReconnect = autoReconnect;
            tcpClient = new TCPClient(IpAddress, Port, BufferSize);
            tcpClient.SocketStatusChange += TcpClient_SocketStatusChange;
            
            
        }

        #endregion

        #region Internal Methods

        //private void ConnectionStateCallback(TCPClient _tcpClient)
        //{
        //    isConnected = tcpClient.ClientStatus == SocketStatus.SOCKET_STATUS_CONNECTED;

        //    SendStatusChangeEvent(this, tcpClient.ClientStatus.ToString(), IsConnected);

        //    if (tcpClient.ClientStatus == SocketStatus.SOCKET_STATUS_CONNECTED)
        //    {
        //        int dataAsync = (int)tcpClient.ReceiveDataAsync(DataReceivedCallback);
        //    }
                
        //}

        private void StatusCheck(object obj)
        {
            if (tcpClient.ClientStatus != SocketStatus.SOCKET_STATUS_CONNECTED && isReconnect)
                Connect();
        }

        //private void DataReceivedCallback(TCPClient _tcpClient, int bytes)
        //{
        //    if (bytes <= 0)
        //        return;

        //    SendDataReceivedEvent(this, Encoding.UTF8.GetString(tcpClient.IncomingDataBuffer, 0, bytes), tcpClient.IncomingDataBuffer);
        //}





        #endregion

        #region Public Methods

        public override void Connect()
        {
            if (isConnected)
                return;


            if (statusCheck != null)
            {
                statusCheck.IsRunning = false;
                statusCheck.Dispose();
            }

            int clientAsync = (int) tcpClient.ConnectToServerAsync(ConnectionStateCallback);

            statusCheck = new JTimer(StatusCheck, 500, 500);
        }

        public override void Disconnect()
        {
            if (!isConnected)
                return;
            tcpClient.DisconnectFromServer();

            statusCheck.IsRunning = false;
            statusCheck.Dispose();
        }

        //public override void SendData(string data)
        //{
        //    if (data.Length <= 0 || data == null)
        //        return;

        //    byte[] bytes = Encoding.ASCII.GetBytes(data);
        //    lastSent = data;
        //    errorCodes = tcpClient.SendDataAsync(bytes,bytes.Length, SendDataCallback);
        //}

        //public override void SendData(byte[] data)
        //{
        //    if (data == null || data.Length == 0)
        //        return;
        //    lastSent = Encoding.ASCII.GetString(data);
        //    errorCodes = tcpClient.SendDataAsync(data, data.Length, SendDataCallback);
        //}

        public override void Dispose()
        {
            tcpClient.Dispose();
        }

        #endregion
    }
}
