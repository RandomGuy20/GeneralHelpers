using Crestron.SimplSharp.CrestronSockets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeneralHelpers.IPCommunications.Telnet
{
    internal class TelnetClient:AIpCommunicationsBase, ISecurityBase
    {
        #region Fields


        private string loginFlag;

        private bool loggedIN;





        #endregion

        #region Properties

        public string UserName 
        {
            get => userName;
            set => SetUserName(value);

        }

        public string Password
        {
            get => userName;
            set => SetPassword(value);

        }

        public bool LoggedIn => loggedIN;


        #endregion

        #region Delegates

        public delegate void TelnetClientLoggedIn(bool state);

        #endregion

        #region Events

        public event TelnetClientLoggedIn onTelnetClientLoggedIn;

        #endregion

        #region Constructors

        public TelnetClient(string UserName, string Password, string IpAddress, string LoginSuccessFlag, bool Reconnect = false)
        {
            port = 23;
            SetUserName(UserName);
            SetPassword(Password);
            ipAddress = IpAddress;
            loginFlag = LoginSuccessFlag;
            bufferSize = 1024;
            isReconnect = Reconnect;
            tcpClient = new TCPClient(IpAddress, port, bufferSize);
            tcpClient.SocketStatusChange += TcpClient_SocketStatusChange;
        }





        #endregion

        #region Internal Methods

        //private void SetUserName(string user)
        //{
        //    if (string.IsNullOrEmpty(userName))
        //        return;
        //    userName = user;
        //}

        //private void SetPassword(string pass)
        //{
        //    if (string.IsNullOrEmpty(password))
        //        return;
        //    password = pass;
        //}

        internal override void DataReceivedCallback(TCPClient _tcpClient, int bytes)
        {
            string lower = Encoding.UTF8.GetString(tcpClient.IncomingDataBuffer, 0, bytes).ToLower();

            if (string.IsNullOrEmpty(lower))
                return;

            if (lower.Contains("login:"))
            {
                SendData(userName + "\r\n");
                return;
            }
            else if (lower.Contains("password:"))
            {
                SendData(password + "\r\n");
                return;
            }
            else if (lower.Contains("logout"))
            {
                loggedIN = false;
                onTelnetClientLoggedIn?.Invoke(loggedIN);
                if (isReconnect)
                    Connect();
                return;
            }
            else if (lower.Contains(loginFlag))
            {
                loggedIN = true;
                onTelnetClientLoggedIn?.Invoke(loggedIN);
                return;
            }
            else if(lower.Length > 0)
            {
                SendDataReceivedEvent(this,lower, null);
            }
        }


        private void StatusCheck(object obj)
        {
            if (tcpClient.ClientStatus != SocketStatus.SOCKET_STATUS_CONNECTED && isReconnect)
                Connect();
        }

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

            int clientAsync = (int)tcpClient.ConnectToServerAsync(ConnectionStateCallback);

            statusCheck = new JTimer(StatusCheck, 500, 500);
        }

        public override void Disconnect()
        {
            tcpClient.DisconnectFromServer();
        }





        public override void Dispose()
        {
            tcpClient.Dispose();
        }

        #endregion
    }
}
