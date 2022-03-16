using Giyu.Core.Managers;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Alchemy;
using Alchemy.Classes;
using System.Net;

namespace Giyu.Core.Web
{
    public class SocketServer
    {
        public static void Init(int port, IPAddress ip)
        {
            var SocketServer = new WebSocketServer(port, ip)
            {
                OnConnected = OnConnected,
                OnReceive = OnReceive,
                OnConnect = OnConnect,
                OnDisconnect = OnDisconnect,
                TimeOut = new TimeSpan(0, 10, 0)
            };

            SocketServer.Start();
        }

        private static void OnDisconnect(UserContext context)
        {
            LogManager.Log("WS-GIYU", context.Data.ToString());
        }

        private static void OnConnect(UserContext context)
        {
            LogManager.Log("WS-GIYU", $"Client connect: {context.ClientAddress.ToString()}");
        }

        private static void OnReceive(UserContext context)
        {
            LogManager.Log("WS-GIYU", context.Data.ToString());
        }

        private static void OnConnected(UserContext context)
        {
            Console.WriteLine("Client Connection From : " + context.ClientAddress.ToString());
        }
    }
}
