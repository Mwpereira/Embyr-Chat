using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using Embyr.Global;
using Embyr.Main;

namespace Embyr.Operations
{
    class FriendsList
    {
        public FriendsList(string action, string user, string friend, int keyTag)
        {
            if (action == "ADDFR" || action == "REMOVEFR")
                AddorRemoveFriendFR(action, user, friend, keyTag);

            if (action == "ACCEPTFR")
                FriendAdder("in", user, friend, keyTag);
        }

        public FriendsList(string action, string user, string friend)
        {
            if (action == "REMOVE")
                RemoveFriend(user, friend);

            if (action == "DECLINEFR")
                RequestRemover(user, friend);
        }

        private void AddorRemoveFriendFR(string action, string user, string friend, int keyTag)
        {
            List<string> requestsOutList;
            List<string> requestsInList;

            string recipientUser;
            string friendsList;
            string input;

            if (KeyTagID.GrabUserKeyTag(friend) != 127)
            {
                try
                {
                    using (NpgsqlConnection connection = new NpgsqlConnection(Network.NpgSQLConnection))
                    {
                        connection.Open();

                        NpgsqlCommand myCommand = new NpgsqlCommand("SELECT * from embyrfriends.friends WHERE accountname = @accountname", connection);

                        myCommand.Parameters.Add(new NpgsqlParameter(":accountname", user));

                        NpgsqlDataReader requestOutReader = myCommand.ExecuteReader();

                        requestOutReader.Read();

                        input = requestOutReader[4].ToString();

                        requestsOutList = input.Split(GlobalVar.delimiterChars, System.StringSplitOptions.RemoveEmptyEntries).OfType<string>().ToList();

                        if (action == "ADDFR")
                        {
                            if (requestsOutList.Contains(friend) == false)
                            {
                                requestsOutList.Add(friend);
                            }
                        }

                        if (action == "REMOVEFR")
                        {
                            requestsOutList.Remove(friend);
                        }

                        friendsList = "";

                        foreach (string requestsO in requestsOutList)
                        {
                            friendsList = friendsList + GlobalVar.cValueR + requestsO;
                        }

                        requestOutReader.Close();                        

                        myCommand.CommandText = "UPDATE embyrfriends.friends set requestsout = :requestsout WHERE accountname = :accountname";

                        myCommand.Parameters.Add(new NpgsqlParameter(":accountname", user));

                        myCommand.Parameters.Add(new NpgsqlParameter(":requestsout", friendsList));

                        NpgsqlDataReader requestOutUpdater = myCommand.ExecuteReader();

                        requestOutUpdater.Read();

                        recipientUser = friend;

                        requestOutUpdater.Close();

                        //Friend Added to User's FriendsList

                        myCommand.CommandText = "SELECT * from embyrfriends.friends WHERE accountname = @accountnameR";

                        myCommand.Parameters.Add(new NpgsqlParameter(":accountnameR", recipientUser));

                        NpgsqlDataReader requestInReader = myCommand.ExecuteReader();

                        requestInReader.Read();

                        try
                        {
                            input = requestInReader[3].ToString(); //Fix here
                        }
                        catch
                        {
                            Console.WriteLine("User does not exist");
                        }

                        requestsInList = input.Split(GlobalVar.delimiterChars, System.StringSplitOptions.RemoveEmptyEntries).OfType<string>().ToList();

                        if (action == "ADDFR")
                        {
                            if (requestsInList.Contains(user) == false)
                            {
                                requestsInList.Add(user);
                            }
                        }

                        if (action == "REMOVEFR")
                        {
                            requestsInList.Remove(user);
                        }

                        friendsList = "";

                        foreach (string requestsI in requestsInList)
                        {
                            friendsList = friendsList + GlobalVar.cValueR + requestsI;
                        }

                        requestInReader.Close();

                        myCommand.CommandText = "UPDATE embyrfriends.friends set requestsin = :requestsin WHERE accountname = @accountnameR";

                        myCommand.Parameters.Add(new NpgsqlParameter(":accountnameR", recipientUser));

                        myCommand.Parameters.Add(new NpgsqlParameter(":requestsin", friendsList));

                        NpgsqlDataReader requestInUpdater = myCommand.ExecuteReader();

                        requestInUpdater.Read();

                        requestInUpdater.Close();

                        //User Added to Friend's FriendsList

                        connection.Close();
                    }
                }

                catch (NpgsqlException exc)
                {
                    Console.WriteLine(exc.ToString());
                }
            }
            else
            {
                Console.WriteLine("User does not exist");
            }
        }

