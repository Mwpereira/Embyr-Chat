using Npgsql;
using System;
using System.IO;
using Embyr.Global;
using Embyr.Main;
using Embyr.Operations;

namespace HTTPServerHost
{
    class AccountInformation
    {
        public static bool DeleteAccount(string request)
        {
            string[] requestArr = request.Split(GlobalVar.delimiterChars, System.StringSplitOptions.RemoveEmptyEntries);
            try
            {
                using (NpgsqlConnection deleteConnection = new NpgsqlConnection(Network.NpgSQLConnection))
                {
                    NpgsqlCommand command;

                    deleteConnection.Open();

                    string[] friends = FriendsList.GetFriends(requestArr[1]).Split(GlobalVar.delimiterChars, System.StringSplitOptions.RemoveEmptyEntries);

                    command = new NpgsqlCommand("DELETE FROM embyraccounts.accounts WHERE accountname = @accountname;", deleteConnection);
                    command.Parameters.Add(new NpgsqlParameter("@accountname", requestArr[1]));
                    NpgsqlDataReader dataReader = command.ExecuteReader();
                    dataReader.Close();

                    command.CommandText = "DELETE FROM embyrfriends.friends WHERE accountname = @accountname;";
                    command.Parameters.Add(new NpgsqlParameter("@accountname", requestArr[1]));
                    command.ExecuteNonQuery();

                    command.CommandText = "DELETE FROM embyrusers.users WHERE accountname = @accountname;";
                    command.Parameters.Add(new NpgsqlParameter("@accountname", requestArr[1]));
                    command.ExecuteNonQuery();

                    File.Delete(Path.Combine(@"C:\Users\Michael\Desktop\HTTPServerHost\server\Profile Photos\", requestArr[1]+".txt"));
                                      
                    foreach(string friend in friends)
                    {
                        FriendsList fl = new FriendsList("REMOVE", requestArr[1],friend);
                    }

                    deleteConnection.Close();

                    return true;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                return false;
            }
        }

        public static bool UpdateAccount(string request)
        {
            string[] requestArr = request.Split(GlobalVar.delimiterChars, System.StringSplitOptions.RemoveEmptyEntries);
            try
            {
                using (NpgsqlConnection updateConnection = new NpgsqlConnection(Network.NpgSQLConnection))
                {
                    NpgsqlCommand command;

                    updateConnection.Open();
                            command = new NpgsqlCommand("UPDATE embyraccounts.accounts set password = :password WHERE accountname = :accountname", updateConnection);
                            Encrypt.encryptText = requestArr[1];
                            requestArr[1] = Encrypt.GlobalVar;
                            command.Parameters.Add(new NpgsqlParameter("@accountname", requestArr[1]));
                            Encrypt.encryptText = requestArr[5];
                            requestArr[5] = Encrypt.GlobalVar;
                            command.Parameters.Add(new NpgsqlParameter("@password", requestArr[5]));
                            NpgsqlDataReader dataReader = command.ExecuteReader();
                    updateConnection.Close();

                    return true;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                return false;
            }
        }
    }
}
