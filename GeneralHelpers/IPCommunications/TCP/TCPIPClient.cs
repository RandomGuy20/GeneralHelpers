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
    public class TCPIPClient
    {

        #region Fields

        internal TCPClient tcpClient;

        internal string ipAddress;
        internal string lastSent;
        internal string userName;
        internal string password;
        internal string url;

        internal int port;
        internal int bufferSize;
        internal int maxConnections = 10;

        internal bool isConnected = false;
        internal bool isReconnect;

        internal object sender;

        internal JTimer statusCheck;
        internal SocketErrorCodes errorCodes;


        #endregion

        #region Properties

        public string IpAddress
        {
            get => ipAddress;
            set => IPAddressChange(value);
        }
        public int Port
        {
            get => port;
            set => IpPortChange(value);
        }

        public bool IsConnected => isConnected;

        public bool AutoReconnect
        {
            get => isReconnect;
            set => isReconnect = value;
        }



        public bool Debug { get; set; } = false;

        #endregion

        #region Delegates

        public delegate void StatusChangedEventHandler(object sender, string statusMessage, bool ConnectionState);

        public delegate void IncomingDataEventHandler(object sender, string sData, byte[] bData);

        #endregion

        #region Events

        public event StatusChangedEventHandler onStatusChangeEvent;

        public event IncomingDataEventHandler onDataReceivedEvent;

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

        private void TcpClient_SocketStatusChange(TCPClient myTCPClient, SocketStatus clientSocketStatus)
        {
            isConnected = clientSocketStatus == SocketStatus.SOCKET_STATUS_CONNECTED;

            SendStatusChangeEvent(this, myTCPClient.ClientStatus.ToString(), IsConnected);
            //if (isConnected)
            //{
                int dataAsync = (int)myTCPClient.ReceiveDataAsync(DataReceivedCallback);
            //}
        }

        #endregion

        #region Internal Methods

        internal void SendDataReceivedEvent(object sender, string sData, byte[] bData)
        {
            onDataReceivedEvent?.Invoke(sender, sData, bData);
        }

        internal void IPAddressChange(string newIpAddress)
        {
            Disconnect();
            ipAddress = newIpAddress;
        }

        internal void IpPortChange(int newPort)
        {
            Disconnect();
            port = newPort;
        }

        internal void SendStatusChangeEvent(object sender, string statusMessage, bool connectionState)
        {
            onStatusChangeEvent?.Invoke(sender, statusMessage, connectionState);
        }

        private void ConnectionStateCallback(TCPClient _tcpClient)
        {
            isConnected = _tcpClient.ClientStatus == SocketStatus.SOCKET_STATUS_CONNECTED;

            SendStatusChangeEvent(this, _tcpClient.ClientStatus.ToString(), isConnected);

            //if (tcpClient.ClientStatus == SocketStatus.SOCKET_STATUS_CONNECTED)
            //{
                int dataAsync = (int)_tcpClient.ReceiveDataAsync(DataReceivedCallback);
            //}

        }

        private void StatusCheck(object obj)
        {
            if (tcpClient.ClientStatus != SocketStatus.SOCKET_STATUS_CONNECTED && isReconnect)
                Connect();
        }

        private void DataReceivedCallback(TCPClient _tcpClient, int bytes)
        {
            //if (bytes <= 0)
            //    return;

           // CrestronConsole.PrintLine($"Received {bytes} bytes from server.");

            SendDataReceivedEvent(this, Encoding.UTF8.GetString(_tcpClient.IncomingDataBuffer, 0, bytes), tcpClient.IncomingDataBuffer);
            int dataAsync = (int)_tcpClient.ReceiveDataAsync(DataReceivedCallback);
        }


        internal void SendDataCallback(TCPClient tcpClient, int bytes)
        {
            SendDebug($"TCP Data Sent was: {lastSent} ");
        }
        internal void SendDebug(string data)
        {
            if (Debug)
            {
                CrestronConsole.PrintLine($"\nTCP Client Debug Message is: {data}");
                ErrorLog.Error($"\nTCP Client Debug Message is: {data}");
            }
        }

        #endregion

        #region Public Methods

        public void Connect()
        {
            if (isConnected)
                return;


            if (statusCheck == null)
                statusCheck = new JTimer(StatusCheck, 500, 500);

            int clientAsync = (int) tcpClient.ConnectToServerAsync(ConnectionStateCallback);


        }
        public void Disconnect()
        {
            if (!isConnected)
                return;
            tcpClient.DisconnectFromServer();

            statusCheck.IsRunning = false;
            statusCheck.Dispose();
        }
        public void SendData(string data)
        {
            if (data.Length <= 0 || data == null)
                return;

            byte[] bytes = Encoding.ASCII.GetBytes(data);
            lastSent = data;
            errorCodes = tcpClient.SendDataAsync(bytes, bytes.Length, SendDataCallback);
        }
        public void SendData(byte[] data)
        {
            if (data == null || data.Length == 0)
                return;
            lastSent = Encoding.ASCII.GetString(data);
            errorCodes = tcpClient.SendDataAsync(data, data.Length, SendDataCallback);
        }
        public  void Dispose()
        {
            tcpClient.Dispose();
        }

        #endregion
    }
}