        private void RemoveFriend(string user, string removeFriend)
        {         
            try
            {
                using (NpgsqlConnection connection = new NpgsqlConnection(Network.NpgSQLConnection))
                {

                    connection.Open();

                    NpgsqlCommand myCommand = new NpgsqlCommand("SELECT * from embyrfriends.friends WHERE accountname = @accountname", connection);

                    myCommand.Parameters.Add(new NpgsqlParameter(":accountname", user));

                    NpgsqlDataReader friendsReader = myCommand.ExecuteReader();

                    friendsReader.Read();

                    FriendRemover(friendsReader, user, removeFriend);

                    friendsReader.Close();

                    myCommand.CommandText = "SELECT * from embyrfriends.friends WHERE accountname = @accountname";

                    myCommand.Parameters.Add(new NpgsqlParameter(":accountname", removeFriend));

                    friendsReader = myCommand.ExecuteReader();

                    friendsReader.Read();

                    FriendRemover(friendsReader, removeFriend, user);

                    friendsReader.Close();

                    connection.Close();
                }
            }
            catch (NpgsqlException exc)
            {
                Console.WriteLine(exc.ToString());
            }
        }

        private void FriendRemover(NpgsqlDataReader friendsReader, string user, string friend)
        {
            List<string> usernames;

            string text = friendsReader[2].ToString();

            usernames = text.Split(GlobalVar.delimiterChars, System.StringSplitOptions.RemoveEmptyEntries).OfType<string>().ToList();

            foreach (var userU in usernames)
            {
                if (usernames.Contains(userU) == false)
                {
                    usernames.Add(userU);
                }
            }

            usernames.Remove(friend);

            string friendsList = "";

            foreach (string userC in usernames)
            {
                friendsList = friendsList + GlobalVar.cValueR + userC;
            }

            using (NpgsqlConnection connection = new NpgsqlConnection(Network.NpgSQLConnection))
            {

                connection.Open();

                NpgsqlCommand myCommand = new NpgsqlCommand("UPDATE embyrfriends.friends set friends = :friends WHERE accountname = :accountname", connection);

                myCommand.Parameters.Add(new NpgsqlParameter(":accountname", user));

                myCommand.Parameters.Add(new NpgsqlParameter(":friends", friendsList));

                NpgsqlDataReader friendsUpdater = myCommand.ExecuteReader();

                connection.Close();
            }
        }

