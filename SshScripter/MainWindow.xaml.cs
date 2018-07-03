using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Newtonsoft.Json;
using Renci.SshNet;

namespace SshScripter
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 


    public partial class MainWindow : Window
    {

        public static SshClient client;
        public static string[] password;
        public static ShellStream sh;
        public static StreamReader reader;
        public  string row_id { get; set; }

        public MainWindow()
        {

            if (!File.Exists("zzz.sqlite"))
            {
                SQLiteConnection.CreateFile("zzz.sqlite");
                ServerDB.ExecuteDB(
                    "create table IF NOT EXISTS  servers (id INTEGER PRIMARY KEY AUTOINCREMENT, group_id int, name varchar(60), hostname varchar(60), port int, type varchar(6), username varchar(60), password varchar(200), description varchar(60))");
                ServerDB.ExecuteDB(
                    "create table IF NOT EXISTS server_groups (id  INTEGER PRIMARY KEY AUTOINCREMENT, name varchar(60))");
                ServerDB.ExecuteDB(
                    "CREATE TABLE IF NOT EXISTS 'commands'( `id` INTEGER PRIMARY KEY AUTOINCREMENT, `group_id` INTEGER, `command` TEXT, `expected_response` TEXT, `response` TEXT, `sort_order` INTEGER)");
                ServerDB.ExecuteDB(
                    "CREATE TABLE IF NOT EXISTS `software` (`id`	INTEGER PRIMARY KEY AUTOINCREMENT,`group_id`	INTEGER,`name`	TEXT)");
                ServerDB.ExecuteDB("INSERT INTO server_groups VALUES (1, 'Server Group')");
                //ServerDB.ExecuteDB("INSERT INTO servers VALUES (1, 1, 'localhost', '127.0.0.1', 22, 'ssh', 'root', '', '')");
                //ServerDB.ExecuteDB("INSERT INTO software VALUES (1, 1, 'Shell')");

            }

            WindowStartupLocation = WindowStartupLocation.Manual;
            Left = -4500;
            Top = -966;
            InitializeComponent();
            PopulateList();
            SoftwareSelection.SelectedIndex = 0;

            //using (var client = new SshClient(hostname, username, serverPassword))
            //{
            //    client.Connect();
            //    var sh = client.CreateShellStream("alan", 0, 0, 0,0,0);
            //    var reader = new StreamReader(sh);
            //    sh.WriteLine("mysql");
            //    Thread.Sleep(500);
            //    sh.WriteLine("show databases;");
            //    Thread.Sleep(500);

            //    Console.WriteLine(reader.ReadToEnd());
            //    //sh.WriteLine("quit;");
            //    client.Disconnect();
            //}

        }



        private void TestLogin_OnClick(object sender, RoutedEventArgs e)
        {
            SSHExecute.RunCommands();
        }

        //public void SoftwareDropdown()
        //{
        //    SoftwareListDropdown.Items.Clear();
        //    TreeViewItem nodeItem = ServerList.SelectedItem as TreeViewItem;

        //    DataTable SoftwareList = ServerDB.ExecuteDB("SELECT * FROM software WHERE group_id = " + nodeItem.Uid ?? "1");
        //    var softwarejson = JsonConvert.SerializeObject(SoftwareList);
        //    dynamic softwaredynJson = JsonConvert.DeserializeObject(softwarejson);
        //    foreach (var softwareitem in softwaredynJson)
        //    {
        //        ComboboxItem SoftwareDropdownItem = new ComboboxItem();
        //        SoftwareDropdownItem.Text = softwareitem.name.ToString();
        //        SoftwareDropdownItem.Value = softwareitem.id;
        //        SoftwareListDropdown.Items.Add(SoftwareDropdownItem);


        //    }
        //}

        //private void SoftwareListDropdown_SelectionChanged(object sender, SelectionChangedEventArgs e)
        //{
        //    TreeViewItem nodeItem = ServerList.SelectedItem as TreeViewItem;
        //    var thisitem = SoftwareListDropdown.Items;
        //    Console.WriteLine(thisitem.ToString());
        //    //    DataTable CommandList = ServerDB.ExecuteDB("SELECT * FROM commands WHERE group_id = " + nodeItem.Uid);
        //    //    CommandListGrid.ItemsSource = CommandList.DefaultView;
        //    //

        //}

        private void CommandListGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //DataRowView rowview = CommandListGrid.SelectedItem as DataRowView;
            //if(rowview != null) { 
            //string id = rowview.Row[0].ToString()??null;
            //Console.WriteLine(id);
            //}
        }

        public static List<CommandGridItems> items = new List<CommandGridItems>();

        private void CommandsTab_OnMouseLeftButtonUpTabClick(object sender, MouseButtonEventArgs e)
        {
            //string[] commandList = new string[] {"hostname","uname"};
            //string hostname = "alan.coursesaver.com";
            //string username = "root";
            //Encryption decryptor = new Encryption();
            //string[] decryptedPassword = decryptor.StartDecryption(Config.Password);
            //string serverPassword = decryptedPassword[0];
            //using (var client = new SshClient(hostname, username, serverPassword))
            //{
            //    try
            //    {
            //        client.Connect();
            //        foreach(string sCommand in commandList) {
            //            var run = client.CreateCommand(sCommand);
            //            run.Execute();
            //            Console.WriteLine("Return Value = {0}", run.Result);
            //            returnedResults = run.Result;
            //            items.Insert(items.Count, new CommandGridItems() { Delete = true, Command = sCommand, Response = returnedResults, Results = "" });
            //            CommandRows2.ItemsSource = items;
            //            CommandRows2.Items.Refresh();
            //        }
            //        client.Disconnect();
            //    }
            //    catch
            //    {
            //        Console.WriteLine("Fail");
            //    }
            //}
        }

        string ReturnedString;

        private void ExecuteSingleCommand(object sender, RoutedEventArgs e)
        {
            ExecuteCommand();
        }

        private void SSHCommand_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key != System.Windows.Input.Key.Enter) return;
            ExecuteCommand();
        }

        private void ExecuteCommand()
        {
            if (client == null)
            {
                MessageBox.Show("Select a server from the list to the left and click 'Connect' before continuing.",
                    "Not Connected");
                return;
            }

            string currentGroupId = Config.groupId ?? "1";
            string sshCommand = CommandTextbox.Text;
            string originalCommand = CommandTextbox.Text;
            sshCommand = sshCommand.Replace("[[Username]]", Regex_Username.Text ?? "");
            sshCommand = sshCommand.Replace("[[OldPassword]]", Regex_Old_Password.Text ?? "");
            sshCommand = sshCommand.Replace("[[NewPassword]]", Regex_New_Password.Text ?? "");
            sshCommand = sshCommand.Replace("[[Custom1]]", Regex_Custom_1.Text ?? "");
            sshCommand = sshCommand.Replace("[[Custom2]]", Regex_Custom_2.Text ?? "");
            sshCommand = sshCommand.Replace("[[Custom3]]", Regex_Custom_3.Text ?? "");

            try
            {
                Thread.Sleep(100);
                reader.ReadToEnd();
                Thread.Sleep(100);
                sh.WriteLine(sshCommand);
                Thread.Sleep(500);
                ReturnedString = reader.ReadToEnd();

                Console.WriteLine(ReturnedString);

                Console.WriteLine("Return Value = {0}", ReturnedString);
            }
            catch
            {
                Console.WriteLine("Fail");
            }

            ServerDB.ExecuteDB("INSERT INTO commands ('group_id', 'command', 'expected_response') VALUES ('" +
                               currentGroupId + "', '" + originalCommand + "', '" + ReturnedString.Replace("'", "''") +
                               "');");
            items.Insert(items.Count,
                new CommandGridItems()
                {
                    Delete = false,
                    Command = sshCommand,
                    Response = ReturnedString.Replace("'", "''"),
                    Results = ""
                });
            CommandRows2.ItemsSource = items;
            CommandRows2.Items.Refresh();
            PopulateDataGrid();
        }

        public void PopulateDataGrid()
        {
            items = new List<CommandGridItems>();
            var serverCommandsList = ServerDB.ExecuteDB("SELECT * FROM commands where group_id = " + Config.groupId);
            var json = JsonConvert.SerializeObject(serverCommandsList);
            dynamic dynJson = JsonConvert.DeserializeObject(json);
            foreach (var command in dynJson)
            {
                items.Insert(items.Count,
                    new CommandGridItems()
                    {
                        Delete = false,
                        Command = command.command.ToString(),
                        Response = command.expected_response.ToString(),
                        Results = "",
                        Command_ID = command.id.ToString()
                    });
            }

            CommandRows2.ItemsSource = items;
            CommandRows2.Items.Refresh();

        }

        private void Window_Resized(object sender, SizeChangedEventArgs e)
        {
            double width = CommandsBottomGrid.ColumnDefinitions[0].ActualWidth;
            SizeChangedEventHandler eventHandler = null;
            CommandTextbox.Width = width * 0.8;
        }

        public void SSH_Connect(object sender, RoutedEventArgs e)
        {
            Connect();
            if (client != null)
            {
                if (client.IsConnected)
                {
                    sh = client.CreateShellStream("alan", 0, 0, 0, 0, 0);
                    reader = new StreamReader(sh);
                    var image = "/Resources/connection-status-on.png";
                    ConnectedIcon.Source = new BitmapImage(new Uri("pack://application:,,," + image, UriKind.Absolute));
                    ServerDisconnect.IsEnabled = true;
                    ServerConnect.IsEnabled = false;
                }
            }
        }
    

    public  void SSH_Disconnect(object sender, RoutedEventArgs e)
        {
            if (client != null)
            {
                client.Disconnect();
                if (!client.IsConnected)
                {
                    var image = "/Resources/connection-status-off.png";
                    ConnectedIcon.Source = new BitmapImage(new Uri("pack://application:,,," + image, UriKind.Absolute));
                    ServerDisconnect.IsEnabled = false;
                    ServerConnect.IsEnabled = true;
                }
            }
        }

        public static SshClient Connect()
        {
            string hostname = Config.Hostname;
            string username = Config.Username;
            Encryption decryptor = new Encryption();
            string[] decryptedPassword = decryptor.StartDecryption(Config.Password);
            if (decryptedPassword ==null)
            {
                return null;
            }

            string serverPassword = decryptedPassword[0];
            client = new SshClient(hostname, username, serverPassword);
            client.Connect();
            Console.WriteLine(client.IsConnected);
            return client;
        }

        private void Delete_Commands(object sender, RoutedEventArgs e)
        {
            foreach (CommandGridItems item in CommandRows2.ItemsSource)
            {
                if (((CheckBox)Delete.GetCellContent(item)).IsChecked == true)
                {
                    var rowId = ID.GetCellContent(item) as TextBlock;
                    ((CheckBox) Delete.GetCellContent(item)).Uid = rowId.Text;
                    ServerDB.ExecuteDB("DELETE FROM commands where id = " + rowId.Text);
                    PopulateDataGrid();
                }
            }
        }

        private void Add_Group_Submit(object sender, RoutedEventArgs e)
        {
            var Group = Group_Textbox.Text;
            ServerDB.ExecuteDB("INSERT INTO server_groups ('name') VALUES ('" + Group + "');");
            PopulateList();

        }

        private void Add_Software_Submit(object sender, RoutedEventArgs e)
        {
            var Software = AddSoftwareTextBox.Text;
            ServerDB.ExecuteDB("INSERT INTO software ('group_id', 'name') VALUES ('" + Config.groupId + "','" + Software + "');");
        }

        public void SSH_Disconnect(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (client !=null)
            {
                client.Disconnect();
            }
        }
    }
    public class CommandGridItems
    {
        public bool Delete { get; set; }
        public string Command_ID { get; set; }
        public string Command { get; set; }
        public string Response { get; set; }
        public string Results { get; set; }

    }

}
