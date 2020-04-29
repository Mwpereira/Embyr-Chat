using System;
using System.IO;
using System.Text;

namespace WebSocketServer.Account
{
    public class ProfilePhoto
    {

        public static void SaveImage(string username, string photoBytesS)
        {
            try
            {
                var fileStream = new FileStream(@"C:\Users\Michael\Desktop\HTTPServerHost\server\Profile Photos\" + username + ".txt", FileMode.Truncate, FileAccess.Write);
                using (var streamWriter = new StreamWriter(fileStream, Encoding.UTF8))
                {
                    streamWriter.Write(photoBytesS);
                }                                      
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        public static string GetImage(string username)
        {
            string photoBase64 = null;
            var fileStream = new FileStream(@"C:\Users\Michael\Desktop\HTTPServerHost\server\Profile Photos\" + username + ".txt", FileMode.Open, FileAccess.Read);
            using (var streamReader = new StreamReader(fileStream, Encoding.UTF8))
            {
                photoBase64 = streamReader.ReadToEnd();
                return photoBase64;
            }
        }

    }
}
