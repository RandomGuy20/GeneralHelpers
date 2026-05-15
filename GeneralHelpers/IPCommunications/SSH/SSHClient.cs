using Crestron.SimplSharp;
using Crestron.SimplSharp.Ssh;
using Crestron.SimplSharp.Ssh.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeneralHelpers.IPCommunications.SSH
{
    public class SSHClient: ISecurityBase
    {
        #region Fields

        private string loginFlag;

        private bool loggedIN;

        internal SshClient sshClient;
        internal ShellStream sshShellStream;

        private KeyboardInteractiveAuthenticationMethod authMethod;
        private ConnectionInfo connectInfo;

        private CTimer connState;


        private string userName, password;
        private string ipAddress;

        private bool isConnected, isReconnect = false;



        #endregion

        #region Properties

        public bool Debug { get; set; } = false;

        public string UserName
        {
            get => userName;
            set => SetUserName(value);

        }

        public string Password
        {
            get => password;
            set => SetPassword(value);

        }

        public bool IsConnected => isConnected;

        public bool AutoReconnect
        {
            get => isReconnect;
            set => isReconnect = value;
        }

        public string IpAddress
        {
            get => ipAddress;
            set => SetIpAddress(value);
            
        }    

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

        public SSHClient(string IpAddress, string UserName, string Password)
        {
            ipAddress = IpAddress;
            password = Password;
            userName = UserName;

            authMethod = new KeyboardInteractiveAuthenticationMethod(userName);
            authMethod.AuthenticationPrompt += AuthMethod_AuthenticationPrompt;

            connectInfo = new ConnectionInfo(ipAddress, userName, authMethod);


            sshClient = new SshClient(connectInfo);
            sshClient.ErrorOccurred += SshClient_ErrorOccurred;
            sshClient.HostKeyReceived += SshClient_HostKeyReceived;
            sshClient.KeepAliveInterval = TimeSpan.FromSeconds(25);

            connState = new CTimer((object obj) =>
            {
                isConnected = sshClient.IsConnected;
                SendStatusChangeEvent(this, sshClient.IsConnected ? "Connected" : "Not Connected", sshClient.IsConnected);
            }, null, 100, 100);
        }





        #endregion

        #region Internal Methods

        internal void SetUserName(string user)
        {
            if (string.IsNullOrEmpty(userName))
                return;
            userName = user;
        }

        internal void SetPassword(string pass)
        {
            if (string.IsNullOrEmpty(password))
                return;
            password = pass;
        }

        internal void SetIpAddress(string IpAddress)
        {
            if (string.IsNullOrEmpty(ipAddress))
                return;

            ipAddress = IpAddress;
        }

        internal void SendStatusChangeEvent(object sender, string statusMessage, bool connectionState)
        {
            onStatusChangeEvent?.Invoke(sender, statusMessage, connectionState);
        }

        internal void SendDataReceivedEvent(object sender, string sData, byte[] bData)
        {
            onDataReceivedEvent?.Invoke(sender, sData, bData);
        }

        private void SshClient_HostKeyReceived(object sender, Crestron.SimplSharp.Ssh.Common.HostKeyEventArgs e)
        {
            e.CanTrust = true; 
        }

        private void SshClient_ErrorOccurred(object sender, Crestron.SimplSharp.Ssh.Common.ExceptionEventArgs e)
        {
            SendDebug($"SSH Client Error: {e}");
            Disconnect();
        }

        private void AuthMethod_AuthenticationPrompt(object sender, Crestron.SimplSharp.Ssh.Common.AuthenticationPromptEventArgs e)
        {
            foreach (AuthenticationPrompt item in e.Prompts)
                item.Response = password;
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
            try
            {
                Disconnect();


                CrestronEnvironment.Sleep(100);
                sshClient.Connect();
                sshShellStream = sshClient.CreateShellStream("xterm", 80, 24, 800,
                    600, 1024);

                CrestronConsole.PrintLine($"SSh Client trying to connect to IP address {ipAddress}");
                sshShellStream.DataReceived += SshShellStream_DataReceived;
                sshShellStream.ErrorOccurred += SshShellStream_ErrorOccurred;
                CrestronEnvironment.Sleep(100);

                sshShellStream.Write("\n");
                sshShellStream.Flush();
            }
            catch (SshConnectionException e)
            {
                SendDebug($"Error in SSHClient Connect is {e}");
            }


        }

        public  void Disconnect()
        {
            try
            {
                if (sshShellStream != null)
                {
                    sshShellStream.DataReceived -= SshShellStream_DataReceived;
                    sshShellStream.ErrorOccurred -= SshShellStream_ErrorOccurred;

                    sshShellStream.Close();
                    sshShellStream.Dispose();
                    sshShellStream = null;
                }

                if (sshClient != null && sshClient.IsConnected)
                {
                    sshClient.Disconnect();
                }
            }
            catch (SshConnectionException e)
            {
                SendDebug($"Error in SSHClient Disconnect is {e}");
            }
        }

        private void SshShellStream_ErrorOccurred(object sender, ExceptionEventArgs e)
        {
            SendDebug($"ERror in Shell Stream SSH Client is {e} disconecting...");
            Disconnect();
        }

        private void SshShellStream_DataReceived(object sender, ShellDataEventArgs e)
        {
            try
            {

                if (e.Data.Length < 1)
                    return;


                var encoding = Encoding.ASCII.GetString(e.Data);
                
                SendDataReceivedEvent(this, encoding, null);

            }
            catch (Exception exception)
            {
                CrestronConsole.PrintLine($"Error in SSHStreamReturn is: {exception}");
            }


        }

        public  void SendData(string data)
        {
            try
            {

                if (sshShellStream == null || !sshShellStream.CanWrite)
                {
                    SendDebug("Cannot write to sshShellStream: Stream is null or disposed.");
                    return;
                }

                sshShellStream.WriteLine(data);
            }
            catch (Exception e)
            {
                SendDebug($"SSHClient Error in SendData is(stack trace) : {e.StackTrace}");
            }

        }

        public  void SendData(byte[] data)
        {
            if (sshShellStream.CanWrite)
            {
                string strData = Encoding.UTF8.GetString(data);
                sshShellStream.WriteLine(strData);
            }
        }

        public  void Dispose()
        {

            AutoReconnect = false;

            if (connState != null)
            {
                connState.Stop();
                connState.Dispose();
                connState = null;
            }

            if (sshShellStream != null)
            {
                sshShellStream.DataReceived -= SshShellStream_DataReceived;
                sshShellStream.ErrorOccurred -= SshShellStream_ErrorOccurred;
                sshShellStream.Close();
                sshShellStream.Dispose();
                sshShellStream = null;
            }


            if (sshClient != null)
            {
   
                Disconnect();
                sshClient.ErrorOccurred -= SshClient_ErrorOccurred;
                sshClient.HostKeyReceived -= SshClient_HostKeyReceived;
                sshClient.Dispose();
                sshClient = null;
            }


        }



        #endregion
    }
}
