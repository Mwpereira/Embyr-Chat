using System;
using System.IO;
using System.Net;

namespace Embyr.Main
{
    public class Host
    {
        public static string hostIP = HostIP();
        public static string url = "http://*:8000/";
        public static string localIP = "10.0.0.17";
        public static int port = 8000;
        public static string HostIP()
        {
            String address = "";
            WebRequest request = WebRequest.Create("http://checkip.dyndns.org/");
            using (WebResponse response = request.GetResponse())
            using (StreamReader stream = new StreamReader(response.GetResponseStream()))
            {
                address = stream.ReadToEnd();
            }

            int first = address.IndexOf("Address: ") + 9;
            int last = address.LastIndexOf("</body>");
            address = address.Substring(first, last - first);

            return address;
        }
    }
}
