using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace SshScripter
{
    public partial class MainWindow 
    {
        public void PopulateList()
        {
            ServerList.Items.Clear();
            TreeViewItem treeItem = null;


  
            var serverGroupList = ServerDB.ExecuteDB("SELECT * FROM server_groups");
            var json = JsonConvert.SerializeObject(serverGroupList);
            dynamic dynJson = JsonConvert.DeserializeObject(json);
            ServerGroupDropdown.Items.Clear();
            foreach (var item in dynJson)
            {
                Console.WriteLine("Adding ServerGroup: " + item.name);
                ComboboxItem DropdownItem = new ComboboxItem();
                DropdownItem.Text = item.name.ToString();
                DropdownItem.Value = item.id;
                ServerGroupDropdown.Items.Add(DropdownItem);
                
                treeItem = new TreeViewItem();
                treeItem.Header = item.name;
                treeItem.Uid = item.id;
                treeItem.DataContext = item.id.ToString();
                treeItem.MouseUp += ServerList_NodeMouseClick;

                var serverListResults = ServerDB.ExecuteDB("SELECT * FROM servers WHERE group_id = " + item.id);
                var serversJSON = JsonConvert.SerializeObject(serverListResults);
                dynamic servers = JsonConvert.DeserializeObject(serversJSON);
                foreach (var server in servers)
                {
                    treeItem.Items.Add(new TreeViewItem() { Header = server.name, Uid = server.id, DataContext = server.group_id.ToString() });
                }
                ServerList.Items.Add(treeItem);
                
            }
            foreach (object item in ServerList.Items)
            {
                TreeViewItem treeItems = (TreeViewItem)item;
                if (treeItems != null)
                {
                    ExpandAllNodes(treeItems);
                    treeItems.IsExpanded = true;
                }
            }
        }
        private void ExpandAllNodes(TreeViewItem rootItem)
        {
            foreach (object item in rootItem.Items)
            {
                TreeViewItem treeItem = (TreeViewItem)item;
                if (treeItem != null)
                {
                    ExpandAllNodes(treeItem);
                    treeItem.IsExpanded = true;
                }
            }
        }
    }

    public class ComboboxItem
    {
        public string Text { get; set; }
        public object Value { get; set; }

        public override string ToString()
        {
            return Text;
        }
    }
}
