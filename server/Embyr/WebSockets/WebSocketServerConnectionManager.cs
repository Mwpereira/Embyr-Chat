using System;
using System.Collections.Concurrent;
using System.Net.WebSockets;

namespace Embyr.WebSockets
{
    public class WebSocketServerConnectionManager
    {
        private ConcurrentDictionary<string, WebSocket> _sockets = new ConcurrentDictionary<string, WebSocket>();

        public string AddSocket(string client, WebSocket socket)
        {
            _sockets.TryAdd(client, socket);
            Console.WriteLine("WebSocketServerConnectionManager: AddSocket -> WebSocket with ID: " + client);
            return client;
        }

        public ConcurrentDictionary<string, WebSocket> GetAllSockets()
        {
            return _sockets;
        }
    }
}