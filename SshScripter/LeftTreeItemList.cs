using System;
using System.Data;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using Newtonsoft.Json;
using DragDropEffects = System.Windows.DragDropEffects;
using DragEventArgs = System.Windows.DragEventArgs;
using MouseEventArgs = System.Windows.Input.MouseEventArgs;

namespace SshScripter
{

    public partial class MainWindow 
    {
        public event TreeNodeMouseClickEventHandler NodeMouseClick;

        private void ServerList_NodeMouseClick(object sender, MouseButtonEventArgs e)
        {
            TreeViewItem nodeItem = ServerList.SelectedItem as TreeViewItem;
            if (nodeItem == null)
            {
                return;
            }

            Config.groupId = nodeItem.DataContext.ToString();
            DataTable Servers = ServerDB.ExecuteDB("SELECT * FROM servers WHERE group_id = " + nodeItem.DataContext ?? "1");
            var serversJSON = JsonConvert.SerializeObject(Servers);
            dynamic server = JsonConvert.DeserializeObject(serversJSON);
            Config.Hostname = server[0]["hostname"];
            Config.Username = server[0]["username"];
            Config.Password = new String[] {server[0]["password"] };
            Config.Port = server[0]["port"];
            ActiveServer.Content = "Connect to server: " + Config.Hostname;
            Console.WriteLine(Config.Hostname);
            DataTable CommandList = ServerDB.ExecuteDB("SELECT * FROM commands WHERE group_id = " + nodeItem.DataContext ?? "1");
            //CommandListGrid.ItemsSource = CommandList.DefaultView;
            DataTable SoftwareList = ServerDB.ExecuteDB("SELECT * FROM software WHERE group_id = " + nodeItem.DataContext ?? "1");
            SoftwareListGrid.ItemsSource = SoftwareList.DefaultView;
            PopulateDataGrid();
            Username.Text = Config.Username;
            Port.Text = Config.Port;
            ServerName.Text = Config.Hostname;

            //ServerGroupDropdown.SelectedIndex = 1;
            //SoftwareDropdown();
        }
        private Point _lastMouseDown;
        private TreeViewItem draggedItem, _target;

        private void TreeView_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                _lastMouseDown = e.GetPosition(ServerList);
            }
        }
        
        private void treeView_MouseMove(object sender, MouseEventArgs e)
        {
            try
            {
                if (e.LeftButton == MouseButtonState.Pressed)
                {

                    Point currentPosition = e.GetPosition(ServerList);

                    if ((Math.Abs(currentPosition.X - _lastMouseDown.X) > 10.0) ||
                        (Math.Abs(currentPosition.Y - _lastMouseDown.Y) > 10.0))
                    {

                        draggedItem = (TreeViewItem)ServerList.SelectedItem;
                        if (draggedItem != null)
                        {
                            DragDropEffects finalDropEffect = DragDrop.DoDragDrop(ServerList, ServerList.SelectedValue, DragDropEffects.Move);
                            //Checking target is not null and item is
                            //dragging(moving)
                            if ((finalDropEffect == DragDropEffects.Move) && (_target != null))
                            {
                                // A Move drop was accepted
                                if (!draggedItem.Header.ToString().Equals(_target.Header.ToString()))
                                {
                                    CopyItem(draggedItem, _target);
                                    _target = null;
                                    draggedItem = null;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
            }
        }
        private void treeView_DragOver(object sender, DragEventArgs e)
        {
            try
            {
                Point currentPosition = e.GetPosition(ServerList);

                if ((Math.Abs(currentPosition.X - _lastMouseDown.X) > 10.0) ||
                   (Math.Abs(currentPosition.Y - _lastMouseDown.Y) > 10.0))
                {
                    // Verify that this is a valid drop and then store the drop target
                    TreeViewItem item = GetNearestContainer
                    (e.OriginalSource as UIElement);
                    if (CheckDropTarget(draggedItem, item))
                    {
                        e.Effects = DragDropEffects.Move;
                    }
                    else
                    {
                        e.Effects = DragDropEffects.None;
                    }
                }
                e.Handled = true;
            }
            catch (Exception)
            {
            }
        }

        private void treeView_Drop(object sender, DragEventArgs e)
        {
            try
            {
                e.Effects = DragDropEffects.None;
                e.Handled = true;

                // Verify that this is a valid drop and then store the drop target
                TreeViewItem TargetItem = GetNearestContainer
                    (e.OriginalSource as UIElement);
                if (TargetItem != null && draggedItem != null)
                {
                    _target = TargetItem;
                    e.Effects = DragDropEffects.Move;
                    PopulateList();
                }
            }
            catch (Exception)
            {
            }
        }

        private bool CheckDropTarget(TreeViewItem _sourceItem, TreeViewItem _targetItem)
        {
            //Check whether the target item is meeting your condition
            bool _isEqual = false;
            if (!_sourceItem.Header.ToString().Equals(_targetItem.Header.ToString()))
            {
                _isEqual = true;
            }
            return _isEqual;

        }

        private void CopyItem(TreeViewItem _sourceItem, TreeViewItem _targetItem)
        {

            try
            {
                //adding dragged TreeViewItem in target TreeViewItem
                addChild(_sourceItem, _targetItem);
                ServerDB.ExecuteDB("UPDATE servers SET group_id = " + _targetItem.Uid + " WHERE id = " + _sourceItem.Uid);
                //finding Parent TreeViewItem of dragged TreeViewItem 
                TreeViewItem ParentItem = FindVisualParent<TreeViewItem>(_sourceItem);
                // if parent is null then remove from TreeView else remove from Parent TreeViewItem
                if (ParentItem == null)
                {
                    ServerList.Items.Remove(_sourceItem);
                }
                else
                {
                    ParentItem.Items.Remove(_sourceItem);
                }
            }
            catch
            {

            }
        }

        public void addChild(TreeViewItem _sourceItem, TreeViewItem _targetItem)
        {
            // add item in target TreeViewItem 
            TreeViewItem item1 = new TreeViewItem();
            item1.Header = _sourceItem.Header;
            _targetItem.Items.Add(item1);
            foreach (TreeViewItem item in _sourceItem.Items)
            {
                addChild(item, item1);
            }
        }

        static TObject FindVisualParent<TObject>(UIElement child) where TObject : UIElement
        {
            if (child == null)
            {
                return null;
            }

            UIElement parent = VisualTreeHelper.GetParent(child) as UIElement;

            while (parent != null)
            {
                TObject found = parent as TObject;
                if (found != null)
                {
                    return found;
                }
                else
                {
                    parent = VisualTreeHelper.GetParent(parent) as UIElement;
                }
            }

            return null;
        }

        private TreeViewItem GetNearestContainer(UIElement element)
        {
            // Walk up the element tree to the nearest tree view item.
            TreeViewItem container = element as TreeViewItem;
            while ((container == null) && (element != null))
            {
                element = VisualTreeHelper.GetParent(element) as UIElement;
                container = element as TreeViewItem;
            }
            return container;
        }
    }
}
