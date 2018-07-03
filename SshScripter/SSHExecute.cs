using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using Newtonsoft.Json;
using Renci.SshNet;
using Renci.SshNet.Messages.Authentication;

namespace SshScripter
{
    public  class SSHExecute
    {
        public static string[] password;

        public static string RunCommands()
        {



                var serverListResults = ServerDB.ExecuteDB("SELECT * FROM servers");
                var serversJSON = JsonConvert.SerializeObject(serverListResults);
                dynamic servers = JsonConvert.DeserializeObject(serversJSON);

                foreach (var server in servers)
                {
                    Console.WriteLine("Connecting to: " + server.name);
                    string[] password = new string[] { server.password };
                    Encryption decryptor = new Encryption();
                    string[] decryptedPassword = decryptor.StartDecryption(password);
                    string serverPassword = decryptedPassword[0];
                    string hostname = server.hostname;
                    string username = server.username;
                    string groupID = server.group_id;


                using (var client = new SshClient(hostname, username, serverPassword))
                    {
                        try
                        {
                            client.Connect();
                            var serverCommandsList = ServerDB.ExecuteDB("SELECT * FROM commands where group_id = " + groupID);
                            var json = JsonConvert.SerializeObject(serverCommandsList);
                            dynamic dynJson = JsonConvert.DeserializeObject(json);
                            foreach(var command in dynJson) {
                                var cmd = command.command.ToString();
                                Console.WriteLine("Command>" + cmd);
                                var run = client.CreateCommand(cmd);
                                run.Execute();
                                Console.WriteLine("Return Value = {0}", run.Result);
                                client.Disconnect();
                                string returnedResults = run.Result.ToString();
                                ServerDB.ExecuteDB("UPDATE commands set response = '" + returnedResults + "' WHERE id = " + command.id);

                            }
                        }
                        catch
                        {
                            Console.WriteLine("Fail");
                            return "Failure";
                        }
                    }


                }

            return "true";

        }
    }

}
