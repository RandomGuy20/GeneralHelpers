using Crestron.SimplSharp.CrestronWebSocketClient;
using Independentsoft.Exchange.Autodiscover;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeneralHelpers.IPCommunications.WebSockets
{
    internal class WebSocket : AIpCommunicationsBase
    {
        #region Fields

        WebSocketClient.WEBSOCKET_RESULT_CODES result;






        #endregion

        #region Properties


        #endregion

        #region Delegates

        #endregion

        #region Events

        #endregion

        #region Constructors

        public WebSocket(int Port, string URL)
        {
            webSocketClient = new WebSocketClient();
            webSocketClient.KeepAlive = true;

            webSocketClient.ConnectionCallBack += (WebSocketClient.WEBSOCKET_RESULT_CODES error) =>
            {
                isConnected = (error == WebSocketClient.WEBSOCKET_RESULT_CODES.WEBSOCKET_CLIENT_SUCCESS);

                SendStatusChangeEvent(this, error.ToString(), isConnected);

                if (isConnected)
                {
                    int async = (int) webSocketClient.ReceiveAsync();
                }

                return 0;
            };

            webSocketClient.DisconnectCallBack += (WebSocketClient.WEBSOCKET_RESULT_CODES error, object obj) =>
            {
                isConnected = false;
                SendStatusChangeEvent(this, error.ToString(), isConnected);

                return 0;
            };

            webSocketClient.ReceiveCallBack += (byte[] bytes, uint length,
                WebSocketClient.WEBSOCKET_PACKET_TYPES packetType,
                WebSocketClient.WEBSOCKET_RESULT_CODES error) =>
            {

                string data = Encoding.UTF8.GetString(bytes, 0, bytes.Length);
                SendDataReceivedEvent(this, data, null);
                int async = (int) webSocketClient.ReceiveAsync();


                return 0;
            };

            webSocketClient.SendCallBack += (WebSocketClient.WEBSOCKET_RESULT_CODES error) =>
            {
                int async = (int)webSocketClient.ReceiveAsync();

                return 0;
            };

            url = URL;
            port = Port;

            

        }



        #endregion

        #region Internal Methods

        #endregion

        #region Public Methods

        public override void Connect()
        {
            result = webSocketClient.ConnectAsync();


        }

        public override void Disconnect()
        {
            result = webSocketClient.DisconnectAsync(null);
        }

        public override void SendData(string data)
        {
            byte[] bytes = Encoding.ASCII.GetBytes(data);

            result = webSocketClient.SendAsync(bytes,(uint) bytes.Length,
                 WebSocketClient.WEBSOCKET_PACKET_TYPES.LWS_WS_OPCODE_07__TEXT_FRAME, WebSocketClient.WEBSOCKET_PACKET_SEGMENT_CONTROL.WEBSOCKET_CLIENT_PACKET_END);
        }

        public override void SendData(byte[] data)
        {
            result = webSocketClient.SendAsync(data, (uint)data.Length,
                     WebSocketClient.WEBSOCKET_PACKET_TYPES.LWS_WS_OPCODE_07__TEXT_FRAME, WebSocketClient.WEBSOCKET_PACKET_SEGMENT_CONTROL.WEBSOCKET_CLIENT_PACKET_END);
        }

        public override void Dispose()
        {
            webSocketClient.Dispose();
        }


        #endregion
    }
}
