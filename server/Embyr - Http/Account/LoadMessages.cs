using Npgsql;
using System;
using Embyr.Main;
using Embyr.Operations;

namespace HTTPServerHost
{
    class LoadMessages
    {
        public static Tuple<string[], string[], string[]> StartupLoadMessages(string senderName, string receiverName, int n)
        {
            try
            {
                string[] message = new string[0];
                string[] sender = new string[0];
                string[] time = new string[0];

                string pmKeyTag = KeyTagID.GetPMKeyTag(senderName, receiverName);

                NpgsqlConnection connMsg = new NpgsqlConnection(Network.NpgSQLConnection);

                connMsg.Open();

                NpgsqlCommand getMessagesCountCommand = new NpgsqlCommand("SELECT count(*) FROM embyrprivatemessages." + pmKeyTag, connMsg);

                Int64 msgCnt = (Int64)getMessagesCountCommand.ExecuteScalar();

                connMsg.Close();

                if (msgCnt != n || msgCnt == 0)
                {
                    if (n != 0)
                        msgCnt = msgCnt - n;

                    if (msgCnt >= 15)
                    {
                        message = new string[15];
                        sender = new string[15];
                        time = new string[15];
                    }

                    else
                    {
                        message = new string[msgCnt];
                        sender = new string[msgCnt];
                        time = new string[msgCnt];
                    }

                    try
                    {
                        using (NpgsqlConnection conn = new NpgsqlConnection(Network.NpgSQLConnection))
                        {
                            conn.Open();

                            NpgsqlCommand getMessagesCommand = new NpgsqlCommand("SELECT * FROM(SELECT message, username, time, ROW_NUMBER() OVER(ORDER BY time) FROM embyrprivatemessages." + pmKeyTag + ") x WHERE ROW_NUMBER BETWEEN " + (msgCnt - 14).ToString() + " AND " + msgCnt.ToString(), conn);

                            NpgsqlDataReader getMessagesReader = getMessagesCommand.ExecuteReader();

                            int i = 0;

                            while (getMessagesReader.Read())
                            {
                                message[i] = getMessagesReader[0].ToString();
                                sender[i] = getMessagesReader[1].ToString();
                                time[i] = getMessagesReader[2].ToString().Substring(0, 14) + DateTime.Parse(getMessagesReader[2].ToString().Remove(14, 1).Substring(14)).ToString(@" h\:mm tt");

                                if (i < message.Length - 1)
                                    i++;
                            }

                            getMessagesReader.Close();

                            var tuple = new Tuple<string[], string[], string[]>(message, sender, time);

                            return tuple;
                        }
                    }

                    catch (NpgsqlException ex)
                    {
                        Console.WriteLine(ex.ToString());
                        return null;
                    }

                }
                return (new Tuple<string[], string[], string[]>(message, sender, time));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return null;
            }
        }                   
    }
}
