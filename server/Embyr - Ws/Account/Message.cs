using Npgsql;
using System;
using System.Threading.Tasks;
using Embyr.Global;
using Embyr.Main;
using Embyr.Operations;

namespace WebSocketServer.Account
{
    class Message
    {
        public static async Task StoreMessage(string sender, string receiver, string message)
        {
            string pmKeyTag = KeyTagID.GetPMKeyTag(sender, receiver);
            string time = GlobalVar.currentDate();

            using (NpgsqlConnection connection = new NpgsqlConnection(Network.NpgSQLConnection))
            {
                connection.Open();

                NpgsqlCommand insertPMCommand = null;

                try
                {
                    insertPMCommand = new NpgsqlCommand("INSERT INTO embyrprivatemessages." + pmKeyTag + " (message, username, time) values(:message, :username, :time)", connection);
                    insertPMCommand.Parameters.Add(new NpgsqlParameter(":message", message));
                    insertPMCommand.Parameters.Add(new NpgsqlParameter(":username", sender));
                    insertPMCommand.Parameters.Add(new NpgsqlParameter(":time", time));
                    NpgsqlDataReader dataReader = await insertPMCommand.ExecuteReaderAsync();
                    await dataReader.ReadAsync();
                }
                catch (NpgsqlException e)
                {
                    Console.WriteLine(e.ToString());
                }
                connection.Close();
            }
        }
    }
}
