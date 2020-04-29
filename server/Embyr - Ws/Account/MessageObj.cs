using System;

namespace WebSocketServer.Account
{
    public class MessageObj
    {
        public string eventV = "receiveMessage";
        public string message = null;
        public string sender = null;
        public string receiver = null;
        public string time = null;
        public string ghostMode = null;

        public MessageObj(string sender, string receiver, string message, string time, string ghostMode)
        {
            this.sender = sender;
            this.receiver = receiver;
            this.message = message;
            this.time = time;
            this.ghostMode = ghostMode;
        }
    }
}