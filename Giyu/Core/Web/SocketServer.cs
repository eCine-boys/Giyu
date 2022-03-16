using WatsonTcp;
using Giyu.Core.Managers;
using System;
using System.Net;
using System.Collections.Generic;
using System.Text;

namespace Giyu.Core.Web
{
    public class SocketServer
    {
        public static void Init(int port, IPAddress ip)
        {
            WatsonTcpServer TServer = new WatsonTcpServer(ip.ToString(), port);

            TServer.Events.ClientConnected += OnConnected;
            TServer.Events.MessageReceived += MessageReceived;
            TServer.Events.ClientDisconnected += ClientDisconnected;
            TServer.Callbacks.SyncRequestReceived = SyncRequestReceived;

            TServer.Start();

            IEnumerable<string> clients = TServer.ListClients();
        }

        private static SyncResponse SyncRequestReceived(SyncRequest arg)
        {
            throw new NotImplementedException();
        }

        private static void ClientDisconnected(object sender, DisconnectionEventArgs e)
        {
            throw new NotImplementedException();
        }

        private static void MessageReceived(object sender, MessageReceivedEventArgs e)
        {
            LogManager.Log("[GIYU-WS] MessageReceived", $"[{e.IpPort}]: {Encoding.UTF8.GetString(e.Data)}");
        }

        private static void OnConnected(object sender, ConnectionEventArgs e)
        {
            LogManager.Log("[GIYU-WS] OnConnected", $"Client connected: {e.IpPort}");
        }
    }
}
