using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Newtonsoft.Json;

namespace SshScripter
{
    public partial class MainWindow 
    {
        private void ServerName_TextChanged(object sender, TextChangedEventArgs e)
        {

            var textBox = sender as TextBox;
            Config.Hostname = textBox.Text;
            Console.WriteLine(textBox.Text);
        }
        

        private void Port_OnGotFocus_GotFocus(object sender, RoutedEventArgs e)
        {
            var textBox = sender as TextBox;
            if (textBox.Text == "Port")
            {
                textBox.Text = "";
            }
        }

        private void Port_OnTextChanged_TextChanged(object sender, TextChangedEventArgs e)
        {
            var textBox = sender as TextBox;
            Config.Port = textBox.Text;

        }

        private void Username_OnGotFocusername_GotFocus(object sender, RoutedEventArgs e)
        {

            var textBox = sender as TextBox;
            if (textBox.Text == "Username")
            {
                textBox.Text = "";
            }
        }

        private void Username_OnTextChangedTextChanged(object sender, TextChangedEventArgs e)
        {
            var textBox = sender as TextBox;
            Config.Username = textBox.Text;

        }

        private void Password_OnGotFocus_GotFocus(object sender, RoutedEventArgs e)
        {

        }
        private void Password_Changed(object sender, RoutedEventArgs e)
        {

        }
        private void Software_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
        private void Server_Add(object sender, RoutedEventArgs e)
        {
            var selectedItem = ServerGroupDropdown.SelectedItem;
            ComboBoxItem cbi = ServerGroupDropdown.ItemContainerGenerator.ContainerFromItem(selectedItem) as ComboBoxItem;
            var json = JsonConvert.SerializeObject(cbi);
            dynamic dynJson = JsonConvert.DeserializeObject(json);
            Encryption decryptor = new Encryption();
            string[] arr1 = new string[] { Password.Password };
            string[] encryptedPassword = decryptor.StartEncryption(arr1);
            // string[] dec = decryptor.StartDecryption(encryptedPassword);
            ServerDB.ExecuteDB("INSERT INTO servers (`group_id`, `name`, `hostname`, `port`, `type`, `username`, `password`) VALUES (" +
                               dynJson.Content.Value + ", '" + ServerName.Text + "', '" + ServerName.Text + "', " + Port.Text + ", 'ssh', '" +
                               Username.Text + "', '" + encryptedPassword[0] + "')");
            ServerList.Items.Clear();
            PopulateList();

        }

        private void ServerGroupDropdown_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {


        }
    }
}
