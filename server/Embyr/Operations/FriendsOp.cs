using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Embyr.Global;
using Embyr.Main;
using Embyr.WebSockets;
using Microsoft.AspNetCore.Http;
using Npgsql;

namespace Embyr.Operations
{
    class FriendsOp
    {
        public readonly RequestDelegate _next;
        public readonly WebSocketServerConnectionManager _manager;

        public FriendsOp(RequestDelegate next, WebSocketServerConnectionManager manager)
        {
            _next = next;
            _manager = manager;
        }

        public FriendsOp(string action, string username, string friend)
        {
            if(action=="removeFriend")
                RemoveFriendWebSocket(username, friend);            
        }

        public async Task RemoveFriendWebSocket(string username, string friend)
        {
            var sock = _manager.GetAllSockets().FirstOrDefault(s => s.Key == friend);

            if (sock.Value != null)
            {
                if (sock.Value.State == WebSocketState.Open)
                {
                    await sock.Value.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes("{\"eventV\": \"friendRemoved\", \"friend\": \"" + username + "\" }")), WebSocketMessageType.Text, true, CancellationToken.None);
                }
            }
        }
    }
}
