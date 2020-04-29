using Npgsql;
using System;
using System.Collections.Generic;
using Embyr.Main;

namespace HTTPServerHost.Account
{
    class Update
    {
        public static List<string> GetUpdates()
        {
            List<string> updatesObj = new List<string>();
            try
            {
                using (NpgsqlConnection conn = new NpgsqlConnection(Network.NpgSQLConnection))
                {
                    conn.Open();

                    NpgsqlCommand getUpdatesCommand = new NpgsqlCommand("SELECT * FROM embyrupdates.updates ORDER BY date DESC LIMIT 1", conn);

                    NpgsqlDataReader getUpdatesReader = getUpdatesCommand.ExecuteReader();

                    getUpdatesReader.Read();

                    updatesObj.Add(getUpdatesReader[0].ToString());
                    updatesObj.Add(getUpdatesReader[1].ToString());
                    updatesObj.Add(getUpdatesReader[2].ToString());
                    updatesObj.Add(getUpdatesReader[3].ToString());
                    updatesObj.Add(getUpdatesReader[4].ToString());

                    return updatesObj;
                }
            }
            catch (NpgsqlException e)
            {
                Console.WriteLine(e.ToString());
                return null;
            }        
        }        
    }
}