        private void FriendAdder(string action, string username, string friend, int keyTag)
        {
            List<string> userFriendsSL;
            List<string> userRequestsSL;
            List<string> friendFriendsSL;
            List<string> friendRequestsSL;

            string userFriends = "";
            string userRequests = "";
            string friendFriends = "";
            string friendRequests = "";

            try
            {
                using (NpgsqlConnection connection = new NpgsqlConnection(Network.NpgSQLConnection))
                {

                    connection.Open();

                    NpgsqlCommand myCommand = new NpgsqlCommand("SELECT * from embyrfriends.friends WHERE accountname = @accountname", connection);

                    myCommand.Parameters.Add(new NpgsqlParameter(":accountname", username));

                    NpgsqlDataReader friendsReader = myCommand.ExecuteReader();

                    //Looking for user's friends

                    friendsReader.Read();

                    userFriends = friendsReader[2].ToString();

                    userRequests = friendsReader[3].ToString();

                    friendsReader.Close();

                    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

                    userFriendsSL = userFriends.Split(GlobalVar.delimiterChars, System.StringSplitOptions.RemoveEmptyEntries).OfType<string>().ToList();

                    userFriendsSL.Add(friend);

                    userFriends = "";

                    foreach (string k in userFriendsSL)
                    {
                        userFriends = userFriends + GlobalVar.cValueR + k;
                    }

                    userRequestsSL = userRequests.Split(GlobalVar.delimiterChars, System.StringSplitOptions.RemoveEmptyEntries).OfType<string>().ToList();

                    userRequestsSL.Remove(friend);

                    userRequests = "";

                    foreach (string k in userRequestsSL)
                    {
                        userRequests = userRequests + GlobalVar.cValueR + k;
                    }

                    myCommand.CommandText = "UPDATE embyrfriends.friends set friends = @friends, requestsin = @requestsin WHERE accountname = @accountnameR";

                    myCommand.Parameters.Add(new NpgsqlParameter(":accountnameR", username));

                    myCommand.Parameters.Add(new NpgsqlParameter(":friends", userFriends));

                    myCommand.Parameters.Add(new NpgsqlParameter(":requestsin", userRequests));

                    NpgsqlDataReader requestInUpdater = myCommand.ExecuteReader();

                    requestInUpdater.Read();

                    requestInUpdater.Close();

                    //Update for user complete!

                    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

                    myCommand.Parameters.Clear();

                    myCommand.CommandText = "SELECT * from embyrfriends.friends WHERE accountname = @accountnameF";

                    myCommand.Parameters.Add(new NpgsqlParameter(":accountnameF", friend));

                    NpgsqlDataReader friendsRReader = myCommand.ExecuteReader();

                    friendsRReader.Read();

                    friendFriends = friendsRReader[2].ToString();

                    friendRequests = friendsRReader[4].ToString();

                    friendsRReader.Close();

                    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

                    friendFriendsSL = friendFriends.Split(GlobalVar.delimiterChars, System.StringSplitOptions.RemoveEmptyEntries).OfType<string>().ToList();

                    friendFriendsSL.Add(username);

                    friendFriends = "";

                    foreach (string k in friendFriendsSL)
                    {
                        friendFriends = friendFriends + GlobalVar.cValueR + k;
                    }

                    friendRequestsSL = friendRequests.Split(GlobalVar.delimiterChars, System.StringSplitOptions.RemoveEmptyEntries).OfType<string>().ToList();

                    friendRequestsSL.Remove(username);

                    friendRequests = "";

                    foreach (string k in friendRequestsSL)
                    {
                        friendRequests = friendRequests + GlobalVar.cValueR + k;
                    }

                    myCommand.CommandText = "UPDATE embyrfriends.friends set friends = @friends, requestsout = @requestsout WHERE accountname = @accountnameR";

                    myCommand.Parameters.Add(new NpgsqlParameter(":accountnameR", friend));

                    myCommand.Parameters.Add(new NpgsqlParameter(":friends", friendFriends));

                    myCommand.Parameters.Add(new NpgsqlParameter(":requestsout", friendRequests));

                    NpgsqlDataReader requestOutUpdater = myCommand.ExecuteReader();

                    requestOutUpdater.Read();

                    requestOutUpdater.Close();

                    //Receiver update complete!

                    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

                    //Create PM Table

                    int recipientKeyTag = 0;
                    string directmessage = "";

                    recipientKeyTag = KeyTagID.GrabUserKeyTag(friend);

                    if (keyTag > recipientKeyTag)
                        directmessage = keyTag.ToString() + "28" + recipientKeyTag.ToString();

                    if (keyTag < recipientKeyTag)
                        directmessage = recipientKeyTag.ToString() + "28" + keyTag.ToString();

                    if (action == "in")
                    {
                        string pmKeyTag = numToAlpha(directmessage);
                        CreatePMAccess(pmKeyTag);
                    }

                    connection.Close();

                }
            }

            catch (NpgsqlException exc)
            {
                Console.WriteLine(exc.ToString());
            }
        }

