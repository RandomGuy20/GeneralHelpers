using Crestron.SimplSharp;
using Crestron.SimplSharp.CrestronSockets;
using Crestron.SimplSharp.CrestronWebSocketClient;
using Crestron.SimplSharp.Ssh;
using GeneralHelpers.IPCommunications.TCP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeneralHelpers.IPCommunications
{
    public abstract class AIpCommunicationsBase
    {


        //TCP Fields -- Ip Address, connected, port, autoreconnect,Debug,
        //UDP Fields -- Ipaddress, port, bufferSize,Debug, isInitialized, isEnabled
        //WakeOnLan Fields -- macaddress, ip address, port, ethernetadaptertype, 
        //Telnet Fields -- buffersize, port, ipaddress,username,password,connected,autoreconnect,
        // SSH Fields -- initialized, username, ipaddress, password, connected, 

        //TCp methods -- Connect, Disconnect, Send Data(byte), senddata(string) , Dispose
        // UDP methods --- Enable UDp, Disable UDP, Send Bytes, Send String, Dispose, 
        // Wake On lan Methods -- sendpakcte, dispose
        //Telnet Methods --- connect, disconnect, send data(byte), send data(string), dispose
        //SSh Methods -- connect, disconnect, send data(byte), send data(string), dispose


        // TCP events - Connection state, datareceived
        //UDp events -- datareceived, udp enabled
        //Wake On Lan Events -- wakeonlan
        //Tenet events --data received, connection state
        //SSh events -- connection state, data received



        #region Fields

        internal TCPClient tcpClient;
        internal TCPServer tcpServer;
        internal UDPServer udpServer;
        internal SshClient sshClient;
        internal ShellStream sshShellStream;
        internal WebSocketClient webSocketClient;

        internal JTimer statusCheck;
        internal SocketErrorCodes errorCodes;


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

        public StatusChangedEventHandler onStatusChangeEvent;

        public IncomingDataEventHandler onDataReceivedEvent;

        #endregion

        #region Constructors



        #endregion

        #region Internal Methods

        internal void SendStatusChangeEvent(object sender, string statusMessage, bool connectionState)
        {
            onStatusChangeEvent?.Invoke(sender, statusMessage, connectionState);
        }

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

        internal virtual void DataReceivedCallback( TCPClient _tcpClient, int bytes)
        {
            if (bytes <= 0)
                return;

            SendDataReceivedEvent(sender, Encoding.UTF8.GetString(tcpClient.IncomingDataBuffer, 0, bytes), tcpClient.IncomingDataBuffer);
        }



        internal virtual void SetUserName(string user)
        {
            if (string.IsNullOrEmpty(userName))
                return;
            userName = user;
        }

        internal virtual void SetPassword(string pass)
        {
            if (string.IsNullOrEmpty(password))
                return;
            password = pass;
        }

        internal void TcpClient_SocketStatusChange(TCPClient myTCPClient, SocketStatus clientSocketStatus)
        {
            isConnected = clientSocketStatus == SocketStatus.SOCKET_STATUS_CONNECTED;

            SendStatusChangeEvent(this, tcpClient.ClientStatus.ToString(), IsConnected);
            if (isConnected)
            {
                int dataAsync = (int)tcpClient.ReceiveDataAsync(DataReceivedCallback);
            }
        }
        internal void ConnectionStateCallback(TCPClient _tcpClient)
        {
            isConnected = tcpClient.ClientStatus == SocketStatus.SOCKET_STATUS_CONNECTED;

            SendStatusChangeEvent(this, tcpClient.ClientStatus.ToString(), IsConnected);

            if (tcpClient.ClientStatus == SocketStatus.SOCKET_STATUS_CONNECTED)
            {
                int dataAsync = (int)tcpClient.ReceiveDataAsync(DataReceivedCallback);
            }

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

        /// <summary>
        /// This will enable UDP communications
        /// This will connect for Telnet and TCP
        /// </summary>
        public abstract void Connect();

        /// <summary>
        /// This will disable UDP communications
        /// this will disconnect for Telnet and TCP
        /// </summary>
        public abstract void Disconnect();

        public virtual void SendData(byte[] data)
        {
            if (data == null || data.Length == 0)
                return;
            lastSent = Encoding.ASCII.GetString(data);
            errorCodes = tcpClient.SendDataAsync(data, data.Length, SendDataCallback);
        }


        public virtual void SendData(string data)
        {
            if (data.Length <= 0 || data == null)
                return;

            byte[] bytes = Encoding.ASCII.GetBytes(data);
            lastSent = data;
            errorCodes = tcpClient.SendDataAsync(bytes, bytes.Length, SendDataCallback);
        }

        public abstract void Dispose();

        #endregion


    }
}
