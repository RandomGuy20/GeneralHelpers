using Crestron.SimplSharp.CrestronSockets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeneralHelpers.IPCommunications.Telnet
{

    public class TelnetClient:AIpCommunicationsBase, ISecurityBase
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
            SendDebug($"Incoming TelnetDataReceivgedCallback is {lower}");

            if (string.IsNullOrEmpty(lower))
                return;




            var actionMap = new Dictionary<Func<string, bool>, Action>()
            {
                {s => s.Contains("login:"),() => SendData(userName + "\r\n") },
                {s => s.Contains("password:"),() => SendData(password + "\r\n") },
                {s => s.Contains("logout"),( ) => HandleLoginStatusChange(false) },
                {s => s.Contains(loginFlag),() => HandleLoginStatusChange(true) },
                {s => s.Length > 0,() => SendDataReceivedEvent(this,lower, null)},
            };


            foreach (var action in actionMap)
            {
                if (action.Key(lower))
                {
                    action.Value.Invoke();
                    return;
                }
            }
        }


        private void StatusCheck(object obj)
        {
            if (tcpClient.ClientStatus != SocketStatus.SOCKET_STATUS_CONNECTED && isReconnect)
                Connect();
        }

        private void HandleLoginStatusChange(bool state)
        {
            loggedIN = state;
           
            if (isReconnect && !state)
                Connect();


            onTelnetClientLoggedIn?.Invoke(loggedIN);
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
