using Npgsql;
using System;
using Embyr.Global;
using Embyr.Main;
using Embyr.Operations;

namespace HTTPServerHost
{
    class Profile
    {
        public static Tuple<string[], string[], string[], string[]> LoadInformation(string username)
        {            
            try
            {
                using (NpgsqlConnection conn = new NpgsqlConnection(Network.NpgSQLConnection))
                {
                    string text = "";

                    conn.Open();

                    NpgsqlCommand myCommand = new NpgsqlCommand("SELECT * FROM embyrfriends.friends WHERE accountname = @accountname", conn);
                    myCommand.Parameters.Add(new NpgsqlParameter(":accountname", username));
                    NpgsqlDataReader friendsReader = myCommand.ExecuteReader();
                    friendsReader.Read();

                    string friendsFromDB = friendsReader[2].ToString();

                    text = friendsFromDB;

                    string[] friends = text.Split(GlobalVar.delimiterChars, System.StringSplitOptions.RemoveEmptyEntries);

                    string requestsIn = friendsReader[3].ToString();

                    text = requestsIn;

                    string[] reqIn = text.Split(GlobalVar.delimiterChars, System.StringSplitOptions.RemoveEmptyEntries);

                    string requestsOut = friendsReader[4].ToString();

                    text = requestsOut;

                    string[] reqOut = text.Split(GlobalVar.delimiterChars, System.StringSplitOptions.RemoveEmptyEntries);

                    conn.Close();

                    string[] friendsStatus = new string[friends.Length];

                    int i = 0;

                    foreach (string friend in friends){
                        friendsStatus[i] = FriendsList.GetStatus(friend);
                        i++;
                    }

                    var tuple = new Tuple<string[], string[], string[], string[]>(friends, reqIn, reqOut, friendsStatus);

                    return tuple;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return null;
            }
        }
    }
}