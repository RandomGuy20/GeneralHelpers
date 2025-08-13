using Crestron.SimplSharp;
using Crestron.SimplSharp.Ssh;
using Crestron.SimplSharp.Ssh.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeneralHelpers.IPCommunications.SSH
{
    internal class SSHClient: AIpCommunicationsBase, ISecurityBase
    {
        #region Fields

        private string loginFlag;

        private bool loggedIN;



        private KeyboardInteractiveAuthenticationMethod authMethod;
        private ConnectionInfo connectInfo;

        private CTimer connState;









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


        #endregion

        #region Delegates



        #endregion

        #region Events



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
                SendStatusChangeEvent(this, sshClient.IsConnected ? "Connected" : "Not Connected", sshClient.IsConnected);
            }, null, 100, 100);
        }





        #endregion

        #region Internal Methods

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

        #endregion

        #region Public Methods

        public override void Connect()
        {
            if(sshClient.IsConnected)
                return;

            try
            {
                sshClient.Connect();
                
            }
            catch (SshConnectionException e)
            {
                SendDebug($"Error in SSHClient Connect is {e}");
            }

            sshShellStream = sshClient.CreateShellStream("terminal", 80U, 24U, 800U,
                600U, 65535);
            sshShellStream.DataReceived += SshShellStream_DataReceived;
            sshShellStream.ErrorOccurred += SshShellStream_ErrorOccurred;
        }

        public override void Disconnect()
        {
            if (!sshClient.IsConnected)
                return;
            try
            {
                sshClient.Disconnect();
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

            if (e.Data.Length < 1)
                return;

            using(ShellStream stream = (ShellStream)sender)
            {
                StringBuilder data = new StringBuilder();

                while(stream.DataAvailable)
                {
                    data.Append(stream.ReadLine());
                }

                SendDataReceivedEvent(this,data.ToString(),null);
            }
        }

        public override void SendData(string data)
        {
            if (sshShellStream.CanWrite)
                sshShellStream.WriteLine(data);
        }

        public override void SendData(byte[] data)
        {
            if (sshShellStream.CanWrite)
            {
                string strData = Encoding.UTF8.GetString(data);
                sshShellStream.WriteLine(strData);
            }
        }

        public override void Dispose()
        {
            Disconnect();
            if(sshClient != null)
            {
                sshClient.Dispose();
                sshClient = null;
            }
        }



        #endregion
    }
}