        private void RequestRemover(string username, string friend)
        {
            List<string> userRequestsSL;
            List<string> friendRequestsSL;

            string userRequests = "";
            string friendRequests = "";

            try
            {
                using (NpgsqlConnection conn = new NpgsqlConnection(Network.NpgSQLConnection))
                {

                    conn.Open();

                    NpgsqlCommand myCommand = new NpgsqlCommand("SELECT * from embyrfriends.friends WHERE accountname = @accountname", conn);

                    myCommand.Parameters.Add(new NpgsqlParameter(":accountname", username));

                    NpgsqlDataReader friendsReader = myCommand.ExecuteReader();

                    friendsReader.Read();
                    
                    userRequests = friendsReader[3].ToString();

                    friendsReader.Close();

                    userRequestsSL = userRequests.Split(GlobalVar.delimiterChars, System.StringSplitOptions.RemoveEmptyEntries).OfType<string>().ToList();

                    userRequestsSL.Remove(friend);

                    userRequests = "";

                    foreach (string k in userRequestsSL)
                    {
                        userRequests = userRequests + GlobalVar.cValueR + k;
                    }

                    myCommand.CommandText = "UPDATE embyrfriends.friends set requestsin = @requestsin WHERE accountname = @accountname";

                    myCommand.Parameters.Add(new NpgsqlParameter(":accountname", username));

                    myCommand.Parameters.Add(new NpgsqlParameter(":requestsin", userRequests));

                    NpgsqlDataReader requestInUpdater = myCommand.ExecuteReader();

                    requestInUpdater.Read();

                    requestInUpdater.Close();

                    myCommand.CommandText = "SELECT * from embyrfriends.friends WHERE accountname = @accountnameF";

                    myCommand.Parameters.Add(new NpgsqlParameter(":accountnameF", friend));

                    NpgsqlDataReader friendsRReader = myCommand.ExecuteReader();

                    friendsRReader.Read();

                    friendRequests = friendsRReader[4].ToString();

                    friendsRReader.Close();

                    friendRequestsSL = friendRequests.Split(GlobalVar.delimiterChars, System.StringSplitOptions.RemoveEmptyEntries).OfType<string>().ToList();

                    friendRequestsSL.Remove(username);

                    friendRequests = "";

                    foreach (string k in friendRequestsSL)
                    {
                        friendRequests = friendRequests + GlobalVar.cValueR + k;
                    }

                    myCommand.CommandText = "UPDATE embyrfriends.friends set requestsout = @requestsout WHERE accountname = @accountnameF";

                    myCommand.Parameters.Add(new NpgsqlParameter(":accountnameF", friend));

                    myCommand.Parameters.Add(new NpgsqlParameter(":requestsout", friendRequests));

                    NpgsqlDataReader requestOutUpdater = myCommand.ExecuteReader();

                    requestOutUpdater.Read();

                    requestOutUpdater.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        private void CreatePMAccess(string pmKeyTag)
        {
            if (pmTableExistsCheck(pmKeyTag) == false)
            {
                try
                {
                    using (NpgsqlConnection connection = new NpgsqlConnection(Network.NpgSQLConnection))
                    {
                        connection.Open();

                        NpgsqlCommand myCommand = new NpgsqlCommand(@"CREATE TABLE embyrprivatemessages." + pmKeyTag + " (message text, username text, time text, ip text)", connection);

                        myCommand.ExecuteNonQuery();

                        connection.Close();
                    }
                }
                catch (NpgsqlException e)
                {
                    Console.WriteLine(e.ToString());
                }
            }
        }

        public static string numToAlpha(string directMessage)
        {
            string alpha = "abcdefghij";
            string pmKeyTag = "";

            for (int i = 0; i <= directMessage.Length - 1; i++)
            {
                pmKeyTag = pmKeyTag + alpha[Int32.Parse(directMessage[i].ToString())];
            }
            return pmKeyTag;
        }

        private bool pmTableExistsCheck(string pmKeyTag)
        {
            try
            {
                using (NpgsqlConnection connection = new NpgsqlConnection(Network.NpgSQLConnection))
                {
                    connection.Open();

                    NpgsqlCommand myCommand = new NpgsqlCommand("SELECT EXISTS (SELECT 1 FROM  pg_tables WHERE  schemaname = 'embyrprivatemessages' AND tablename = @tablename)", connection);

                    myCommand.Parameters.Add(new NpgsqlParameter(":tablename", pmKeyTag));

                    NpgsqlDataReader dr = myCommand.ExecuteReader();

                    dr.Read();

                    bool drBool = Convert.ToBoolean(dr[0].ToString());

                    dr.Close();

                    connection.Close();

                    return (drBool);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return false;
            }
        }

        public static string GetFriends(string username)
        {
            try
            {
                using (NpgsqlConnection connection = new NpgsqlConnection(Network.NpgSQLConnection))
                {
                    connection.Open();

                    NpgsqlCommand myCommand = new NpgsqlCommand("SELECT * from embyrfriends.friends WHERE accountname = @accountname", connection);

                    myCommand.Parameters.Add(new NpgsqlParameter(":accountname", username));

                    NpgsqlDataReader friendsReader = myCommand.ExecuteReader();

                    friendsReader.Read();

                    string friendsString = friendsReader[2].ToString();

                    friendsReader.Close();

                    return friendsString;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return null;
            }
        }

        public static string GetStatus(string username)
        {
            try
            {
                if (GetOnline(username) == true)
                {
                    using (NpgsqlConnection connection = new NpgsqlConnection(Network.NpgSQLConnection))
                    {
                        connection.Open();

                        NpgsqlCommand myCommand = new NpgsqlCommand("SELECT status from embyrfriends.friends WHERE accountname = @accountname", connection);

                        myCommand.Parameters.Add(new NpgsqlParameter(":accountname", username));

                        NpgsqlDataReader friendsReader = myCommand.ExecuteReader();

                        friendsReader.Read();

                        string friendsString = friendsReader[0].ToString();

                        friendsReader.Close();

                        return friendsString;
                    }
                }
                else
                {
                    return "offline";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return null;
            }
        }

        public static void SetStatus(string username, string status)
        {
            try
            {
                using (NpgsqlConnection connection = new NpgsqlConnection(Network.NpgSQLConnection))
                {
                    connection.Open();

                    NpgsqlCommand myCommand = new NpgsqlCommand("UPDATE embyrfriends.friends SET status = @status WHERE accountname = @accountname", connection);

                    myCommand.Parameters.Add(new NpgsqlParameter(":accountname", username));

                    myCommand.Parameters.Add(new NpgsqlParameter(":status", status));

                    NpgsqlDataReader friendsReader = myCommand.ExecuteReader();

                    friendsReader.Read();

                    friendsReader.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        public static bool GetOnline(string username)
        {
            try
            {
                using (NpgsqlConnection connection = new NpgsqlConnection(Network.NpgSQLConnection))
                {
                    connection.Open();

                    NpgsqlCommand myCommand = new NpgsqlCommand("SELECT online from embyrfriends.friends WHERE accountname = @accountname", connection);

                    myCommand.Parameters.Add(new NpgsqlParameter(":accountname", username));

                    NpgsqlDataReader friendsReader = myCommand.ExecuteReader();

                    friendsReader.Read();

                    bool online = Convert.ToBoolean(friendsReader[0].ToString());

                    friendsReader.Close();

                    return online;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return false;
            }
        }

        public static void SetOnline(string username, string boolS)
        {
            try
            {
                using (NpgsqlConnection connection = new NpgsqlConnection(Network.NpgSQLConnection))
                {
                    connection.Open();

                    NpgsqlCommand myCommand = new NpgsqlCommand("UPDATE embyrfriends.friends SET online = @boolS WHERE accountname = @accountname", connection);

                    myCommand.Parameters.Add(new NpgsqlParameter(":accountname", username));
                    myCommand.Parameters.Add(new NpgsqlParameter(":boolS", boolS));

                    NpgsqlDataReader onlineReader = myCommand.ExecuteReader();

                    onlineReader.Read();

                    onlineReader.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
    }
}