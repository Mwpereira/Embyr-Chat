using Npgsql;
using System;
using Embyr.Global;
using Embyr.Main;
using Embyr.Operations;

namespace Embyr.Operations
{
    class KeyTagID
    {
        public static int GrabUserKeyTag(string username)
        {
            int tempKey = 127;
            try
            {
                using (NpgsqlConnection conn = new NpgsqlConnection(Network.NpgSQLConnection))
                {
                    conn.Open();

                    NpgsqlCommand myCommand = new NpgsqlCommand("SELECT keyid FROM embyrusers.users WHERE accountname = :accountname", conn);

                    myCommand.Parameters.Add(new NpgsqlParameter(":accountname", username));

                    NpgsqlDataReader keyTagReader = myCommand.ExecuteReader();

                    while (keyTagReader.Read())
                    {
                        tempKey = Convert.ToInt32(keyTagReader[0].ToString());
                    }
                    conn.Close();
                }
            }
            catch (NpgsqlException e)
            {
                Console.WriteLine(e.ToString());
            }
            return tempKey;
        }

        public static bool CheckKeyTag(int keytag)
        {
            try
            {
                using (NpgsqlConnection conn = new NpgsqlConnection(Network.NpgSQLConnection))
                {
                    conn.Open();

                    NpgsqlCommand myCommand = new NpgsqlCommand("SELECT COUNT(*) FROM embyrusers.users WHERE keyid = :keyid", conn);

                    myCommand.Parameters.Add(new NpgsqlParameter("@keyid", keytag.ToString()));

                    Int64 keyTagCount = (Int64)myCommand.ExecuteScalar();

                    if (keyTagCount > 0)
                        return true;

                    return false;
                }
            }
            catch (NpgsqlException e)
            {
                Console.WriteLine(e.ToString());
                return false;
            }
        }

        public static string GetPMKeyTag(string senderName, string receiverName)
        {
            try
            {
                string directmessage = "";

                int senderID = KeyTagID.GrabUserKeyTag(senderName);

                int receiverID = KeyTagID.GrabUserKeyTag(receiverName);

                if (senderID > receiverID)
                    directmessage = senderID.ToString() + "28" + receiverID.ToString();

                if (senderID < receiverID)
                    directmessage = receiverID.ToString() + "28" + senderID.ToString();

                string pmKeyTag = FriendsList.numToAlpha(directmessage);

                return pmKeyTag;
            }
            catch (NpgsqlException e)
            {
                Console.WriteLine(e.ToString());
                return null;
            }
        }

        public static bool GetUserExists(string username)
        {
            try
            {
                using (NpgsqlConnection conn = new NpgsqlConnection(Network.NpgSQLConnection))
                {
                    conn.Open();

                    NpgsqlCommand myCommand = new NpgsqlCommand("SELECT COUNT(*) FROM embyrusers.users WHERE accountname = :accountname", conn);

                    myCommand.Parameters.Add(new NpgsqlParameter("@accountname", username));

                    Int64 keyTagCount = (Int64)myCommand.ExecuteScalar();

                    if (keyTagCount > 0)
                        return true;

                    return false;
                }
            }
            catch (NpgsqlException e)
            {
                Console.WriteLine(e.ToString());
                return false;
            }
        }
    }
}