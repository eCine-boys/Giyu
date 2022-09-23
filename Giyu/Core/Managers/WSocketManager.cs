using System;
using Websocket.Client;
using System.Text;
using System.Threading;

namespace Giyu.Core.Managers
{
    public class WSocketManager
    {
        public WSocketManager(string _uri)
        {
            Uri uri = new Uri($"wss://{_uri}");

            try
            {
                ManualResetEvent exitEvent = new ManualResetEvent(false);

                using WebsocketClient client = new WebsocketClient(uri);

                client.ReconnectTimeout = TimeSpan.FromSeconds(5);

                client.ReconnectionHappened.Subscribe(info =>
                {
                    Console.WriteLine("Reconnection; Type:" + info.Type);
                });

                client.MessageReceived.Subscribe(msg =>
                {
                    Console.WriteLine($"Message received {msg}");

                    if (msg.ToString().ToLower() == "connected")
                    {
                        client.Send("Hello");
                    }
                });

                client.Start();

                exitEvent.WaitOne();
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR: " + ex.ToString());
            }
        }

    }
}
