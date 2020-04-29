using System;

namespace HTTPServerHost
{
    public class MessageObj
    {
        public string[] message = new string[1];
        public string[] sender = new string[1];
        public string[] time = new string[1];
        public string ghostMode = "false";

        public MessageObj(Tuple<string[], string[], string[]> tuple)
        {
            this.message = tuple.Item1;
            this.sender = tuple.Item2;
            this.time = tuple.Item3;
        }
    }
}