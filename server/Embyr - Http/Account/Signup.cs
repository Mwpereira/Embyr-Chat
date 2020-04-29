using Npgsql;
using System;
using Embyr.Global;
using Embyr.Main;
using Embyr.Operations;

namespace HTTPServerHost
{
    class Signup
    {
        public static bool CreateAccount(string request)
        {
            string[] postReqKV = request.Split(GlobalVar.delimiterChars, System.StringSplitOptions.RemoveEmptyEntries);

            string username = postReqKV[1];
            string password = postReqKV[3];
            string ipaddress = postReqKV[5];

            string usernameEncrypt;
            string time = GlobalVar.currentDate();

            if (!(KeyTagID.GetUserExists(username)))
            {
                try
                {
                    Random rand = new Random();

                    int keytag = rand.Next(0, 10000);

                    while (KeyTagID.CheckKeyTag(keytag) == true)
                    {
                        keytag = rand.Next(0, 10000);
                        if (KeyTagID.CheckKeyTag(keytag) == true)
                            continue;
                        else
                            break;
                    }

                    using (NpgsqlConnection signupConnection = new NpgsqlConnection(Network.NpgSQLConnection))
                    {
                        signupConnection.Open();

                        NpgsqlCommand command = new NpgsqlCommand("INSERT INTO embyraccounts.accounts (keyid,accountname,password,datecreated,ipaddress) values(:keytag,:usernameEncrypt,:password,:datecreated,:ipaddress)", signupConnection);
                        command.Parameters.Add(new NpgsqlParameter("@keytag", keytag));
                        Encrypt.encryptText = username;
                        usernameEncrypt = Encrypt.GlobalVar;
                        command.Parameters.Add(new NpgsqlParameter("@usernameEncrypt", usernameEncrypt));
                        Encrypt.encryptText = password;
                        password = Encrypt.GlobalVar;
                        command.Parameters.Add(new NpgsqlParameter("@password", password));
                        command.Parameters.Add(new NpgsqlParameter("@datecreated", time));
                        command.Parameters.Add(new NpgsqlParameter("@ipaddress", ipaddress));
    
                        NpgsqlDataReader dataReader = command.ExecuteReader();
                        dataReader.Close();

                        command.CommandText = "INSERT INTO embyrfriends.friends (keyid, accountname, friends,  requestsin, requestsout, status, online) VALUES (:keytag, :username, '',  '', '', 'online', 'false')";
                        command.Parameters.Add(new NpgsqlParameter("@keytag", keytag));
                        command.Parameters.Add(new NpgsqlParameter("@username", username));
                        command.ExecuteNonQuery();

                        command.CommandText = "INSERT INTO embyrusers.users (keyid, accountname) VALUES (:keytag, :username)";
                        command.Parameters.Add(new NpgsqlParameter("@keytag", keytag));
                        command.Parameters.Add(new NpgsqlParameter("@username", username));
                        command.ExecuteNonQuery();

                        signupConnection.Close();

                        try
                        {
                            string sourceFile = System.IO.Path.Combine(@"C:\Users\Michael\Desktop\HTTPServerHost\server\Profile Photos\", "default.txt");
                            string destFile = System.IO.Path.Combine(@"C:\Users\Michael\Desktop\HTTPServerHost\server\Profile Photos\", username + ".txt");
                            System.IO.File.Copy(sourceFile, destFile, true);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e.ToString());
                        }
                        return true;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                    return false;
                }
            }
            else
                return false;
        }
    }
}
