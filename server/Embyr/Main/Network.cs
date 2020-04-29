using System;
using System.IO;
using System.Net;

namespace Embyr.Main
{
    public class Network
    {
        public static string NpgSQLConnection = String.Format("Server={0};Port={1};User Id={2};Password={3};Database={4};", "10.0.0.17", "5432", "postgres", "mrRobot2831", "embyr");

        public static string GetIP()
        {
            string url = "http://checkip.dyndns.org";
            WebRequest req = System.Net.WebRequest.Create(url);
            WebResponse resp = req.GetResponse();
            StreamReader sr = new StreamReader(resp.GetResponseStream());
            string response = sr.ReadToEnd().Trim();
            string[] a = response.Split(':');
            string a2 = a[1].Substring(1);
            string[] a3 = a2.Split('<');
            string a4 = a3[0];
            return a4;
        }
    }
}
