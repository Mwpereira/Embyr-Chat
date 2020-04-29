using Npgsql;
using System;
using Embyr.Global;
using Embyr.Main;
using Embyr.Operations;

namespace HTTPServerHost
{
    class Login
    {
        public static bool SignIn(string request)
        {
            string[] postReqKV = request.Split(GlobalVar.delimiterChars, System.StringSplitOptions.RemoveEmptyEntries);

            string usrTemp = postReqKV[1];

            string passTemp = postReqKV[3];

            Encrypt.encryptText = usrTemp;
            usrTemp = Encrypt.GlobalVar;

            string pT = NpgSQLLogin(usrTemp);
                       
            if (pT != "error")
            {
                Encrypt.encryptText = passTemp;
                if (Encrypt.GlobalVar == pT)
                {                    
                    FriendsList.SetOnline(postReqKV[1], "true");
                    return true;
                }
                else
                {
                    return false;
                }
            }
            return false;
        }

        private static string NpgSQLLogin(string username)
        {
            string usrExists = "error";
            try
            {
                using (NpgsqlConnection conn = new NpgsqlConnection(Network.NpgSQLConnection))
                {
                    conn.Open();

                    NpgsqlCommand getPasswordCommand = new NpgsqlCommand("SELECT password FROM embyraccounts.accounts WHERE accountname = :accountname", conn);

                    getPasswordCommand.Parameters.Add(new NpgsqlParameter("@accountname", username));

                    NpgsqlDataReader passwordReader = getPasswordCommand.ExecuteReader();

                    while (passwordReader.Read())
                    {
                        return passwordReader[0].ToString();
                    }
                    return usrExists;
                }
            }
            catch (NpgsqlException e)
            {
                Console.WriteLine(e.ToString());
                return usrExists;
            }
        }
    }
}
