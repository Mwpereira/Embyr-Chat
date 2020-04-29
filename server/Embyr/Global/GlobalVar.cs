using System;
using System.Net;

namespace Embyr.Global
{
    public class GlobalVar
    {
        public static int cValue = 138;

        public static string cValueR = cValue.ToString("X");

        public static string[] delimiterChars = { cValueR, "=", "&" };

        public static string border = "************************************************************************************************************************";
        

        public static string currentDate()
        {
            var timeN = DateTime.Now;
            string time = timeN.ToString("MMM dd, yyyy, HH:mm:ss tt");

            return time;
        }

        public static string currentDateMsg()
        {
            var timeN = DateTime.Now;
            string time = timeN.ToString("MMM dd, yyyy, h:mm:ss tt");

            return time;
        }

        public static bool CheckForInternetConnection()
        {
            try
            {
                using (var client = new WebClient())
                using (client.OpenRead("http://google.ca/"))
                {
                    return true;
                }
            }
            catch (WebException e)
            {
                Console.WriteLine(e.ToString());
                return false;
            }
        }
    }
}