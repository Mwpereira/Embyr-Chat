using System;

namespace HTTPServerHost
{
    public class UserInfoObj
    {
        public string accountname = "";
        public int keyID = 0;
        public string status = null;
        public string[] friendsList = null;
        public string[] friendReqIn = null;
        public string[] friendReqOut = null;
        public string[] friendStatus = null;

        public UserInfoObj(Tuple<string[], string[], string[], string[]> tuple)
        {
            this.accountname = Program.Response.tmpUser;
            this.keyID = Embyr.Operations.KeyTagID.GrabUserKeyTag(Program.Response.tmpUser);
            this.status = Embyr.Operations.FriendsList.GetStatus(Program.Response.tmpUser);
            this.friendsList = tuple.Item1;
            this.friendReqIn = tuple.Item2;
            this.friendReqOut = tuple.Item3;
            this.friendStatus = tuple.Item4;
        }
    }
}
