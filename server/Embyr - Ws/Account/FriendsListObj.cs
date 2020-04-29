using System;

namespace WebSocketServer.Account
{
    public class FriendsListObj
    {
        public string eventV = null;
        public string sender = null;
        public string photoBytes = null;

        public FriendsListObj(string eventV, string sender)
        {
            this.eventV = eventV;
            this.sender = sender;
        }

        public FriendsListObj(string eventV, string sender, string photoBytes)
        {
            this.eventV = eventV;
            this.sender = sender;
            this.photoBytes = photoBytes;
        }
    }
}