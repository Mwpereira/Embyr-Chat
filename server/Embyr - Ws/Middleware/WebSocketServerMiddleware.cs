using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Embyr.Global;
using Embyr.WebSockets;
using Embyr.Operations;
using WebSocketServer.Account;

namespace WebSocketServer.Middleware
{
    public class WebSocketServerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly  WebSocketServerConnectionManager _manager;

        public static int requestCount = 0;

        public WebSocketServerMiddleware(RequestDelegate next, WebSocketServerConnectionManager manager)
        {
            _next = next;
            _manager = manager;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context.WebSockets.IsWebSocketRequest)
            {
                WebSocket webSocket = await context.WebSockets.AcceptWebSocketAsync();

                bool clientEstablished = false;

                await Receive(webSocket, async (result, buffer) =>
                {
                if (clientEstablished == false && result.MessageType == WebSocketMessageType.Text)
                {
                    _manager.AddSocket(Encoding.UTF8.GetString(buffer, 0, result.Count), webSocket);
                    clientEstablished = true;
                    string username = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    await webSocket.SendAsync(Encoding.UTF8.GetBytes("{\"eventV\": \"setProfilePhoto\", \"user\": \"" + username + "\", \"photoBytes\": \"" + ProfilePhoto.GetImage(username) + "\"}"), WebSocketMessageType.Text, true, CancellationToken.None);
                    FriendsList.SetOnline(username, "true");
                    List<string> obj = new List<string>() { username, FriendsList.GetStatus(username) };
                    List<string> friends = FriendsList.GetFriends(username).Split(GlobalVar.delimiterChars, System.StringSplitOptions.RemoveEmptyEntries).OfType<string>().ToList();
                    foreach (string friend in friends)
                    {
                        await RouteJSONMessageAsync("setStatus", obj, friend);
                        try
                        {
                           await webSocket.SendAsync(Encoding.UTF8.GetBytes("{\"eventV\": \"setProfilePhoto\", \"user\": \"" + friend + "\", \"photoBytes\": \"" + ProfilePhoto.GetImage(friend) + "\"}"), WebSocketMessageType.Text, true, CancellationToken.None);
                        }
                        catch
                        {
                           Console.WriteLine("Error Loading {0}'s Profile Photo!",friend);
                        }
                    }
                    
                    Console.WriteLine($"WebSocket Operation: Client->EstablishedConnection");
                    Console.WriteLine("\nSet the status of: {0}\tChanged to: online", obj[0]);
                }
                else
                {
                    if (result.MessageType == WebSocketMessageType.Text)
                    {
                        List<string> obj = new List<string>();
                        dynamic jsonObj = JObject.Parse(Encoding.UTF8.GetString(buffer, 0, result.Count));
                        string job = jsonObj.job;
                        switch (job)
                        {
                            case ("sendMessage"):
                                obj.Add((jsonObj.message).ToString());
                                obj.Add((jsonObj.sender).ToString());
                                obj.Add((jsonObj.receiver).ToString());
                                obj.Add(GlobalVar.currentDateMsg());
                                obj.Add((jsonObj.ghostMode).ToString());
                                Console.WriteLine(obj[4].ToString());
                                Console.WriteLine("Operation:\tsendMessage\n");
                                await RouteJSONMessageAsync(job, obj, obj[2]);
                                break;
                            case ("setStatus"):
                                obj.Add((jsonObj.sender).ToString());
                                obj.Add((jsonObj.status).ToString());
                                Console.WriteLine("Operation:\tsetStatus\n");
                                List<string> friends = FriendsList.GetFriends(obj[0]).Split(GlobalVar.delimiterChars, System.StringSplitOptions.RemoveEmptyEntries).OfType<string>().ToList();
                                foreach (string friend in friends)
                                {
                                    await RouteJSONMessageAsync(job, obj, friend);
                                }
                                break;
                            case ("addFriendReq"):
                                obj.Add((jsonObj.sender).ToString());
                                obj.Add((jsonObj.receiver).ToString());
                                Console.WriteLine("Operation:\taddFriendReq\n");
                                if (KeyTagID.GetUserExists(obj[1]))
                                {
                                    FriendsList flAFR = new FriendsList("ADDFR", obj[0], obj[1], KeyTagID.GrabUserKeyTag(obj[0]));
                                    await webSocket.SendAsync(Encoding.UTF8.GetBytes("{\"eventV\": \"userExists\", \"existsV\": true, \"user\": \"" + obj[1].ToString() + "\"}"), WebSocketMessageType.Text, true, CancellationToken.None);
                                    await RouteJSONMessageAsync(job, obj, obj[1]);
                                }
                                else
                                    await webSocket.SendAsync(Encoding.UTF8.GetBytes("{\"eventV\": \"userExists\", \"existsV\": false}"), WebSocketMessageType.Text, true, CancellationToken.None);
                                break;
                            case ("cancelFriendReq"):
                                obj.Add((jsonObj.sender).ToString());
                                obj.Add((jsonObj.receiver).ToString());
                                Console.WriteLine("Operation:\tcancelFriendReq\n");
                                FriendsList flRFR = new FriendsList("REMOVEFR", obj[0], obj[1], KeyTagID.GrabUserKeyTag(obj[0]));
                                await RouteJSONMessageAsync(job, obj, obj[1]);
                                break;
                            case ("acceptFriendReq"):
                                obj.Add((jsonObj.sender).ToString());
                                obj.Add((jsonObj.receiver).ToString());
                                obj.Add(ProfilePhoto.GetImage(obj[0]));
                                Console.WriteLine("Operation:\tacceptFriendReq\n");
                                FriendsList flACFR = new FriendsList("ACCEPTFR", obj[0], obj[1], KeyTagID.GrabUserKeyTag(obj[0]));
                                await RouteJSONMessageAsync(job, obj, obj[1]);
                                await webSocket.SendAsync(Encoding.UTF8.GetBytes("{\"eventV\": \"setProfilePhoto\", \"user\": \"" + obj[0] + "\", \"photoBytes\": \"" + ProfilePhoto.GetImage(obj[1]) + "\"}"), WebSocketMessageType.Text, true, CancellationToken.None);
                                break;
                            case ("declineFriendReq"):
                                obj.Add((jsonObj.sender).ToString());
                                obj.Add((jsonObj.receiver).ToString());
                                Console.WriteLine("Operation:\tdeclineFriendReq\n");
                                FriendsList flDFR = new FriendsList("DECLINEFR", obj[0], obj[1], 0);
                                await RouteJSONMessageAsync(job, obj, obj[1]);
                                break;
                            case ("removeFriend"):
                                obj.Add((jsonObj.sender).ToString());
                                obj.Add((jsonObj.receiver).ToString());
                                Console.WriteLine("Operation:\removeFriend\n");
                                FriendsList flRF = new FriendsList("REMOVE", obj[0], obj[1], 0);
                                await RouteJSONMessageAsync(job, obj, obj[1]);
                                break;
                            case ("setProfilePhoto"):
                                obj.Add((jsonObj.sender).ToString());
                                obj.Add((jsonObj.photoBytes).ToString());
                                Console.WriteLine("Operation:\tsetProfilePhoto\n");
                                ProfilePhoto.SaveImage(obj[0], obj[1]);
                                List<string> friendsPP = FriendsList.GetFriends(obj[0]).Split(GlobalVar.delimiterChars, System.StringSplitOptions.RemoveEmptyEntries).OfType<string>().ToList();
                                foreach (string friend in friendsPP)
                                {
                                     await RouteJSONMessageAsync(job, obj, friend);
                                }
                                break;
                            }
                            obj.Clear();
                        }
                        else if (result.MessageType == WebSocketMessageType.Close)
                        {
                            string id = _manager.GetAllSockets().FirstOrDefault(s => s.Value == webSocket).Key;

                            _manager.GetAllSockets().TryRemove(id, out WebSocket sock);

                            await sock.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);

                            List<string> obj = new List<string>();
                            obj.Add(id);
                            obj.Add("offline");
                            List<string> friends = FriendsList.GetFriends(obj[0]).Split(GlobalVar.delimiterChars, System.StringSplitOptions.RemoveEmptyEntries).OfType<string>().ToList();
                            foreach (string friend in friends)
                            {
                                await RouteJSONMessageAsync("setStatusOffline", obj, friend);
                            }
                            FriendsList.SetOnline(id, "false");
                            Console.WriteLine("\nSet the status of: {0}\tChanged to: {1}", obj[0], obj[1]);
                            Console.WriteLine($"WebSocket Operation: Receive->Close");
                        }
                    }
                    requestCount++;
                    Console.WriteLine("\nRequest #: " + requestCount + "\nManaged Connections: " + _manager.GetAllSockets().Count.ToString());
                    Console.WriteLine("\n\n" + GlobalVar.border + "\n");
                    return;
                });
            }
            else
                await _next(context);
        }

        private async Task Receive(WebSocket socket, Action<WebSocketReceiveResult, byte[]> handleMessage)
        {
            var buffer = new byte[10000000];

            while (socket.State == WebSocketState.Open)
            {
                var result = await socket.ReceiveAsync(buffer: new ArraySegment<byte>(buffer), cancellationToken: CancellationToken.None);
                handleMessage(result, buffer);
            }
        }

        private async Task RouteJSONMessageAsync(string route, List<string> obj, string receiver)
        {
            var sock = _manager.GetAllSockets().FirstOrDefault(s => s.Key == receiver);                       

            if (sock.Value != null)
            {
                if (sock.Value.State == WebSocketState.Open)
                {
                    switch (route)
                    {
                        case ("sendMessage"):
                            MessageObj msgObj = new MessageObj(obj[1], obj[2], obj[0], obj[3], obj[4]);
                            string json = JsonConvert.SerializeObject(msgObj);
                            await sock.Value.SendAsync(Encoding.UTF8.GetBytes(json), WebSocketMessageType.Text, true, CancellationToken.None);
                            Console.WriteLine("Receiver Online! Message sent and stored in database.");
                            break;
                        case ("groupSendMessage"):
                            Console.WriteLine("Broadcast");
                            MessageObj grpMsgObj = new MessageObj(obj[1], obj[2], obj[0], obj[3], obj[4]);
                            string jsonGrp = JsonConvert.SerializeObject(grpMsgObj);
                            await sock.Value.SendAsync(Encoding.UTF8.GetBytes(jsonGrp), WebSocketMessageType.Text, true, CancellationToken.None);
                            Console.WriteLine("Receiver Online! Status sent!");
                            break;
                        case ("setStatusOffline"):
                        case("setStatus"):
                            await sock.Value.SendAsync(Encoding.UTF8.GetBytes("{\"eventV\": \"setStatus\", \"friend\": \"" + obj[0] + "\", \"status\": \"" + obj[1] + "\" }"), WebSocketMessageType.Text, true, CancellationToken.None);                            
                            break;
                        case ("addFriendReq"):                            
                                FriendsListObj flAFR = new FriendsListObj("incomingRequest", obj[0].ToString());
                                string jsonAFR = JsonConvert.SerializeObject(flAFR);
                                await sock.Value.SendAsync(Encoding.UTF8.GetBytes(jsonAFR), WebSocketMessageType.Text, true, CancellationToken.None);
                                Console.WriteLine("Receiver Online! Data sent and stored in database.");
                            break;
                        case ("cancelFriendReq"):
                            FriendsListObj flCFR = new FriendsListObj("cancelIncomingRequest", obj[0].ToString());
                            string jsonCFR = JsonConvert.SerializeObject(flCFR);
                            await sock.Value.SendAsync(Encoding.UTF8.GetBytes(jsonCFR), WebSocketMessageType.Text, true, CancellationToken.None);
                            Console.WriteLine("Receiver Online! Data sent and stored in database.");
                            break;
                        case ("acceptFriendReq"):
                            FriendsListObj flACFR = new FriendsListObj("addFriend", obj[0].ToString(), obj[2].ToString());
                            string jsonACFR = JsonConvert.SerializeObject(flACFR);
                            await sock.Value.SendAsync(Encoding.UTF8.GetBytes(jsonACFR), WebSocketMessageType.Text, true, CancellationToken.None);
                            Console.WriteLine("Receiver Online! Data sent and stored in database.");
                            break;
                        case ("declineFriendReq"):
                            FriendsListObj flDFR = new FriendsListObj("removeIncomingRequest", obj[0].ToString());
                            string jsonDFR = JsonConvert.SerializeObject(flDFR);
                            await sock.Value.SendAsync(Encoding.UTF8.GetBytes(jsonDFR), WebSocketMessageType.Text, true, CancellationToken.None);
                            Console.WriteLine("Receiver Online! Data sent and stored in database.");
                            break;
                        case ("removeFriend"):
                            FriendsListObj flRF = new FriendsListObj("removeFriend", obj[0].ToString());
                            string jsonRF = JsonConvert.SerializeObject(flRF);
                            await sock.Value.SendAsync(Encoding.UTF8.GetBytes(jsonRF), WebSocketMessageType.Text, true, CancellationToken.None);
                            Console.WriteLine("Receiver Online! Data sent and stored in database.");
                            break;
                        case ("setProfilePhoto"):
                            await sock.Value.SendAsync(Encoding.UTF8.GetBytes("{\"eventV\": \"setProfilePhoto\", \"user\": \"" + obj[0] + "\", \"photoBytes\": \"" + obj[1] + "\" }"), WebSocketMessageType.Text, true, CancellationToken.None);
                            break;  
                    }
                }
            }
            else
            {
                switch (route)
                {
                    case ("sendMessage"):
                        Console.WriteLine("Receiver Offline! Message will be stored in database.");
                        break;
                    case ("addFriendReq"):
                    case ("cancelFriendReq"):
                    case ("acceptFriendReq"):
                    case ("declineFriendReq"):
                    case ("removeFriend"):
                        Console.WriteLine("Receiver Offline! Data will be stored in database.");
                        break;
                }
            }
            if (route == "sendMessage")
            {
                if (obj[4].ToLower()=="false")
                {
                    await Message.StoreMessage(obj[1], obj[2], obj[0]);
                }                
                Console.WriteLine("\nSender:\t\t{0}\nReceiver:\t{1}\nMessage:\t{2}\n\n", obj[1], obj[2], obj[0]);
            }
        }
    }
}